using AutoMapper;
using CommonUtil;
using CommonUtil.RandomIdUtil;
using CommonUtil.YoloUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualBasic.CompilerServices;
using Model;
using Model.Dto.photo;
using Model.Dto.Yolo;
using Model.Entities;
using Model.Other;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
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
    private readonly UserInformationUtil _informationUtil;

    private static readonly string? BasePath =
        Directory.GetCurrentDirectory();

    public YoloService(MyDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor,
        UserInformationUtil informationUtil)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _informationUtil = informationUtil;
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

        string base64 = po.Photo.Substring(po.Photo.IndexOf(',') + 1);
        byte[] data = Convert.FromBase64String(base64);
        using (MemoryStream ms = new MemoryStream(data))
        {
            var sbjgCount = 0;
            var result = "";
            using (var image = Image.Load<Rgba32>(ms))
            {
                if (aiModels.ModelCls == "图像分类")
                {
                    using var yolo = new Yolo(Path.Combine(BasePath, aiModels.Path), false);
                    var runClassification = yolo.RunClassification(image);
                    if (runClassification[0].Confidence < 0.8)
                    {
                        result = "未识别出来！";
                    }
                    else
                    {
                        if (aiModels.ModelName == "动物识别")
                        {
                            result = YoloClassAnimalUtil.GetAnimalName(runClassification[0].Label);
                        }
                        else
                        {
                            result = runClassification[0].Label;
                        }
                    }
                }

                if (aiModels.ModelCls == "目标监测")
                {
                    using var yolo = new Yolo(Path.Combine(BasePath, aiModels.Path), false);
                    var results = yolo.RunObjectDetection(image, 0.3);
                    image.Draw(results);
                    sbjgCount = results.Count;
                }

                if (aiModels.ModelCls == "其他模型")
                {
                    return "暂未开放";
                }


                using (MemoryStream outputMs = new MemoryStream())
                {
                    image.Save(outputMs, new JpegEncoder());
                    byte[] imageBytes = outputMs.ToArray();
                    string base64Image = Convert.ToBase64String(imageBytes);

                    var yolotbs = new YoLoTbs()
                    {
                        Cls = aiModels.ModelCls,
                        Name = aiModels.ModelName,
                        SbJgCount = sbjgCount,
                        SbJg = result,
                        IsManualReview = false,
                        SbZqCount = 0,
                        RgMsCount = 0,
                        Zql = 0,
                        Zhl = 0,
                        CreateDate = DateTime.Now,
                        CreateUserId = createUserId,
                        IsDeleted = 0
                    };
                    yolotbs.Photos = new Photos() { PhotoBase64 = "data:image/jpeg;base64," + base64Image };
                    _context.YoLoTbs.Add(yolotbs);
                    _context.SaveChanges();
                    if (aiModels.ModelCls == "目标监测")
                    {
                        return "data:image/jpeg;base64," + base64Image;
                    }
                    else
                    {
                        return result;
                    }
                }
            }
        }
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