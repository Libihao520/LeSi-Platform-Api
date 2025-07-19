namespace Model.UtilData;

public class AiRequestData
{
    /// <summary>
    /// 模型版本
    /// </summary>
    public string model { get; set; } = "generalv3.5";

    /// <summary>
    /// 用户提问数据
    /// </summary>
    public List<message> messages { get; set; }

    /// <summary>
    ///是否开启流式传输
    /// </summary>
    public bool stream { get; set; } = false;
}

public class message
{
    public string role { get; set; } = "user";
    public string content { get; set; }
}