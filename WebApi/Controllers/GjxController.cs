using Interface;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Other;
using WebApi.Config;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class GjxController : ControllerBase
{
    private IGjxService _gjxService;

    public GjxController(IGjxService gjxService)
    {
        _gjxService = gjxService;
    }

    /// <summary>
    /// 获取二维码
    /// </summary>
    /// <param name="txt"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> ewm(string txt)
    {
        return ResultHelper.Success("获取成功！", _gjxService.GetEwm(txt));
    }
}