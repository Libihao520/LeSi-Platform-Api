using EFCoreMigrations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Model;
using Model.Other;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly MyDbContext _context;

    public QuestionsController(MyDbContext context)
    {
        _context = context;
    }

    // 获取所有题目
    [HttpGet]
    public async Task<ApiResult> GetQuestions()
    {
        var listAsync = await _context.Questions.ToListAsync();
        return ResultHelper.Success("获取成功！", listAsync);
    }

    // 根据ID获取题目
    [HttpGet("{id}")]
    public async Task<ActionResult<Question>> GetQuestion(int id)
    {
        var question = await _context.Questions.FindAsync(id);

        if (question == null)
        {
            return NotFound();
        }

        return question;
    }
}