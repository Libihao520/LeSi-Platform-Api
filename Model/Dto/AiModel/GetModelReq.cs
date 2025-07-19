using Model.Common;

namespace Model.Dto.AiModel;

public class GetModelReq : Paging
{
    /// <summary>
    /// 模型类型
    /// </summary>
    public string? ModelCls { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }
}