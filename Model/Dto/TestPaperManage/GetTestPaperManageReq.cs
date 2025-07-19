using Microsoft.AspNetCore.Http;
using Model.Common;

namespace Model.Dto.TestPaperManage;

public class GetTestPaperManageReq : Paging
{
    /// <summary>
    /// 卷名
    /// </summary>
    public string? FileLabel { get; set; }

    /// <summary>
    /// 科目
    /// </summary>
    public string? QuestionBankCourseTitle { get; set; }
}