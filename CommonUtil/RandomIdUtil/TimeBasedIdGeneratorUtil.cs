namespace CommonUtil.RandomIdUtil;

/// <summary>
/// 生成随机Id
/// </summary>
public static class TimeBasedIdGeneratorUtil
{
    // 用于在同一秒内生成唯一 ID 的计数器  
    private static int _counter = 0;

    // 用于锁定计数器以确保线程安全  
    private static readonly object _lock = new object();

    // 上一次生成 ID 的时间戳（秒）  
    private static long _lastTimestamp = -1;

    public static long GenerateId()
    {
        long timestamp = GetTimestampInSeconds();

        lock (_lock)
        {
            if (timestamp == _lastTimestamp)
            {
                // 如果时间戳相同，则增加计数器  
                _counter++;
            }
            else
            {
                // 如果时间戳不同，重置计数器  
                _counter = 0;
                _lastTimestamp = timestamp;
            }

            // 返回生成的 ID，可以结合时间戳和计数器  
            return (timestamp << 20) | _counter; // 左移20位是为了给计数器留出足够的空间  
        }
    }

    private static long GetTimestampInSeconds()
    {
        return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
    }
}