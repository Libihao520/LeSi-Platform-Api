public class User
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PassWord { get; set; } = string.Empty;
    public string? Email { get; set; }
    
    // 导航属性
    public List<ExamRecord> ExamRecords { get; set; } = new List<ExamRecord>();
}