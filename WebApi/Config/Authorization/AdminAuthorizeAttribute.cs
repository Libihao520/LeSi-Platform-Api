using Microsoft.AspNetCore.Mvc;
using Model.Enum;

namespace WebApi.Config.Authorization;

public class AdminAuthorizeAttribute: TypeFilterAttribute
{
    private AuthorizeRoleName Role { get; set; }
    public string ErrorMessage { get; set; }
    
    public AdminAuthorizeAttribute(AuthorizeRoleName role, string errorMessage) : base(typeof(AdminAuthorizeFilter))
    {
        Role = role;
        ErrorMessage = errorMessage;
        Arguments = new object[] { role, errorMessage };
    }
}