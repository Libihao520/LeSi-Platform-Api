namespace Model.Dto.Yolo;

public class YoloDetectionPutReq
{
    /// <summary>
    /// 主键
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 类别
    /// </summary>
    public string Cls { get; set; }

    /// <summary>
    /// 识别结果数量
    /// </summary>
    public int sbjgCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 照片(base 64)
    /// </summary>
    public string Photo { get; set; }
    
    /// <summary>
    /// 是否人工审核
    /// </summary>
    public bool isManualReview { get; set; }
}