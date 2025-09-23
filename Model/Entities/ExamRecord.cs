using Model.Common;

public class ExamRecord : Base
{
    public long UserId { get; set; } // 添加用户ID
    public long QuestionId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string? StudentOutput { get; set; }
    public bool IsCorrect { get; set; }
    public int Score { get; set; }

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}