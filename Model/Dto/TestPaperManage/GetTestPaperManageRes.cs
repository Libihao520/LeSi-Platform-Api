namespace Model.Dto.TestPaperManage;

public class GetTestPaperManageRes
{
    /// <summary>
    /// 主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreateName { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 卷名
    /// </summary>
    public string? FileLabel { get; set; }
    
    /// <summary>
    /// 是否开始作答
    /// </summary>
    public bool HasAnsweringStarted { get; set; }

    /// <summary>
    /// 科目
    /// </summary>
    public string? QuestionBankCourseTitle { get; set; }

    /// <summary>
    /// 地址
    /// </summary>
    public string? Path { get; set; }
}