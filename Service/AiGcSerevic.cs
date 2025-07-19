using System.Net.Http.Headers;
using System.Text;
using AutoMapper;
using Azure.Core;
using CommonUtil;
using CommonUtil.AiGcUtil;
using CommonUtil.RandomIdUtil;
using CommonUtil.RedisUtil;
using CommonUtil.RequesUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.ValueGeneration.Internal;
using Model;
using Model.Consts;
using Model.Dto.AiModel;
using Model.Entities;
using Model.Other;
using Model.UtilData;
using MySqlConnector;
using Newtonsoft.Json.Linq;

namespace Service;

public class AiGcSerevic : IAiGcService
{
    private MyDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserInformationUtil _informationUtil;
    private readonly AiRequestProcessor _aiRequestProcessor;

    public AiGcSerevic(MyDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        UserInformationUtil informationUtil, AiRequestProcessor aiRequestProcessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _informationUtil = informationUtil;
        _aiRequestProcessor = aiRequestProcessor;
    }

    public async Task<ApiResult> GetModelService(GetModelReq req)
    {
        try
        {
            var query = _context.AiModels.Where(q => q.IsDeleted == 0).AsQueryable();

            if (!string.IsNullOrEmpty(req.ModelName))
            {
                query = query.Where(m => m.ModelName.Contains(req.ModelName));
            }

            if (!string.IsNullOrEmpty(req.ModelCls) && req.ModelCls.ToUpper() != "全部")
            {
                query = query.Where(m => m.ModelCls == req.ModelCls);
            }

            var totalCount = await query.CountAsync();

            var paginatedResult = await query
                .Skip((req.PageNum - 1) * req.PageSize)
                .Take(req.PageSize)
                .ToListAsync();

            var resultList = _mapper.Map<List<GetModelRes>>(paginatedResult);
            foreach (var getModelRes in resultList)
            {
                if (getModelRes.CreateName != null)
                    getModelRes.CreateName =
                        await _informationUtil.GetUserNameByIdAsync(long.Parse(getModelRes.CreateName));
            }

            return ResultHelper.Success("查询成功", resultList, totalCount);
        }
        catch (Exception e)
        {
            Console.WriteLine("$发生异常");
            return ResultHelper.Error("查询异常，请稍后重试");
        }
    }

    public async Task<ApiResult> PutModelService(PutModelReq req)
    {
        //数据校验
        if (string.IsNullOrEmpty(req.ModelName))
        {
            return ResultHelper.Error("模型名称不能为空");
        }

        if (string.IsNullOrEmpty(req.ModelCls))
        {
            return ResultHelper.Error("模型类型不能为空");
        }

        var models = _context.AiModels.FirstOrDefault(a => a.ModelName == req.ModelName && a.IsDeleted == 0);
        if (models != null)
        {
            return ResultHelper.Error("模型已存在，请更换名称后重新添加");
        }

        // 流式上传
        if (!MultipartRequestHelper.HasFormFileContentDisposition(_httpContextAccessor.HttpContext.Request.ContentType))
        {
            return ResultHelper.Error("没有文件上传");
        }

        var formFile = _httpContextAccessor.HttpContext.Request.Form.Files[0];

        //缓冲方式
        // var formFile = req.Model;

        if (formFile.Length == 0)
        {
            return ResultHelper.Error("没有文件上传");
        }

        //判断后缀是否符合
        var allowExtensions = new[] { ".pt", ".onnx" };
        var fileExtension = Path.GetExtension(formFile.FileName).ToLowerInvariant();
        if (!allowExtensions.Contains(fileExtension))
        {
            return ResultHelper.Error($"不支持文件格式，仅支持{String.Join(",", allowExtensions)}格式的模型文件");
        }

        //相对路径
        var relativeFilePath = Path.Combine("Model",
            req.ModelName + "_" + TimeBasedIdGeneratorUtil.GenerateId() + "_" + formFile.FileName);
        // 绝对路径
        var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), relativeFilePath);

        var user = _httpContextAccessor?.HttpContext?.User;
        var createUserId = long.Parse(user.Claims.FirstOrDefault(c => c.Type == "Id").Value);
        var fileSizeInMb = (int)Math.Round(formFile.Length / (1024.0 * 1024.0));
        var aiModels = new AiModels()
        {
            ModelName = req.ModelName,
            ModelCls = req.ModelCls,
            Id = TimeBasedIdGeneratorUtil.GenerateId(),
            Path = relativeFilePath, // 设置文件路径到数据库记录中
            ModelSize = fileSizeInMb,
            CreateDate = DateTime.Now,
            IsDeleted = 0,
            CreateUserId = createUserId,
        };

        // 开启数据库事务
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            //创建文件流，创建模式打开目标文件，不存在则创建，存在则覆盖
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }

            var entityEntry = _context.AiModels.Add(aiModels);
            await _context.SaveChangesAsync();
            // 提交数据库事务
            await transaction.CommitAsync();
            return ResultHelper.Success("请求成功!", "模型添加成功");
        }
        catch (Exception ex)
        {
            // 回滚事务
            await transaction.RollbackAsync();
            // 异常，删除已部分保存的文件（如果有的话）
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }

            return ResultHelper.Error($"模型添加失败，原因：{ex.Message}");
        }
    }

    public async Task<ApiResult> DelModelService(long id)
    {
        // TODO 根据id，软删除模型----0未删除；1已删除
        try
        {
            var entity = await _context.AiModels.FindAsync(id);
            if (entity != null)
            {
                entity.IsDeleted = 1;
                await _context.SaveChangesAsync();
            }

            return ResultHelper.Success("请求成功！", "数据已删除");
        }
        catch (Exception e)
        {
            return ResultHelper.Error("删除模型出现异常，请稍后重试");
        }
    }

    public async Task<ApiResult> QuestionsAndAnswers(string q, string? model, CancellationToken cancellationToken)
    {
        object? result = null;
        if (!string.IsNullOrEmpty(model))
        {
            if (model.StartsWith("DeepSeek", StringComparison.OrdinalIgnoreCase))
            {
                result = await _aiRequestProcessor.DeepSeekProcess(q, model, cancellationToken);
            }
            else if (model.StartsWith("Spark", StringComparison.OrdinalIgnoreCase))
            {
                result = await _aiRequestProcessor.SparkProcess(q, cancellationToken);
            }
            else
            {
                return ResultHelper.Error("不支持的模型类型");
            }
        }
        else
        {
            // 默认
            result = await _aiRequestProcessor.SparkProcess(q, cancellationToken);
        }

        return ResultHelper.Success("请求成功！", result);
    }

    public async IAsyncEnumerable<string> QuestionsAndAnswersStream(string q, string model,
        CancellationToken cancellationToken)
    {
        if (model.StartsWith("DeepSeek", StringComparison.OrdinalIgnoreCase))
        {
            await foreach (var chunk in _aiRequestProcessor.DeepSeekProcessStreamAsync(q, model, cancellationToken))
            {
                yield return chunk;
            }
        }
        else if (model.StartsWith("Spark", StringComparison.OrdinalIgnoreCase))
        {
            await foreach (var chunk in _aiRequestProcessor.SparkProcessStreamAsync(q, cancellationToken))
            {
                yield return chunk;
            }
        }
        else
        {
            yield return "不支持的模型类型";
        }
    }

    public ApiResult GetHistoryService()
    {
        var userId = _informationUtil.GetCurrentUserId();
        var cacheKey = string.Format(RedisKey.UserAiRecentDialogs, userId);

        var Messages = CacheManager.Exist(cacheKey)
            ? CacheManager.Get<List<message>>(cacheKey)
            : new List<message>();

        return ResultHelper.Success("请求成功", Messages);
    }

    public ApiResult DelHistoryService()
    {
        var userId = _informationUtil.GetCurrentUserId();
        var cacheKey = string.Format(RedisKey.UserAiRecentDialogs, userId);
        if (CacheManager.Exist(cacheKey))
            CacheManager.Remove(cacheKey);
        return ResultHelper.Success("请求成功", "成功移除缓存");
    }
}