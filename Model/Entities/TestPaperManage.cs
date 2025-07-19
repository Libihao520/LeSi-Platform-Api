using Model.Common;

namespace Model.Entities;

public class TestPapersManage : Entity
{
    
    /// <summary>
    /// 试卷地址
    /// </summary>
    public string? ExcelFilePath  { get; set; }

    /// <summary>
    /// 卷名
    /// </summary>
    public string? FileLabel { get; set; }

    /// <summary>
    /// 科目
    /// </summary>
    public string? QuestionBankCourseTitle  { get; set; }
    
    /// <summary>
    /// 是否开始作答
    /// </summary>
    public bool? HasAnsweringStarted { get; set; }
    
    /// <summary>
    /// 题库
    /// </summary>
    public List<TestPapers>? TestPapers { get; set; }
}