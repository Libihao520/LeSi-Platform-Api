using Model.Common;

namespace Model.Entities;

public class Photos
{
    public long PhotosId { get; set; }

    /// <summary>
    /// 照片(base 64)
    /// </summary>
    public string? PhotoBase64 { get; set; }
}