namespace CommonUtil.RedisUtil;

/// <summary>
/// 所有开发人员redis使用的Key前缀
/// </summary>
public static class RedisKeyPrefix
{
    /// <summary>
    /// 会话
    /// </summary>
    public const string RedisSession = "RedisSession";
    /// <summary>
    /// 持久化
    /// </summary>
    public const string Permission = "Permission";

    /// <summary>
    /// 缓存及临时数据
    /// </summary>
    public const string RedisTempData = "RedisTempData";

    /// <summary>
    /// 无前缀
    /// </summary>
    public const string Empty = "";
}