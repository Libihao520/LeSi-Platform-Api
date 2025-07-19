namespace Model.Dto.Yolo;

public class YoloSjdpRes
{
    /// <summary>
    /// 注册用户
    /// </summary>
    public int userCount { get; set; }
    /// <summary>
    /// 识别次数
    /// </summary>
    public int sbcsCount { get; set; }
    /// <summary>
    /// 目标数量
    /// </summary>
    public int mbslCount { get; set; }
    /// <summary>
    /// 有效数量
    /// </summary>
    public int yxslCount { get; set; }
}