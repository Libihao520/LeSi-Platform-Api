using Model.Common;

namespace Model.Dto.Role;

/// <summary>
/// 角色管理获取用户请求
/// </summary>
public class GetUserRoleReq : Paging
{
    /// <summary>
    /// 用户id
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string? username { get; set; }
}