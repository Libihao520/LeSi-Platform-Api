namespace Model.Consts;

public static class RedisKey
{
    /// <summary>
    /// 用户的激活码
    /// </summary>
    public const string UserActiveCode = "UserActiveCode{0}";

    /// <summary>
    /// 用户最近AI对话记录（参数0:用户ID）
    /// </summary>
    public const string UserAiRecentDialogs = "UserAIRecentDialogs:{0}";
}