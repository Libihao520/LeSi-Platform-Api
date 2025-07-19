using ProtoBuf;

namespace CommonUtil.RedisUtil;

/// <summary>
///  序列化工具类
/// </summary>
public static class ProtoBufUtil
{
    // 安装包：protobuf-net 3.2
    
    /// <summary>
    /// 序列化指定对象
    /// </summary>
    /// <typeparam name="T">对象类型,必须标示[ProtoContract]等</typeparam>
    /// <param name="obj">对象</param>
    /// <returns>byte[]</returns>
    public static byte[] Serialize<T>(T obj)

    {
        using var memoryStream = new MemoryStream();
        Serializer.Serialize(memoryStream, obj);
        byte[] byteArray = memoryStream.ToArray();
        return byteArray;
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T">对象类型,必须标示[ProtoContract]等</typeparam>
    /// <param name="byteArray">二进制</param>
    /// <returns>对象</returns>
    public static T? DeSerialize<T>(byte[]? byteArray)
    {
        if (byteArray==null||byteArray.Length==0)
        {
            return default(T);
        }

        using var memoryStream = new MemoryStream(byteArray);
        var obj = Serializer.Deserialize<T>(memoryStream);
        return obj;
    }
}