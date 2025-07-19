namespace CommonUtil.RedisUtil;

public interface IRedisWrapper
{
    SuperObj StringGetSuperObj<T>(string key);
        /// <summary>
        /// 开始连接Redis,构造之后首先调用此方法
        /// </summary>
        /// <param name="connectString">redis连接字符串</param>
        object StartConnect(string connectString);

        /// <summary>
        /// 序列化对象的方式
        /// </summary>
        SerializerType SerializerType { get; }

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作情况</returns>
        bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?));

        /// <summary>
        /// 通过序列化对象保存一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">对象</param>
        /// <param name="dontSerialize">不进行序列化操作(默认false)</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作情况</returns>
        bool StringSet<T>(string key, T obj, bool dontSerialize = false, TimeSpan? expiry = default(TimeSpan?));

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>对应值</returns>
        string StringGet(string key);

        DateTime GetRedisServerTime();

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        T StringGet<T>(string key);

        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        long StringIncrement(string key, long val = 1);

        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>减少后的值</returns>
        double StringDecrement(string key, long val = 1);

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>存在否</returns>
        bool HashExists(string key, string hashField);

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <param name="obj">对象</param>
        /// <returns>操作结果</returns>
        bool HashSet<T>(string key, string hashField, T obj);

        /// <summary>
        /// HashGet RedisValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        SuperObj HashGetRedisValue(string key, string hashField);

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>操作结果</returns>
        bool HashDelete(string key, string hashField);

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>值</returns>
        T HashGet<T>(string key, string hashField);

       

        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        List<string> HashKeys(string key);

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void ListRemove<T>(string key, T value);

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>对应List</returns>
        List<T> ListRange<T>(string key);

        /// <summary>
        /// ListRightPush
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void ListRightPush<T>(string key, T value);

        /// <summary>
        /// ListRightPop
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>结果</returns>
        T ListRightPop<T>(string key);

        /// <summary>
        /// ListLeftPush
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        void ListLeftPush<T>(string key, T value);

        /// <summary>
        /// ListLeftPop
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对应的值</returns>
        T ListLeftPop<T>(string key);

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>长度</returns>
        long ListLength(string key);

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        bool KeyDelete(string key);

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>存在否</returns>
        bool KeyExists(string key);

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">旧的key</param>
        /// <param name="newKey">新的key</param>
        /// <returns>操作结果</returns>
        bool KeyRename(string key, string newKey);

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作结果</returns>
        bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?));

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel">频道</param>
        /// <param name="handler">处理机制</param>
        void Subscribe<T>(string subChannel, Action<object, T>? handler = null);

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="channel">频道</param>
        /// <param name="msg">发布的数据</param>
        /// <returns>操作结果</returns>
        long Publish<T>(string channel, T msg);

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        void Unsubscribe(string channel);

        

        /// <summary>
        /// 获取在redis中真实的key
        /// </summary>
        /// <param name="key">开发人员输入的KEY</param>
        /// <returns>真实的key</returns>
        string GetRealStoreKey(string key);

        /// <summary>
        /// 手动释放连接
        /// </summary>
        void Close();

        /// <summary>
        /// 释放连接
        /// </summary>
        void Dispose();
}