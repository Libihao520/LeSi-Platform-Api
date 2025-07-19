namespace Model.Dto.TestPapers;

public class AchievementCenterRes
{
    public long Id { get; set; }

    /// <summary>
    /// 科目
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 总分
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreateName { get; set; }

    /// <summary>
    /// 总题目数量
    /// </summary>
    public int NumberOfQuestions { get; set; }

    /// <summary>
    /// 答对数量
    /// </summary>
    public int CorrectQuantity { get; set; }
}