using Model.Common;

namespace Model.Dto.Yolo;

public class YoloDetectionQueryReq : Paging
{
    /// <summary>
    /// 模型类型
    /// </summary>
    public string? ModelCls { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string? ModelName { get; set; }
    
    /// <summary>
    /// 是否人工审核
    /// </summary>
    public int isaudit { get; set; }
}