namespace Model.Dto.Yolo;

public class YoloPkqEditRes
{
    /// <summary>
    /// 主键
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 类别
    /// </summary>
    public string Cls { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 识别结果数量
    /// </summary>
    public int sbjgCount { get; set; }

    /// <summary>
    /// 识别结果
    /// </summary>
    public string? SbJg { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreateName { get; set; }

    /// <summary>
    /// 照片(base 64)
    /// </summary>
    public string Photo { get; set; }

    /// <summary>
    /// 是否人工审核
    /// </summary>
    public bool IsManualReview { get; set; }

    /// <summary>
    /// 识别正确数量
    /// </summary>
    public int sbzqCount { get; set; }

    /// <summary>
    /// 人工目视数量
    /// </summary>
    public int rgmsCount { get; set; }

    /// <summary>
    /// 准确率
    /// </summary>
    public double zql { get; set; }

    /// <summary>
    /// 召回率
    /// </summary>
    public double zhl { get; set; }
}