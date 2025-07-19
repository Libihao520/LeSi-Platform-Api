using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using AutoMapper;
using Azure.Core;
using CommonUtil;
using CommonUtil.RandomIdUtil;
using CommonUtil.RedisUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Model;
using Model.Consts;
using Model.Dto.photo;
using Model.Dto.User;
using Model.Entities;
using Model.Enum;
using Model.Other;

namespace Service;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private MyDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly EmailUtil _emailUtil;
    private readonly UserInformationUtil _informationUtil;

    public UserService(IMapper mapper, MyDbContext context, IHttpContextAccessor httpContextAccessor,
        EmailUtil emailUtil, ILogger<UserService> logger, UserInformationUtil informationUtil)
    {
        _mapper = mapper;
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _emailUtil = emailUtil;
        _logger = logger;
        _informationUtil = informationUtil;
    }

    public GetUserRes GetUser(string userName, string passWord)
    {
        var users = _context.Users.Where(u => u.Name == userName && u.PassWord == passWord).FirstOrDefault();
        if (users != null)
        {
            return _mapper.Map<GetUserRes>(users);
        }

        return new GetUserRes();
    }

    public async Task<ApiResult> Add(AddUserReq addUserReq)
    {
        if (string.IsNullOrWhiteSpace(addUserReq.PassWord) || string.IsNullOrWhiteSpace(addUserReq.RePassWord))
        {
            return ResultHelper.Error("密码为空!");
        }

        if (addUserReq.PassWord != addUserReq.RePassWord)
        {
            return ResultHelper.Error("两次输入的密码不一致!");
        }

        if (string.IsNullOrWhiteSpace(addUserReq.Email))
        {
            return ResultHelper.Error("邮箱为空！");
        }

        if (string.IsNullOrWhiteSpace(addUserReq.UserName))
        {
            return ResultHelper.Error("用户名为空！");
        }

        if (_context.Users.Any(u => u.Name == addUserReq.UserName && u.IsDeleted == 0))
        {
            return ResultHelper.Error("用户名已被注册，请换一个！");
        }

        var password = AesUtilities.Decrypt(addUserReq.PassWord);
        var decodeEmail = AesUtilities.Decrypt(addUserReq.Email);

        if (!IsValidEmail(decodeEmail))
        {
            return ResultHelper.Error("请输入正确格式的邮箱!");
        }

        if (_context.Users.Any(u => u.Email == decodeEmail && u.IsDeleted == 0))
        {
            return ResultHelper.Error("该邮箱已经注册过了！");
        }

        var s = CacheManager.Get<string>(string.Format(RedisKey.UserActiveCode, decodeEmail));
        if (string.IsNullOrWhiteSpace(s))
        {
            return ResultHelper.Error("验证码还未发送或已失效，请再发送一次！");
        }

        if (addUserReq.Authcode != s)
        {
            return ResultHelper.Error("验证码错误！");
        }


        var users = _context.Users.Where(u => u.Name == addUserReq.UserName).FirstOrDefault();
        if (users == null)
        {
            Users user = new Users()
            {
                Name = addUserReq.UserName,
                PassWord = addUserReq.PassWord,
                Email = decodeEmail,
                CreateDate = DateTime.Now,
                CreateUserId = 0,
                IsDeleted = 0
            };
            _context.Users.Add(user);
            _context.SaveChanges();
            return ResultHelper.Success("注册成功！", "验证码正确！已注册成功！  ");
        }
        else
        {
            return ResultHelper.Success("注册失败！", "用户已存在");
        }
    }

    /// <summary>
    /// 发送邮箱验证码
    /// </summary>
    /// <returns></returns>
    public async Task<ApiResult> SendVerificationCode(string email)
    {
        var decodeEmail = AesUtilities.Decrypt(email);
        if (!IsValidEmail(decodeEmail))
        {
            return ResultHelper.Error("请输入正确格式的邮箱!");
        }

        if (_context.Users.Any(u => u.Email == decodeEmail && u.IsDeleted == 0))
        {
            return ResultHelper.Error("该邮箱已经注册过了！");
        }

        //查看缓存有没有这条key
        var exist = CacheManager.Exist(string.Format(RedisKey.UserActiveCode, decodeEmail));
        if (exist)
        {
            return ResultHelper.Error("该邮箱的上一条验证码还未失效,请查看您的邮箱继续激活！");
        }

        try
        {
            string randomId = RandomIdGenerator.GenerateRandomId(6);
            //发送邮箱
            _emailUtil.NetSendEmail($"欢迎注册AI识别系统,您的验证码是：{randomId},验证码有效期至-{DateTime.Now.AddMinutes(30)}", "AI识别系统注册",
                decodeEmail);
            //将验证码写入缓存，并设置过期时间
            CacheManager.Set(string.Format(RedisKey.UserActiveCode, decodeEmail), randomId, TimeSpan.FromMinutes(30));
        }
        catch (Exception e)
        {
            return ResultHelper.Error("邮箱发送失败，请稍后再试！");
        }

        return ResultHelper.Success("发送成功，尽快验证！", $"验证码已经发送到您的邮箱{decodeEmail}！有效期30分钟");
    }

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ApiResult> GetUserInfo()
    {
        var user = await _context.Users.FindAsync(_informationUtil.GetCurrentUserId());
        if (user == null)
        {
            return ResultHelper.Error("用户不存在！");
        }

        var tPhotos = await _context.Photos.FindAsync(user.PhotosId);
        var userRes = new GetUserRes()
        {
            Id = user.Id,
            Name = user.Name,
            Role = EnumConvert.ConvertRoleNameToString(user.Role),
            Photo = tPhotos?.PhotoBase64,
            CreateDate = user.CreateDate
        };
        return ResultHelper.Success("成功！", userRes);
    }

    public async Task<ApiResult> PutUserAvatar(PhotoAddDto po, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(_informationUtil.GetCurrentUserId(), cancellationToken);
        if (user == null)
        {
            return ResultHelper.Error("用户不存在！");
        }

        if (string.IsNullOrEmpty(po.Photo))
        {
            return ResultHelper.Error("图片不能为空！");
        }

        // 检查用户是否已经有照片，如果有则更新，没有则创建新的照片实体
        if (user.PhotosId == null)
        {
            var newPhotoId = TimeBasedIdGeneratorUtil.GenerateId();
            user.PhotosId = newPhotoId;

            var newPhoto = new Photos()
            {
                PhotosId = newPhotoId,
                PhotoBase64 = po.Photo
            };
            _context.Photos.Add(newPhoto);
        }
        else
        {
            // 用户已经有照片ID，更新现有照片
            var existingPhoto = await _context.Photos.FindAsync(user.PhotosId, cancellationToken);
            if (existingPhoto == null)
            {
                return ResultHelper.Error("找不到与用户关联的照片！");
            }

            existingPhoto.PhotoBase64 = po.Photo;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            return ResultHelper.Error("保存头像时出错：" + ex.Message);
        }

        return ResultHelper.Success("请求成功！", "头像更新成功");
    }

    /// <summary>
    /// 验证邮箱格式工具
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    static bool IsValidEmail(string email)
    {
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }
}