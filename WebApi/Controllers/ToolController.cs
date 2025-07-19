using CommonUtil;
using EFCoreMigrations;
using Microsoft.AspNetCore.Mvc;
using Model.Entities;
using Model.Enum;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class ToolController : ControllerBase
{
    private MyDbContext _context;

    public ToolController(MyDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 初始化值
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public string InitDateBase()
    {
        //用户
        var users = new List<Users>()
        {
            new Users
            {
                Id = 1823648887341056,
                Name = "lbhlbh",
                PassWord = AesUtilities.Encrypt("123456"),
                Email = AesUtilities.Encrypt("1074775781@qq.com"),
                Role = AuthorizeRoleName.Administrator,
                CreateUserId = 001,
                CreateDate = DateTime.Now,
                IsDeleted = 0
            },
            new Users
            {
                Id = 1823648984858624,
                Name = "yhjyhj",
                PassWord = AesUtilities.Encrypt("123456"),
                Email = AesUtilities.Encrypt("1074775782@qq.com"),
                Role = AuthorizeRoleName.Administrator,
                CreateUserId = 001,
                CreateDate = DateTime.Now,
                IsDeleted = 0
            }
        };
        var aiModels = new List<AiModels>()
        {
            new AiModels()
            {
                Id = 1813709254033409,
                Path = @$"Model/pkq.onnx",
                ModelCls = "目标监测",
                ModelName = "皮卡丘识别",
                ModelSize = 43,
                CreateUserId = 1823648887341056,
                CreateDate = DateTime.Now,
                IsDeleted = 0
            },
            new AiModels()
            {
                Id = 1813709254033410,
                Path = @$"Model/animal.onnx",
                ModelCls = "图像分类",
                ModelName = "动物识别",
                ModelSize = 214,
                CreateUserId = 1823648887341056,
                CreateDate = DateTime.Now,
                IsDeleted = 0
            }
        };

        _context.Users.AddRange(users);
        _context.AiModels.AddRange(aiModels);
        _context.SaveChanges();


        return "ok";
    }
}