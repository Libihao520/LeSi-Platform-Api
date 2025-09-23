using Model.Dto.CodeExercises;
using Model.Other;

namespace Interface;

public interface ICodeExercisesService
{
    /// <summary>
    /// 获取所有题目
    /// </summary>
    /// <returns></returns>
    Task<ApiResult> GetAllExercises();
    
    /// <summary>
    /// 提交or测试代码
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    Task<ApiResult> SubmitCode(CodeSubmissionRequest req);
}