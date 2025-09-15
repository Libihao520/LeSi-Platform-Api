using System.Diagnostics;
using CommonUtil;
using EFCoreMigrations;
using Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Other;

[ApiController]
[Route("api/[controller]")]
public class ExamController : ControllerBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserInformationUtil _informationUtil;
    private readonly IConfiguration _configuration;
    private readonly MyDbContext _context;
    private readonly ICodeExecutor _codeExecutor;

    public ExamController(MyDbContext context, IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
        UserInformationUtil informationUtil, ICodeExecutor codeExecutor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _informationUtil = informationUtil;
        _codeExecutor = codeExecutor;
        _configuration = configuration;
    }

    // 提交代码
    [HttpPost("submit")]
    public async Task<ApiResult> SubmitCode([FromBody] CodeSubmission submission)
    {
        //验证用户对话
        var user = _httpContextAccessor.HttpContext.User;
        var createUserId = long.Parse(user.Claims.FirstOrDefault(c => c.Type == "Id").Value);
        // 获取题目信息
        var question = await _context.Questions.FindAsync(submission.QuestionId);
        if (question == null)
        {
            return ResultHelper.Error("题目不存在");
        }

        // 运行代码并获取输出
        var output = await _codeExecutor.ExecutePython(submission.Code, question.ExampleInput);
        // // 检查输出是否正确
        bool isCorrect = output.Output.Equals(question.ExampleOutput.Trim(), StringComparison.OrdinalIgnoreCase);
        int score = isCorrect ? 25 : 0; // 假设每题10分

        if (submission.sub)
        {
            var existingRecord = await _context.ExamRecords
                .FirstOrDefaultAsync(er => er.UserId == createUserId && er.QuestionId == submission.QuestionId);
    
            if (existingRecord != null)
            {
                return ResultHelper.Error("您已经提交过该题目，不能重复提交");
            }

            // 保存考试记录
            var examRecord = new ExamRecord
            {
                UserId = createUserId,
                QuestionId = submission.QuestionId,
                StudentCode = submission.Code,
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
        var result = new ExamResult
        {
            IsCorrect = isCorrect,
            Score = score,
            Output = $"输入：\n{question.ExampleInput}\n输出：\n{outputOutput}\n期望输出：\n{question.ExampleOutput}",
            ExpectedOutput = question.ExampleOutput
        };
        return ResultHelper.Success("成功", result);
    }


    // 运行Python代码
    private async Task<string> RunPythonCode(string code, string input)
    {
        // 创建临时文件
        string tempDir = Path.Combine("/data/code-tmp", Path.GetRandomFileName());
        string scriptPath = Path.Combine(tempDir, "student_code.py");
        string inputPath = Path.Combine(tempDir, "input.txt");

        // 写入代码和输入
        await System.IO.File.WriteAllTextAsync(scriptPath, code);
        await System.IO.File.WriteAllTextAsync(inputPath, input);

        try
        {
            // 配置Python进程
            ProcessStartInfo start = new ProcessStartInfo
            {
                FileName = _configuration["PythonPath"] ?? "python", // 可从配置获取Python路径
                Arguments = $"\"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using Process process = Process.Start(start);
            if (process == null)
            {
                return "无法启动Python进程";
            }

            // 传递输入
            using StreamWriter sw = process.StandardInput;
            string inputContent = await System.IO.File.ReadAllTextAsync(inputPath);
            await sw.WriteAsync(inputContent);
            sw.Close();

            // 获取输出
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            // 清理临时文件
            System.IO.File.Delete(scriptPath);
            System.IO.File.Delete(inputPath);

            return string.IsNullOrEmpty(error) ? output : $"错误: {error}";
        }
        catch (Exception ex)
        {
            return $"执行错误: {ex.Message}";
        }
    }
}

// 提交模型
public class CodeSubmission
{
    public int QuestionId { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool sub { get; set; } = false; // 添加SessionId字段
}

// 考试结果模型
public class ExamResult
{
    public bool IsCorrect { get; set; }
    public int Score { get; set; }
    public string Output { get; set; } = string.Empty;
    public string ExpectedOutput { get; set; } = string.Empty;
}