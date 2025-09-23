using Google.Protobuf.WellKnownTypes;
using Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model.Dto.CodeExercises;
using Model.Other;

namespace WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]/[action]")]
public class CodeExercisesController : ControllerBase
{
    private readonly ICodeExercisesService _codeExercisesService;

    public CodeExercisesController(ICodeExercisesService codeExercisesService)
    {
        _codeExercisesService = codeExercisesService;
    }

    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public Task<ApiResult> GetAllExercises()
    {
        return _codeExercisesService.GetAllExercises();
    }


    /// <summary>
    /// 提交or测试代码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost]
    public Task<ApiResult> SubmitCode([FromBody] CodeSubmissionRequest req)
    {
        return _codeExercisesService.SubmitCode(req);
    }
}