namespace Model.SignaIR;

public class RecognitionResult
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public long TaskId { get; set; }

    public string FrontendTaskId { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 识别结果
    /// </summary>
    public string SbJg { get; set; }
}