using System.IdentityModel.Tokens.Jwt;
using Interface;
using LeSi.Admin.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Dto.User;
using Model.Enum;
using Model.Other;
using WebApi.Config;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class LoginController : ControllerBase
{
    private IUserService _userService;
    private ICustomJwtService _jwtService;
    private readonly PublicKeyService.PublicKeyServiceClient _client;

    public LoginController(IUserService userService, ICustomJwtService jwtService,
        PublicKeyService.PublicKeyServiceClient client)
    {
        _userService = userService;
        _jwtService = jwtService;
        _client = client;
    }

    /// <summary>
    /// 登录接口
    /// </summary>
    /// <param name="getUserReq"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> GetToken(GetUserReq getUserReq)
    {
        var res = Task.Run(() =>
        {
            if (string.IsNullOrEmpty(getUserReq.UserName) || string.IsNullOrEmpty(getUserReq.PassWord))
            {
                return ResultHelper.Error("参数不能为空");
            }

            GetUserRes getUser = _userService.GetUser(getUserReq.UserName, getUserReq.PassWord);
            if (string.IsNullOrEmpty(getUser.Name))
            {
                return ResultHelper.Error("账号不存在，用户名或密码错误！");
            }

            return ResultHelper.Success("登录成功！", _jwtService.GetToken(getUser));
        });
        return await res;
    }

    /// <summary>
    /// 注册接口
    /// </summary>
    /// <param name="addUserReq"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> add(AddUserReq addUserReq)
    {
        return await _userService.Add(addUserReq);
    }

    /// <summary>
    /// 解析token
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<ApiResult> userinfo()
    {
        return await _userService.GetUserInfo();
    }

    /// <summary>
    /// 给邮箱发送验证码
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> SendVerificationCode([FromQuery] string email)
    {
        return await _userService.SendVerificationCode(email);
    }

    [HttpGet]
    public async Task<ApiResult> GetPublicKey()
    {
        var response = await _client.GetPublicKeyAsync(new GetPublicKeyRequest());
        return ResultHelper.Success("成功", response.PublicKey);
    }
}