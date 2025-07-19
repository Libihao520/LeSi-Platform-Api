using System.ComponentModel.DataAnnotations;
using Model.Common;
using Model.Enum;

namespace Model.Entities;

public class Users : Entity
{
    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string PassWord { get; set; }

    /// <summary>
    /// 用户角色
    /// </summary>
    public AuthorizeRoleName Role { get; set; } = AuthorizeRoleName.Ordinary;

    /// <summary>
    /// 邮箱
    /// </summary>
    [Required]
    public string Email { get; set; }

    /// <summary>
    /// 头像id
    /// </summary>
    public long? PhotosId { get; set; }
}