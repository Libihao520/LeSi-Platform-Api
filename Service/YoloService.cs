using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using AutoMapper;
using CommonUtil;
using CommonUtil.RandomIdUtil;
using CommonUtil.YoloUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic.CompilerServices;
using Model;
using Model.Dto.photo;
using Model.Dto.Yolo;
using Model.Entities;
using Model.Other;
using Model.SignaIR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Service.SignalR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using YoloDotNet;
using YoloDotNet.Extensions;

namespace Service;

public class YoloService : IYoloService
{
    private readonly IMapper _mapper;
    private MyDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConnection _rabbitMqConnection;
    private readonly UserInformationUtil _informationUtil;
    private readonly ILogger<YoloService> _logger;
    private readonly IHubContext<RecognitionHub> _hubContext;
    private readonly ConcurrentDictionary<long, string> _connectionMap = new ConcurrentDictionary<long, string>();

    private static readonly string? BasePath =
        Directory.GetCurrentDirectory();

    public YoloService(MyDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        UserInformationUtil informationUtil, IConnection rabbitMqConnection, ILogger<YoloService> logger,
        IHubContext<RecognitionHub> hubContext)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _informationUtil = informationUtil;
        _rabbitMqConnection = rabbitMqConnection;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <returns></returns>
    public async Task<ApiResult> getpkqTb(YoloDetectionQueryReq req)
    {
        IQueryable<YoLoTbs> yolotb = _context.YoLoTbs.Where(p => p.IsDeleted == 0).OrderByDescending(q => q.CreateDate);
        //筛选条件
        if (req.ModelCls != "全部")
        {
            yolotb = yolotb.Where(p => p.Cls == req.ModelCls);
        }

        if (!string.IsNullOrEmpty(req.ModelName))
        {
            yolotb = yolotb.Where(p => p.Name.Contains(req.ModelName));
        }

        if (req.isaudit != 0)
        {
            yolotb = yolotb.Where(p => p.IsManualReview == (req.isaudit == 1 ? true : false));
        }

        var total = await yolotb.CountAsync();

        var paginatedResult = await yolotb
            .Skip((req.PageNum - 1) * req.PageSize) // 跳过前面的记录  
            .Take(req.PageSize) // 取接下来的指定数量的记录  
            .ToListAsync(); // 转换为列表  

        var yoloPkqResList = _mapper.Map<List<YoloPkqRes>>(paginatedResult);

        foreach (var yoloPkqRese in yoloPkqResList)
        {
            if (yoloPkqRese.CreateName != null)
                yoloPkqRese.CreateName =
                    await _informationUtil.GetUserNameByIdAsync(long.Parse(yoloPkqRese.CreateName));
        }

        return ResultHelper.Success("获取成功！", yoloPkqResList, total);
    }

    public async Task<string> PutPhoto(PhotoAddDto po, CancellationToken cancellationToken)
    {
        var user = _httpContextAccessor.HttpContext.User;
        var createUserId = long.Parse(user.Claims.FirstOrDefault(c => c.Type == "Id").Value);

        var aiModels = await _context.AiModels.FindAsync(po.ModelId, cancellationToken);
        if (aiModels == null || string.IsNullOrEmpty(aiModels.Path))
        {
            return "模型不存在，或地址异常";
        }

        // 生成唯一任务ID
        var taskId = TimeBasedIdGeneratorUtil.GenerateId();
        _connectionMap.TryAdd(taskId, po.connectionId);
        // 生成回调队列名称
        var callbackQueue = $"callback_queue_{taskId}";

        var message = new
        {
            TaskId = taskId,
            FrontendTaskId = po.taskId,
            ModelCls = aiModels.ModelCls,
            ModelName = aiModels.ModelName,
            Photo = po.Photo,
            Path = aiModels.Path,
            CallbackQueue = callbackQueue // 添加回调队列
        };
        // 发送到RabbitMQ
        using (var channel = _rabbitMqConnection.CreateModel())
        {
            channel.QueueDeclare(queue: "ai_yolo_recognition_tasks",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                routingKey: "ai_yolo_recognition_tasks",
                basicProperties: properties,
                body: body);
        }

        _logger.LogInformation($"Task {taskId} submitted for processing");

        StartCallbackConsumer(callbackQueue, taskId);

        string base64 = po.Photo.Substring(po.Photo.IndexOf(',') + 1);
        byte[] data = Convert.FromBase64String(base64);

        var yolotbs = new YoLoTbs()
        {
            Id = taskId,
            Cls = aiModels.ModelCls,
            Name = aiModels.ModelName,
            SbJgCount = 0,
            SbJg = "识别中...",
            IsManualReview = false,
            SbZqCount = 0,
            RgMsCount = 0,
            Zql = 0,
            Zhl = 0,
            CreateDate = DateTime.Now,
            CreateUserId = createUserId,
            IsDeleted = 0
        };
        yolotbs.Photos = new Photos() { PhotoBase64 = po.Photo };
        _context.YoLoTbs.Add(yolotbs);
        _context.SaveChanges();


        return "任务已提交，处理中...";
    }

    // 监听回调队列的临时消费者
    private void StartCallbackConsumer(string callbackQueue, long taskId)
    {
        var channel = _rabbitMqConnection.CreateModel();
        channel.QueueDeclare(callbackQueue, durable: false, exclusive: true, autoDelete: true);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var result = JsonSerializer.Deserialize<RecognitionResult>(Encoding.UTF8.GetString(body));

            // 通过 SignalR 推送结果
            if (_connectionMap.TryGetValue(taskId, out var connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveRecognitionResult", result);
                _connectionMap.TryRemove(taskId, out _);
            }

            channel.BasicAck(ea.DeliveryTag, false);
            channel.Close(); // 处理完成后关闭临时队列
        };

        channel.BasicConsume(callbackQueue, autoAck: false, consumer);
    }

    public async Task<YoloPkqEditRes> GetPkqEdtTb(long id)
    {
        var yoloTb = await _context.YoLoTbs.FindAsync(id);
        if (yoloTb == null)
        {
            return null;
        }

        var yoloPkqEditRes = _mapper.Map<YoloPkqEditRes>(yoloTb);
        var photos = _context.Photos.Where(u => u.PhotosId == yoloTb.PhotosId).FirstOrDefault();
        yoloPkqEditRes.Photo = photos?.PhotoBase64;
        return yoloPkqEditRes;
    }

    public async Task<YoloSjdpRes> Getsjdp()
    {
        var userCount = await _context.Users.CountAsync();
        var sbcsCount = await _context.YoLoTbs.CountAsync();
        var mbslCount = await _context.YoLoTbs.SumAsync(x => x.SbJgCount);
        var yoloSjdpRes = new YoloSjdpRes()
        {
            userCount = userCount,
            sbcsCount = sbcsCount,
            mbslCount = mbslCount,
            yxslCount = mbslCount,
        };
        return yoloSjdpRes;
    }

    #region 添加

    public async Task<ApiResult> AddDataTb(YoloDetectionPutReq req)
    {
        // ID为空时新增，ID存在时更新数据
        if (req.Id == null)
        {
            //数据校验
            if (string.IsNullOrEmpty(req.Cls))
            {
                return ResultHelper.Error("类别不可为空!");
            }

            if (req.sbjgCount < 0)
            {
                return ResultHelper.Error("数量不可为空!");
            }

            if (string.IsNullOrEmpty(req.Photo))
            {
                return ResultHelper.Error("照片不可为空!");
            }

            //将参数映射入实体类
            var yoloRes = _mapper.Map<YoLoTbs>(req);
            var generateId = TimeBasedIdGeneratorUtil.GenerateId();
            yoloRes.Id = generateId;
            var photId = TimeBasedIdGeneratorUtil.GenerateId();
            yoloRes.PhotosId = photId;
            var photos = new Photos()
            {
                PhotosId = photId,
                PhotoBase64 = req.Photo
            };
            _context.YoLoTbs.Add(yoloRes);
            _context.Photos.Add(photos);
        }
        // 否则更新
        else
        {
            var findAsync = await _context.YoLoTbs.FindAsync(req.Id);
            if (findAsync != null)
            {
                if (findAsync.PhotosId != null)
                {
                    var photos = await _context.Photos.FindAsync(findAsync.PhotosId);
                    photos.PhotoBase64 = req.Photo;
                }
                else
                {
                    var photId = TimeBasedIdGeneratorUtil.GenerateId();
                    findAsync.PhotosId = photId;
                    var photos = new Photos()
                    {
                        PhotosId = photId,
                        PhotoBase64 = req.Photo
                    };
                    _context.Photos.Add(photos);
                }

                _mapper.Map(req, findAsync);
            }
            else
            {
                return ResultHelper.Error("更新失败，数据不存在!");
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            return ResultHelper.Success("请求成功", "目标监测数据添加或修改成功！");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return ResultHelper.Error("系统异常!");
        }
    }

    #endregion

    #region 删除

    public async Task<ApiResult> DeleteAsync(long id)
    {
        try
        {
            // 根据id查找yoloTb对象
            var yoloTb = await _context.YoLoTbs.FindAsync(id);
            // 不为空则执行软删除并且保存到数据库中
            if (yoloTb != null)
            {
                yoloTb.IsDeleted = 1;
                await _context.SaveChangesAsync();
            }

            return ResultHelper.Success("请求成功！", "数据已删除");
        }
        catch (Exception e)
        {
            return ResultHelper.Error("yolo数据删除失败");
        }
    }

    #endregion


    #region 查询

    public async Task<YoLoTbs?> GetByIdAsync(int id)
    {
        var findAsync = _context.YoLoTbs.FindAsync(id);
        return await findAsync;
    }

    #endregion
}