using System.ComponentModel.DataAnnotations;
using Model.Enum;

namespace Model.Dto.User;

public class GetUserRes
{
    /// <summary>
    /// id
    /// </summary>
    [Required]
    public long Id { get; set; }

    /// <summary>
    /// 创建人Id
    /// </summary>
    [Required]
    public long CreateUserId { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    [Required]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public int IsDeleted { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [Required]
    public string? PassWord { get; set; }

    /// <summary>
    /// 用户角色
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// 照片(base 64)
    /// </summary>
    public string? Photo { get; set; }
}