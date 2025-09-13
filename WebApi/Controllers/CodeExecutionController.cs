using Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Dto.CodeExecution;
using Model.Other;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
[Authorize]
public class CodeExecutionController : ControllerBase
{
    private readonly ICodeExecutor _codeExecutor;

    public CodeExecutionController(ICodeExecutor codeExecutor)
    {
        _codeExecutor = codeExecutor;
    }

    [HttpPost]
    public async Task<ApiResult> ExecuteJava([FromBody] CodeExecutionRequest request)
    {
        string input = "";
        if (request.Language == "java")
        {
            var result = await _codeExecutor.ExecuteJava(request.Code, input);
            return ResultHelper.Success("成功", result);
        }
        else if (request.Language == "python")
        {
            var result = await _codeExecutor.ExecutePython(request.Code, input);
            return ResultHelper.Success("成功", result);
        }
        else if (request.Language == "cpp")
        {
            var result = await _codeExecutor.ExecuteCpp(request.Code, input);
            return ResultHelper.Success("成功", result);
        }
        else
        {
            return ResultHelper.Error("不支持的语言");
        }
    }
}