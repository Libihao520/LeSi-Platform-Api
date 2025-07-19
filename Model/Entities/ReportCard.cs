using Model.Common;
using Newtonsoft.Json.Linq;

namespace Model.Entities;

/// <summary>
/// 成绩
/// </summary>
public class ReportCard : Entity
{
    /// <summary>
    /// 科目
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// 总分
    /// </summary>
    public int TotalPoints { get; set; }

    /// <summary>
    /// 总题目数量
    /// </summary>
    public int NumberOfQuestions { get; set; }

    /// <summary>
    /// 答对数量
    /// </summary>
    public int CorrectQuantity { get; set; }

    /// <summary>
    /// 提交的答案
    /// </summary>
    public string? SubmittedOptions { get; set; }

    /// <summary>
    /// 试卷ID
    /// </summary>
    public long? TestPapersManageId { get; set; }
}