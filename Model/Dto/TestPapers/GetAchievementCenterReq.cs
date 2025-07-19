using Model.Common;

namespace Model.Dto.TestPapers;

/// <summary>
/// 获取成绩
/// </summary>
public class GetAchievementCenterReq : Paging
{
    /// <summary>
    /// 科目
    /// </summary>
    public string? Subject { get; set; }
}