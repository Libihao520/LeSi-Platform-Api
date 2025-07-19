using Microsoft.AspNetCore.Http;

namespace Model.Dto.TestPaperManage;

public class AddTestPaperManageReq
{
    /// <summary>
    /// 卷名
    /// </summary>
    public string? FileLabel { get; set; }

    /// <summary>
    /// 科目
    /// </summary>
    public string? QuestionBankCourseTitle { get; set; }

    /// <summary>
    ///Excel文件
    /// </summary>
    public IFormFile? File { get; set; }
}