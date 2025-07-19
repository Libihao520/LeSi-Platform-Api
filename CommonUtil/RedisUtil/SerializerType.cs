namespace CommonUtil.RedisUtil;

public enum SerializerType
{
    Json,
    ProtoBuf
}
public class SuperObj
{
    public bool HasValue { get; set; }
    public object? Value { get; set; }
}