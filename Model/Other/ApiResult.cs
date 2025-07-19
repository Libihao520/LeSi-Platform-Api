namespace Model.Other;

/// <summary>
/// 统一返回模型
/// </summary>
public class ApiResult
{
    /// <summary>
    /// 业务状态码
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// 响应消息
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// 响应数据
    /// </summary>
    public object data { get; set; }

    /// <summary>
    /// 数据总条数
    /// </summary>
    public int? total { get; set; }
}