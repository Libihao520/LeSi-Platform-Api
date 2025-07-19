using Microsoft.AspNetCore.Mvc;
using Model.Enum;

namespace WebApi.Config.Authorization;

public class AdminAuthorizeAttribute: TypeFilterAttribute
{
    private AuthorizeRoleName Role { get; set; }
    public AdminAuthorizeAttribute(AuthorizeRoleName role) : base(typeof(AdminAuthorizeFilter))
    {
        Role = role;
        Arguments = new object[] { role };
    }
}