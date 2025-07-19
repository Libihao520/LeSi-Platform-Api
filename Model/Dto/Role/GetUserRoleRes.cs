namespace Model.Dto.Role;

public class GetUserRoleRes
{
    /// <summary>
    /// 主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>  
    /// 用户名  
    /// </summary>  
    public string? Name { get; set; }

    /// <summary>  
    /// 创建日期  
    /// </summary>  
    public DateTime CreateDate { get; set; }

    /// <summary>  
    /// 角色  
    /// </summary>  
    public string? Role { get; set; }

    /// <summary>  
    /// 电子邮件  
    /// </summary>  
    public string? Email { get; set; }
    
    /// <summary>
    /// 密码
    /// </summary>
    public string? PassWord { get; set; }
}