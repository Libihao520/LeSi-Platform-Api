using Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Dto.photo;
using Model.Dto.User;
using Model.Other;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPut]
    public async Task<ApiResult> PutUserAvatar(PhotoAddDto po, CancellationToken cancellationToken)
    {
        return await _userService.PutUserAvatar(po, cancellationToken);
    }
}