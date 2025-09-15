public class ExamRecord
{
    public long Id { get; set; }
    public long UserId { get; set; } // 添加用户ID
    public int QuestionId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string? StudentOutput { get; set; }
    public bool IsCorrect { get; set; }
    public int Score { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    // 导航属性
    public Question? Question { get; set; }
    public User? User { get; set; } // 添加用户导航属性
}