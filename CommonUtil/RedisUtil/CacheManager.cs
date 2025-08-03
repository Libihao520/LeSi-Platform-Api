using Microsoft.Extensions.Configuration;

namespace CommonUtil.RedisUtil;

public class CacheManager
{
    static readonly StackExchangeRedisWrapper RedisClient
            = new StackExchangeRedisWrapper(RedisKeyPrefix.Empty, 0, SerializerType.Json);
        
        static CacheManager()
        {
            // 需要安装的包：
            // Microsoft.Extensions.Configuration 6.0
            // Microsoft.Extensions.Configuration.Json 6.0
            ConfigurationBuilder configuration = new ConfigurationBuilder(); //读取配置文件
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true" ? "Docker" : "Development")}.json",
                    optional: true, 
                    reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
             RedisClient.StartConnect(config["Redis"]);
        }


        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>操作结果</returns>
        public static bool HashDelete(string key, string hashField)
        {
            return RedisClient.HashDelete(key, hashField);
        }
       

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <param name="value">对象</param>
        /// <returns>操作结果</returns>
        public static bool HashSet<T>(string key, string hashField, T value)
        {
            return RedisClient.HashSet(key, hashField, value);
        }

        

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public static T HashGet<T>(string key, string hashField)
        {
            return RedisClient.HashGet<T>(key, hashField);
        }
       
        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="hashField">hash域</param>
        /// <returns></returns>
        public static bool HashExists(string key, string hashField)
        {
            return RedisClient.HashExists(key, hashField);
        }

       

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> HashKeys(string key)
        {
            return RedisClient.HashKeys(key);
        }
        

        /// <summary>
        /// 设置缓存(记得尽可能设置过期时间 TimeSpan time)
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value(记得尽可能设置过期时间 TimeSpan time)</param>
        public static void Set(string key, object value)
        {
            RedisClient.StringSet(key, value, false);
        }

        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="time">缓存有效期</param>
        public static void Set(string key, object value, TimeSpan time)
        {
            RedisClient.StringSet(key, value, false, time);
        }


        /// <summary>
        /// 获取缓存值,(需要进一步判断Key是否存在可用 Exsit)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            return RedisClient.StringGet<T>(key);
        }
        
       /// <summary>
       /// 如果值不存在，则添加
       /// </summary>
       /// <param name="key">redis key</param>
       /// <param name="action">从数据库查询数据的过程</param>
       /// <param name="expTime">过期时间</param>
       /// <typeparam name="T"></typeparam>
       /// <returns></returns>
        public static T GetOrSet<T>(string key, Func<T> action,TimeSpan? expTime=null)
        {
            var val = RedisClient.StringGet<T>(key);
            if (val == null)
            {
                val = action();
                if (expTime!=null)
                {
                    RedisClient.StringSet(key, val, false, expTime);
                }
                else
                {
                    RedisClient.StringSet(key, val);
                }
            }

            return val;
        }
    

        /// <summary>
        /// 注意开发人员通常不要使用此方法.
        /// 此方法用于按模式匹配的方式进行删除.
        /// 注意:最多删除int.MaxValue个数据
        /// </summary>
        /// <param name="key">缓存键</param>
        public static void RemoveByPattern(string key)
        {
            RedisClient.RemoveByPattern(key + "*");
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            RedisClient.KeyDelete(key);
        }

        

        /// <summary>
        /// 是否存在stringkey
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool Exist(string key)
        {
            return RedisClient.KeyExists(key);
        }


        /// <summary>
        /// 加入元素到集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry">过期时间</param>
        /// <returns></returns>
        public static bool SetAdd<T>(string key, T value, TimeSpan? expiry = null)
        {
            return RedisClient.SetAdd(key, value, expiry);
        }

        /// <summary>
        /// 判断集合中是否存在此元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool SetContains<T>(string key, T value)
        {
            return RedisClient.SetContains(key, value);
        }

        /// <summary>
        /// 设置key 时效
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="expiry">时效</param>
        /// <returns></returns>
        public static bool KeyExpire(string key, TimeSpan? expiry = null)
        {
            return RedisClient.KeyExpire(key, expiry);
        }
}