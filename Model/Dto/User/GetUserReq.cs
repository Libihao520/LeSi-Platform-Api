using System.ComponentModel.DataAnnotations;

namespace Model.Dto.User;

public class GetUserReq
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string? PassWord { get; set; }
}