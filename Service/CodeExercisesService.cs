using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Dto.CodeExercises;
using Model.Other;

namespace Service;

public class CodeExercisesService : ICodeExercisesService
{
    private readonly MyDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICodeExecutor _codeExecutor;

    public CodeExercisesService(MyDbContext context, IHttpContextAccessor httpContextAccessor,
        ICodeExecutor codeExecutor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _codeExecutor = codeExecutor;
    }

    public async Task<ApiResult> GetAllExercises()
    {
        var listAsync = await _context.Questions.ToListAsync();
        return ResultHelper.Success("获取成功！", listAsync);
    }

    public async Task<ApiResult> SubmitCode(CodeSubmissionRequest req)
    {
        //验证用户对话
        var user = _httpContextAccessor.HttpContext.User;
        var createUserId = long.Parse(user.Claims.FirstOrDefault(c => c.Type == "Id").Value);
        // 获取题目信息
        var question = await _context.Questions.FindAsync(req.QuestionId);
        if (question == null)
        {
            return ResultHelper.Error("题目不存在");
        }

        // 运行代码并获取输出
        var output = await _codeExecutor.ExecutePython(req.Code, question.ExampleInput);
        // // 检查输出是否正确
        bool isCorrect = output.Output.Equals(question.ExampleOutput.Trim(), StringComparison.OrdinalIgnoreCase);
        int score = isCorrect ? 25 : 0; // 假设每题10分

        if (req.IsPractice)
        {
            var existingRecord = await _context.ExamRecords
                .FirstOrDefaultAsync(er => er.UserId == createUserId && er.QuestionId == req.QuestionId);

            if (existingRecord != null)
            {
                return ResultHelper.Error("您已经提交过该题目，不能重复提交");
            }

            // 保存考试记录
            var examRecord = new ExamRecord
            {
                UserId = createUserId,
                QuestionId = req.QuestionId,
                StudentCode = req.Code,
                StudentOutput = output.Output,
                IsCorrect = isCorrect,
                Score = score
            };

            _context.ExamRecords.Add(examRecord);
            await _context.SaveChangesAsync();
            return ResultHelper.Success("提交成功", "提交成功");
        }

        var outputOutput = !string.IsNullOrEmpty(output.Output) ? output.Output : output.Error;
        // 返回结果
        var result = new CodeSubmissionResult
        {
            IsCorrect = isCorrect,
            Score = score,
            Output = $"输入：\n{question.ExampleInput}\n输出：\n{outputOutput}\n期望输出：\n{question.ExampleOutput}",
            ExpectedOutput = question.ExampleOutput
        };
        return ResultHelper.Success("成功", result);
    }
}