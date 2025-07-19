using System.Collections.Concurrent;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CommonUtil.RedisUtil;

/**
 * 安装包：
 *  1. protobuf-net
 *  2. StackExchange.Redis
 * 
 */

    /// <summary>
    /// Redis操作的封装,实例化对象然后调用StartConnect方法即可
    /// 如果不Dispose/Close可以一直用同一个 ConnectionMultiplexer
    /// </summary>
    public class StackExchangeRedisWrapper : IDisposable, IRedisWrapper
    {

        /// <summary>
        /// 开始连接Redis,构造之后首先调用此方法
        /// </summary>
        /// <param name="connectString">redis连接字符串</param>
        public object StartConnect(string connectString)
        {
            _connectionString = connectString;
            _conn = GetConnection(connectString);
            return _conn;
        }

        #region 字段和属性

        /// <summary>
        /// db号
        /// </summary>
        private readonly int _dbNum;
        /// <summary>
        /// 当前实例所独有的ConnectionMultiplexer
        /// </summary>
        private ConnectionMultiplexer _conn;

        private string _connectionString;
        /// <summary>
        /// 根据连接字符串缓存ConnectionMultiplexer
        /// </summary>
        private static readonly ConcurrentDictionary<String, ConnectionMultiplexer>
            ConnectionMultiplexerCache = new ConcurrentDictionary<string, ConnectionMultiplexer>();

        /// <summary>
        /// 自定义Key前缀
        /// </summary>
        private readonly string? _keyPrefix;


        /// <summary>
        /// 序列化对象的方式
        /// </summary>
        public SerializerType SerializerType
        {
            get;
            set;
        }
        #endregion 字段和属性

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="keyPrefix">所有Key统一前缀</param>
        /// <param name="dbNum">db号</param>
        /// <param name="serializerType">序列化对象的方式,默认采用Json,目前开发不要手动设置其他序列化方式!!!!!!</param>
        public StackExchangeRedisWrapper(string? keyPrefix = null,
            int dbNum = 0,
            SerializerType serializerType = SerializerType.Json)
        {
            _dbNum = dbNum;
            _keyPrefix = keyPrefix;
            SerializerType = serializerType;
        }


        #endregion 构造函数

  

        #region String相关操作->同步方法

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作情况</returns>
        public bool StringSet(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = GetRealStoreKey(key);
            return Do(db => db.StringSet(key, value, expiry));
        }

       

        /// <summary>
        /// 通过序列化对象保存一个对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="obj">对象</param>
        /// <param name="dontSerialize">不进行序列化操作(默认false)</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作情况</returns>
        public bool StringSet<T>(string key, T obj, bool dontSerialize = false, TimeSpan? expiry = default(TimeSpan?))
        {
            key = GetRealStoreKey(key);
            var value = SerializeObject(obj, dontSerialize);
            return Do(db => db.StringSet(key, value, expiry));
        }

        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key">Redis Key</param>
        /// <param name="value">保存的值</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作情况</returns>
        public bool StringSetIfNotExists<T>(string key, T obj, bool dontSerialize = false, TimeSpan? expiry = default(TimeSpan?))
        {
            key = GetRealStoreKey(key);
            var value = SerializeObject(obj, dontSerialize);
            return Do(db => db.StringSet(key, value, expiry, When.NotExists));
        }

        /// <summary>
        /// 获取单个key的值
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>对应值</returns>
        public string StringGet(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.StringGet(key));
        }
        public SuperObj StringGetSuperObj<T>(string key)
        {
            key = GetRealStoreKey(key);
            var result = new SuperObj();
            var data = Do(db =>
            {
                return db.StringGet(key);
            });

            result.HasValue = data.HasValue;
            result.Value = DeserializeObject<T>(data);
            return result;
        }

        protected RedisResult ScriptEvaluate(string lua)
        {
            return Do(
                db => db.ScriptEvaluate(lua)
            );
        }

        public DateTime GetRedisServerTime()
        {
            var value = ScriptEvaluate("return redis.call('TIME')");
            var array = (long[])value;
            var seconds = array[0];
            var milliseconds = array[1] / 1000d;
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            var time = startTime.AddSeconds(seconds);
            time.AddMilliseconds(milliseconds);
            return time;
        }

        //public long GetGetRedisServerUsTime()
        //{
        //    var value = ScriptEvaluate("return redis.call('TIME')");
        //    var array = (long[])value;
        //    var seconds = array[0];
        //    var us = array[1];
        //    return seconds * 1000 + us;
        //}

        ///// <summary>
        ///// 获取多个Key
        ///// </summary>
        ///// <param name="listKey">Redis Key集合</param>
        ///// <returns>对应值</returns>
        //public RedisValue[] StringGet(List<string> listKey)
        //{
        //    List<string> newKeys = listKey.Select(GetRealStoreKey).ToList();
        //    return Do(db => db.StringGet(ConvertStringList2RedisKeyArray(newKeys)));
        //}

        /// <summary>
        /// 获取一个key的对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对象</returns>
        public T StringGet<T>(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db => DeserializeObject<T>(db.StringGet(key)));
        }

        /// <summary>
        /// 注意开发人员通常不要使用此方法.
        /// 此方法用于按模式匹配的方式进行删除.
        /// 注意:最多删除int.MaxValue个数据
        /// </summary>
        /// <param name="key"></param>
        public void RemoveByPattern(string key)
        {
            key = GetRealStoreKey(key);
            var server = _conn.GetServer(ConnectionMultiplexerCache.First().Value.GetEndPoints().First());
            var targetKeyList = server.Keys(_dbNum, key, int.MaxValue);
            var tList = targetKeyList.ToArray();
            Do(db => db.KeyDelete(tList));
        }

       
        /// <summary>
        /// 为数字增长val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">可以为负</param>
        /// <returns>增长后的值</returns>
        public long StringIncrement(string key, long val = 1)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.StringIncrement(key, val));
        }
        /// <summary>
        /// 为数字减少val
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="val">val</param>
        /// <returns>减少后的值</returns>
        public double StringDecrement(string key, long val = 1)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.StringDecrement(key, val));
        }

        #endregion 同步方法


        #region Hash相关操作->同步方法

        /// <summary>
        /// 判断某个数据是否已经被缓存
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>存在否</returns>
        public bool HashExists(string key, string hashField)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.HashExists(key, hashField));
        }

        /// <summary>
        /// 存储数据到hash表
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <param name="obj">对象</param>
        /// <returns>操作结果</returns>
        public bool HashSet<T>(string key, string hashField, T obj)
        {
            key = GetRealStoreKey(key);
            return Do(db =>
            {
                var value = SerializeObject(obj);
                return db.HashSet(key, hashField, value);
            });
        }
        ///// <summary>
        ///// HashSet RedisValue
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="hashField"></param>
        ///// <param name="value"></param>
        ///// <returns></returns>
        //public bool HashSetRedisValue(string key, string hashField, RedisValue value)
        //{
        //    key = GetRealStoreKey(key);
        //    return Do(db =>
        //    {
        //        return db.HashSet(key, hashField, value);
        //    });
        //}
        /// <summary>
        /// HashGet RedisValue
        /// </summary>
        /// <param name="key"></param>
        /// <param name="hashField"></param>
        /// <returns></returns>
        public SuperObj HashGetRedisValue(string key, string hashField)
        {
            key = GetRealStoreKey(key);
            var result = new SuperObj();
            var data = Do(db =>
             {
                 return db.HashGet(key, hashField);
             });
            result.HasValue = data.HasValue;
            result.Value = data;

            return result;
        }

        /// <summary>
        /// 移除hash中的某值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>操作结果</returns>
        public bool HashDelete(string key, string hashField)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.HashDelete(key, hashField));
        }

        ///// <summary>
        ///// 移除hash中的多个值
        ///// </summary>
        ///// <param name="key">键</param>
        ///// <param name="hashField">hash域</param>
        ///// <returns>操作结果</returns>
        //public long HashDelete(string key, List<RedisValue> hashField)
        //{
        //    key = GetRealStoreKey(key);
        //    return Do(db => db.HashDelete(key, hashField.ToArray()));
        //}

        /// <summary>
        /// 从hash表获取数据
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="hashField">hash域</param>
        /// <returns>值</returns>
        public T HashGet<T>(string key, string hashField)
        {
            key = GetRealStoreKey(key);
            return Do(db =>
            {
                var value = db.HashGet(key, hashField);
                return DeserializeObject<T>(value);
            });
        }



        /// <summary>
        /// 获取hashkey所有Redis key
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public List<string> HashKeys(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db =>
            {
                RedisValue[] values = db.HashKeys(key);
                return ConvertRedisValueArray2List<string>(values);
            });
        }

        #endregion Hash相关操作->同步方法
        

        #region List相关操作->同步方法

        /// <summary>
        /// 移除指定ListId的内部List的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void ListRemove<T>(string key, T value)
        {
            key = GetRealStoreKey(key);
            Do(db => db.ListRemove(key, SerializeObject(value)));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>对应List</returns>
        public List<T> ListRange<T>(string key)
        {
            key = GetRealStoreKey(key);
            return Do(redis =>
            {
                var values = redis.ListRange(key);
                return ConvertRedisValueArray2List<T>(values);
            });
        }

        /// <summary>
        /// ListRightPush
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void ListRightPush<T>(string key, T value)
        {
            key = GetRealStoreKey(key);
            Do(db => db.ListRightPush(key, SerializeObject(value)));
        }

        /// <summary>
        /// ListRightPop
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>结果</returns>
        public T ListRightPop<T>(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db =>
             {
                 var value = db.ListRightPop(key);
                 return DeserializeObject<T>(value);
             });
        }

        /// <summary>
        /// ListLeftPush
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public void ListLeftPush<T>(string key, T value)
        {
            key = GetRealStoreKey(key);
            Do(db => db.ListLeftPush(key, SerializeObject(value)));
        }

        /// <summary>
        /// ListLeftPop
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>对应的值</returns>
        public T ListLeftPop<T>(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db =>
            {
                var value = db.ListLeftPop(key);
                return DeserializeObject<T>(value);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>长度</returns>
        public long ListLength(string key)
        {
            key = GetRealStoreKey(key);
            return Do(redis => redis.ListLength(key));
        }

        #endregion 同步方法

        

        #region Set相关操作
        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public bool SetAdd<T>(string key, T value, TimeSpan? expiry = null)
        {
            key = GetRealStoreKey(key);
            var flag = Do(redis => redis.SetAdd(key, SerializeObject(value)));
            if (flag)
            {
                if (expiry != null)
                {
                    Do(redis => redis.KeyExpire(key, expiry));
                }
            }
            return flag;
        }

        /// <summary>
        /// 集合中是否包含传入的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public bool SetContains<T>(string key, T value)
        {
            key = GetRealStoreKey(key);
            return Do(redis => redis.SetContains(key, SerializeObject(value)));
        }

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public async Task<bool> SetAddAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            key = GetRealStoreKey(key);
            if (expiry != null)
            {
                this.KeyExpire(key, expiry);
            }
            var flag = await Do(redis => redis.SetAddAsync(key, SerializeObject(value))).ConfigureAwait(false);
            if (flag)
            {
                if (expiry != null)
                {
                    Do(redis => redis.KeyExpire(key, expiry));
                }
            }
            return flag;
        }

        /// <summary>
        /// 集合中是否包含传入的值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public async Task<bool> SetContainsAsync<T>(string key, T value)
        {
            key = GetRealStoreKey(key);
            return await Do(redis => redis.SetContainsAsync(key, SerializeObject(value))).ConfigureAwait(false);
        }
        #endregion

        
        


        #region SortedSet(有序集合)相关操作->同步方法

        /// <summary>
        /// 添加对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="score">分值</param>
        public bool SortedSetAdd<T>(string key, T value, double score)
        {
            key = GetRealStoreKey(key);
            return Do(redis => redis.SortedSetAdd(key, SerializeObject<T>(value), score));
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        public bool SortedSetRemove<T>(string key, T value)
        {
            key = GetRealStoreKey(key);
            return Do(redis => redis.SortedSetRemove(key, SerializeObject(value)));
        }

        /// <summary>
        /// 按照分数从低到高的顺序获取全部数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>list</returns>
        public List<T> SortedSetRangeByRank<T>(string key)
        {
            key = GetRealStoreKey(key);
            return Do(redis =>
            {
                var values = redis.SortedSetRangeByRank(key);
                return ConvertRedisValueArray2List<T>(values);
            });
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>长度</returns>
        public long SortedSetLength(string key)
        {
            key = GetRealStoreKey(key);
            return Do(redis => redis.SortedSetLength(key));
        }

        #endregion SortedSet(有序集合)相关操作->同步方法
        

        
        #region key相关操作

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.KeyDelete(key));
        }

      

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>存在否</returns>
        public bool KeyExists(string key)
        {
            key = GetRealStoreKey(key);
            return Do(db => db.KeyExists(key));
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key">旧的key</param>
        /// <param name="newKey">新的key</param>
        /// <returns>操作结果</returns>
        public bool KeyRename(string key, string newKey)
        {
            key = GetRealStoreKey(key);
            newKey = GetRealStoreKey(newKey);
            return Do(db => db.KeyRename(key, newKey));
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="expiry">过期时间</param>
        /// <returns>操作结果</returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = GetRealStoreKey(key);
            return Do(db => db.KeyExpire(key, expiry));
        }

        #endregion key相关操作
        
        
        

        #region 发布订阅相关操作

        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel">频道</param>
        /// <param name="handler">处理机制</param>
        public void Subscribe<T>(string subChannel, Action<object, T>? handler = null)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " 订阅收到消息：" + message);
                }
                else
                {
                    if (this.SerializerType == SerializerType.ProtoBuf)
                    {
                        handler(channel, ProtoBufUtil.DeSerialize<T>(message));
                    }
                    else
                    {
                        handler(channel, DeserializeObject<T>(message));
                    }

                }
            });
        }

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="channel">频道</param>
        /// <param name="msg">发布的数据</param>
        /// <returns>操作结果</returns>
        public long Publish<T>(string channel, T msg)
        {
            ISubscriber sub = _conn.GetSubscriber();
            return sub.Publish(channel, SerializeObject(msg));
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            ISubscriber sub = _conn.GetSubscriber();
            sub.Unsubscribe(channel);
        }



        #endregion 发布订阅相关操作
        
        
        

        #region 系统级别方法(获取事务、服务器等)

        public ITransaction CreateTransaction()
        {
            return GetDatabase().CreateTransaction();
        }

        public IDatabase GetDatabase()
        {
            return _conn.GetDatabase(_dbNum);
        }

        public IServer GetServer(string hostAndPort)
        {
            return _conn.GetServer(hostAndPort);
        }

        /// <summary>
        /// 根据连接字符串获取ConnectionMultiplexer
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>连接实例</returns>
        private ConnectionMultiplexer GetConnection(string connectionString)
        {
            if (ConnectionMultiplexerCache.ContainsKey(connectionString))
            {
                var result = ConnectionMultiplexerCache[connectionString];
                if (result.IsConnected)
                {
                    return result;
                }
            }
            var connect = ConnectionMultiplexer.Connect(connectionString);

            //注册如下事件
            //if (ConnectionFailed != null)
            //{
            //    connect.ConnectionFailed += ConnectionFailed;
            //}
            //if (ConnectionRestored != null)
            //{
            //    connect.ConnectionRestored += ConnectionRestored;
            //}
            //if (ErrorMessage != null)
            //{
            //    connect.ErrorMessage += ErrorMessage;
            //}
            //if (ConfigurationChanged != null)
            //{
            //    connect.ConfigurationChanged += ConfigurationChanged;
            //}
            //if (ConfigurationChangedBroadcast != null)
            //{
            //    connect.ConfigurationChangedBroadcast += ConfigurationChangedBroadcast;
            //}
            //if (HashSlotMoved != null)
            //{
            //    connect.HashSlotMoved += HashSlotMoved;
            //}
            //if (InternalError != null)
            //{
            //    connect.InternalError += InternalError;
            //}
            ConnectionMultiplexerCache[connectionString] = connect;
            return connect;
        }

        #endregion 系统级别方法(获取事务、服务器等)
        
        
        

        #region 辅助方法

        /// <summary>
        /// 获取在redis中真实的key
        /// </summary>
        /// <param name="key">开发人员输入的KEY</param>
        /// <returns>真实的key</returns>
        public string GetRealStoreKey(string key)
        {
            return _keyPrefix + key;
        }

        /// <summary>
        /// 自定义处理方法
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="func">处理方法委托</param>
        /// <returns>返回值</returns>
        private T Do<T>(Func<IDatabase, T> func)
        {
            var database = _conn.GetDatabase(_dbNum);
            var ret = func(database);
            return ret;
        }

        /// <summary>
        /// 将对象序列化,基本数据类型就不会用序列化类进行序列化
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="value">数据值</param>
        /// <param name="dontSerialize">不进行序列化</param>
        /// <returns>RedisValue</returns>
        private RedisValue SerializeObject<T>(T value, bool dontSerialize = false)
        {
            if (value is int || value is long || value is byte || value is short ||
                value is uint || value is sbyte || value is ushort || value is string || value is double || value.GetType() == typeof(byte[]))
            {
                RedisValue ret = (dynamic)value;
                return ret;
            }

            if (dontSerialize)
            {
                RedisValue ret = (dynamic)value;
                return ret;
            }
            if (value is bool)
            {
                return (dynamic)value ? "1" : "0";
            }
            if (value is DateTime)
            {
                var obj = (dynamic)value;
                return (dynamic)(obj.ToString("yyyy-MM-ddTHH:mm:sszzzz", System.Globalization.DateTimeFormatInfo.InvariantInfo));
            }
            if (value is TimeSpan)
            {
                return (dynamic)(value.ToString());
            }
            if (this.SerializerType == SerializerType.ProtoBuf)
            {
                return ProtoBufUtil.Serialize(value);
            }
            return JsonConvert.SerializeObject (value,new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /// <summary>
        /// 将对应的数值转化为对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">数值</param>
        /// <returns>对象</returns>
        public T DeserializeObject<T>(RedisValue value)
        {
            if (value.IsNullOrEmpty)
            {
                return default(T);
            }
            var t = typeof(T);
            if (t == typeof(RedisValue))
            {
                return (dynamic)value;
            }
            if (value.IsInteger)
            {
                if (t == typeof(long))
                {
                    long result1;
                    if (value.TryParse(out result1))
                    {
                        return (dynamic)result1;
                    }
                }
                if (t == typeof(int))
                {
                    int result2;
                    if (value.TryParse(out result2))
                    {
                        return (dynamic)result2;
                    }
                }
                long result;
                if (value.TryParse(out result))
                {
                    if (t == typeof(byte))
                    {
                        return (dynamic)((byte)result);
                    }
                    else if (t == typeof(short))
                    {
                        return (dynamic)((short)result);
                    }
                    else if (t == typeof(sbyte))
                    {
                        return (dynamic)((sbyte)result);
                    }
                    else if (t == typeof(ushort))
                    {
                        return (dynamic)((ushort)result);
                    }
                    else if (t == typeof(uint))
                    {
                        return (dynamic)((uint)result);
                    }
                }
            }
            if (t == typeof(bool))
            {
                var v = value.ToString();
                if (v == "1" || v == "true" || v == "True")
                {
                    return (dynamic)true;
                }
            }
            if (t == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out DateTime trydt))
                {
                    return (dynamic)trydt;
                }
            }
            if (t == typeof(TimeSpan))
            {
                if (Int64.TryParse(value, out long tryint64)) {

                    var obj = new TimeSpan(tryint64);
                    return (dynamic)obj;
                };
            }

            if (t == typeof(double))
            {
                double result;
                if (value.TryParse(out result))
                {
                    return (dynamic)result;
                }
            }
            if (t == typeof(string))
            {
                return (dynamic)value.ToString();
            }
            if (t == typeof(byte[]))
            {
                return (dynamic)value;
            }
            if (this.SerializerType == SerializerType.ProtoBuf)
            {
                return ProtoBufUtil.DeSerialize<T>(value);
            }
            return JsonConvert.DeserializeObject<T>(value,new JsonSerializerSettings()
            {
                 ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }

        /// <summary>
        /// 将RedisValue数组转换为对象List
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="values">对象数组</param>
        /// <returns>List对象</returns>
        private List<T> ConvertRedisValueArray2List<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();
            foreach (var item in values)
            {
                var model = DeserializeObject<T>(item);
                result.Add(model);
            }
            return result;
        }

        ///// <summary>
        ///// 将字符串List形式的redisKey转换为RedisKey[]
        ///// </summary>
        ///// <param name="redisKeyList">目标key列表</param>
        ///// <returns>RedisKey[]</returns>
        //private RedisKey[] ConvertStringList2RedisKeyArray(List<string> redisKeyList)
        //{
        //    return redisKeyList.Select(redisKey => (RedisKey)redisKey).ToArray();
        //}




        #endregion 辅助方法

        #region 资源释放
        /// <summary>
        /// 手动释放连接
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 释放连接
        /// </summary>
        public void Dispose()
        {
            if (_conn != null && _conn.IsConnected)
            {
                _conn.Close();
                ConnectionMultiplexer value;
                ConnectionMultiplexerCache.TryRemove(_connectionString, out value);
            }
        }


        #endregion 资源释放

        #region ConnectionMultiplexer实例相关事件

        ///// <summary>
        /////  当检测到配置变更时触发
        ///// </summary>
        //public event EventHandler<EndPointEventArgs> ConfigurationChanged;

        ///// <summary>
        /////    当一个redis节点被明确要求重新配置(通过广播告知)时触发,这通常意味着主从变化.
        ///// </summary>
        //public event EventHandler<EndPointEventArgs> ConfigurationChangedBroadcast;

        ///// <summary>
        /////  当物理连接失败时触发
        ///// </summary>
        //public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed;

        ///// <summary>
        /////  当物理连接建立成功时触发
        ///// </summary>
        //public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored;

        ///// <summary>
        ///// 当redis服务端回应一个错误信息时触发
        ///// </summary>
        //public event EventHandler<RedisErrorEventArgs> ErrorMessage;

        ///// <summary>
        ///// 当一个hash-slot已经完成搬迁时触发
        ///// </summary>
        //public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved;

        ///// <summary>
        /////  只要当一个网络错误出现时就会触发(这主要用于调试)
        ///// </summary>
        //public event EventHandler<InternalErrorEventArgs> InternalError;

        #endregion ConnectionMultiplexer实例相关事件
    }