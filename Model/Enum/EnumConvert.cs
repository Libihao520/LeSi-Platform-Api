namespace Model.Enum;

public class EnumConvert
{
    /// <summary>
    /// 枚举获取中文
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string ConvertRoleNameToString(AuthorizeRoleName role)
    {
        switch (role)
        {
            case AuthorizeRoleName.Administrator:
                return "超级管理员";
            case AuthorizeRoleName.Editor:
                return "编辑用户";
            case AuthorizeRoleName.Ordinary:
                return "普通用户";
            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }

    /// <summary>  
    /// 根据中文字符串获取枚举  
    /// </summary>  
    /// <param name="roleName"></param>  
    /// <returns></returns>  
    /// <exception cref="ArgumentException"></exception>  
    public static AuthorizeRoleName ConvertStringToRoleName(string roleName)
    {
        switch (roleName)
        {
            case "超级管理员":
                return AuthorizeRoleName.Administrator;
            case "编辑用户":
                return AuthorizeRoleName.Editor;
            case "普通用户":
                return AuthorizeRoleName.Ordinary;
            default:
                throw new ArgumentException($"未知的角色名称: {roleName}");
        }
    }
}