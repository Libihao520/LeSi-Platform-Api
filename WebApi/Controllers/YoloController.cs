using Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Dto.photo;
using Model.Dto.Yolo;
using Model.Other;
using WebApi.Config;

namespace WebApi.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize]
public class YoloController : ControllerBase
{
    private IYoloService _yoloService;

    public YoloController(IYoloService yoloService)
    {
        _yoloService = yoloService;
    }

    /// <summary>
    /// 获取AI识别表单数据
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> yolopkq([FromQuery] YoloDetectionQueryReq req)
    {
        return await _yoloService.getpkqTb(req);
    }

    /// <summary>
    /// 传入照片识别接口，返回照片
    /// </summary>
    /// <param name="po"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ApiResult> PutPhoto(PhotoAddDto po, CancellationToken cancellationToken)
    {
        return ResultHelper.Success("识别成功！", await _yoloService.PutPhoto(po, cancellationToken));
    }

    /// <summary>
    /// 获取表单数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> GetPkqEditTb(long id)
    {
        return ResultHelper.Success("获取成功！", await _yoloService.GetPkqEdtTb(id));
    }

    /// <summary>
    /// 获取数据大屏数据
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ApiResult> Getsjdp()
    {
        return ResultHelper.Success("获取成功！", await _yoloService.Getsjdp());
    }

    /// <summary>
    /// 根据id删除数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<ApiResult> Deleted(long id)
    {
        return await _yoloService.DeleteAsync(id);
    }

    /// <summary>
    /// 手动新增数据
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ApiResult> PutDataTb([FromBody] YoloDetectionPutReq req)
    {
        return await _yoloService.AddDataTb(req);
    }
}