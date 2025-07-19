using System.ComponentModel.DataAnnotations;
using Model.Common;

namespace Model.Entities;

public class YoLoTbs : Entity
{
    /// <summary>
    /// 类别
    /// </summary>
    [Required]
    public string? Cls { get; set; }

    /// <summary>
    /// 模型名称
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// 识别结果数量
    /// </summary>
    [Required]
    public int SbJgCount { get; set; }

    /// <summary>
    /// 识别结果
    /// </summary>
    public string? SbJg { get; set; }

    /// <summary>
    /// 是否人工审核
    /// </summary>
    [Required]
    public bool IsManualReview { get; set; }

    /// <summary>
    /// 识别正确数量
    /// </summary>
    [Required]
    public int SbZqCount { get; set; }

    /// <summary>
    /// 人工目视数量
    /// </summary>
    [Required]
    public int RgMsCount { get; set; }

    /// <summary>
    /// 准确率
    /// </summary>
    [Required]
    public double Zql { get; set; }

    /// <summary>
    /// 召回率
    /// </summary>
    [Required]
    public double Zhl { get; set; }

    public long PhotosId { get; set; }

    public Photos? Photos { get; set; }
}