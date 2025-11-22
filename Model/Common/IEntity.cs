using System.ComponentModel.DataAnnotations;

namespace Model.Common;

public abstract class Entity : Base
{
    /// <summary>
    /// 版本
    /// </summary>
    [Timestamp]
    public byte[] RowVersion { get; set; }

    /// <summary>
    /// 创建人Id
    /// </summary>
    [Required]
    public long CreateUserId { get; set; }

    /// <summary>
    /// 创建日期
    /// </summary>
    [Required]
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// 修改人Id
    /// </summary>
    public long? LastModifiedUserId { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    [Required]
    public int IsDeleted { get; set; }
}