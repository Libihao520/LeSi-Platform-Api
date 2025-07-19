using Model.Dto.photo;
using Model.Dto.User;
using Model.Other;

namespace Interface;

public interface IUserService
{
    /// <summary>
    /// 获取用户token
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="passWord"></param>
    /// <returns></returns>
    GetUserRes GetUser(string userName, string passWord);

    /// <summary>
    /// 添加用户
    /// </summary>
    /// <param name="addUserReq"></param>
    /// <returns></returns>
    Task<ApiResult> Add(AddUserReq addUserReq);

    /// <summary>
    /// 发送邮箱验证码
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<ApiResult> SendVerificationCode(string email);

    /// <summary>
    /// 获取用户信息
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> GetUserInfo();

    /// <summary>
    /// 更新头像
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> PutUserAvatar(PhotoAddDto po, CancellationToken cancellationToken);
}