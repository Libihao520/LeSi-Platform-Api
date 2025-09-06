using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Model.Enum;

namespace WebApi.Config.Authorization;

public class AdminAuthorizeFilter : IAuthorizationFilter
{
    private readonly string _errorMessage;

    private readonly AuthorizeRoleName _role;

    public AdminAuthorizeFilter(AuthorizeRoleName role, string errorMessage)
    {
        _errorMessage = errorMessage;
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 检查用户是否已认证  
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new ChallengeResult();
            return;
        }

        // 检查用户是否属于指定的角色通，常涉及到检查用户的Claims或其他安全令牌中的信息  
        // 我们有一个方法GetCurrentUserRole()来获取当前用户的角色  
        var userRole = GetCurrentUserRole(context.HttpContext);

        if (userRole != _role)
        {
            context.Result = new JsonResult(new
            {
                Code = 403,
                Message = _errorMessage,
                Data = (object)null
            })
            {
                StatusCode = 403
            };
        }
    }

    private AuthorizeRoleName GetCurrentUserRole(HttpContext context)
    {
        var user = context.User;
        var roleClaim = user.Claims.FirstOrDefault(c => c.Type == "RoleName");

        if (roleClaim != null)
        {
            // 将 Claim 的 Value 转换为 AuthorizeRoleName 枚举  
            // 这里需要实现一个转换逻辑，因为 Claim 的 Value 是字符串   
            if (Enum.TryParse<AuthorizeRoleName>(roleClaim.Value, true, out var role))
            {
                return role;
            }

            throw new InvalidOperationException("无法确定用户的角色!");
        }

        throw new InvalidOperationException("角色信息为空！");
    }
}