using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Reflection;

namespace Limited.Gateway.Cache
{
    public class ConsulCache : ICache
    {
        //done
        public async Task<bool> Exists(string key)
        {
            var client = ConsulCacheConnection.CreateInstance().Client;
            var kvClient = client.KV;
            var result = await kvClient.Get(key);

            if (result.Response == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //done
        public async Task<string> Get(string key)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
            var result = await kvClient.Get(key);
            if (result.Response == null)
            {
                return default;
            }
            return ToResult<string>(result.Response);
        }

        //done
        public async Task<List<string>> Get(List<string> keys)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;


            var tasks = new List<Task<QueryResult<KVPair>>>();
            foreach (var key in keys)
            {
                tasks.Add(kvClient.Get(key));
            }

            var list = new List<string>();

            foreach (var task in tasks)
            {
                if (task.Result.Response == null)
                {
                    list.Add(default);
                }
                else
                {
                    list.Add(ToResult<string>(task.Result.Response));
                }
            }
            return list;
        }

        //done
        public async Task<Dictionary<string, string>> GetHash(string key)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
            var result = await kvClient.List(key);
            //return ToResult<string>(result);
            if (result.Response == null)
            {
                return new Dictionary<string, string>();
            }

            var items = new Dictionary<string, string>();
            foreach (var kvp in result.Response)
            {
                var value = Encoding.UTF8.GetString(kvp.Value); ;
                if (value != null)
                {
                    items.Add(kvp.Key, value);
                }
            }
            return items;
        }

        //done
        public async Task<string> GetHash(string key, string field)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
            var result = await kvClient.Get($"{key}/{field}");
            if (result.Response == null)
            {
                return default;
            }
            return Encoding.UTF8.GetString(result.Response.Value);
        }

        //done
        public async Task<List<string>> GetHash(List<string> keys, string field)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;

            var tasks = new List<Task<QueryResult<KVPair>>>();

            foreach (var key in keys)
            {
                tasks.Add(kvClient.Get($"{key}/{field}"));
            }

            Task.WaitAll(tasks.ToArray());

            var list = new List<string>();
            foreach (var task in tasks)
            {
                var json = Encoding.UTF8.GetString(task.Result.Response.Value);
                list.Add(json);
            }

            return list;
        }

        //done
        public async Task<bool> Remove(string key)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
            var result = await kvClient.Delete(key);
            return result.Response;
        }

        //done
        public async Task<bool> Remove(List<string> keys)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
            var tasks = new List<Task>();
            foreach (var key in keys)
            {
                tasks.Add(kvClient.Delete(key));
            };

            return true;
        }

        //done
        public async Task<bool> Set<T>(CacheNode<T> node)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;

            var cacheValue = new CacheValue<T>()
            {
                Value = node.Data
            };
            if (node.CacheTime != default)
            {
                cacheValue.ExpiryTime = DateTime.Now.Add(node.CacheTime);
            }

            var dataBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(cacheValue));

            var kvp = new KVPair(node.Key)
            {
                Value = dataBuffer
            };

            var result = await kvClient.Put(kvp);
            return result.Response;
        }

        //done
        public async Task<bool> Set<T>(List<CacheNode<T>> nodes)
        {
            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;

            var kvps = new ConcurrentBag<KVPair>();
            Parallel.ForEach(nodes, node =>
             {
                 var cacheValue = new CacheValue<T>()
                 {
                     Value = node.Data
                 };
                 if (node.CacheTime != default)
                 {
                     cacheValue.ExpiryTime = DateTime.Now.Add(node.CacheTime);
                 }
                 var dataBuffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(cacheValue));
                 var kvp = new KVPair(node.Key)
                 {
                     Value = dataBuffer
                 };
                 kvps.Add(kvp);
             });


            var tasks = new List<Task<WriteResult<bool>>>();
            foreach (var kvp in kvps)
            {
                tasks.Add(kvClient.Put(kvp));
            }

            // Task.WaitAll(tasks.ToArray());
            return true;
        }

        //done
        public async Task<bool> SetHash<T>(CacheNode<T> node)
        {
            if (node.CacheTime != default)
            {
                throw new Exception("nonsupport 'CacheTime' in Hash!");
            }
            if (node.Key.Substring(node.Key.Length - 1, 1) == "/")
            {
                throw new Exception("nonsupport the key has '/' in the end");
            }

            try
            {
                var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
                var dic = ToMap(node.Data);
                Parallel.ForEach(dic, x =>
                {
                    var dataBuffer = Encoding.UTF8.GetBytes(x.Value);
                    var kvp = new KVPair($"{node.Key}/{x.Key}")
                    {
                        Value = dataBuffer
                    };

                    var result = kvClient.Put(kvp);
                });
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        //done
        public async Task<bool> SetHash<T>(List<CacheNode<T>> nodes)
        {
            if (nodes.Any(x => x.CacheTime != default))
            {
                throw new Exception("nonsupport 'CacheTime' in Hash!");
            }

            var kvClient = ConsulCacheConnection.CreateInstance().Client.KV;
            var kvps = new ConcurrentBag<KVPair>();

            try
            {
                Parallel.ForEach(nodes, node =>
                {
                    var dic = ToMap(node.Data);
                    Parallel.ForEach(dic, x =>
                    {
                        var dataBuffer = Encoding.UTF8.GetBytes(x.Value);
                        var kvp = new KVPair($"{node.Key}/{x.Key}")
                        {
                            Value = dataBuffer
                        };

                        kvps.Add(kvp);
                    });
                });


                var tasks = new List<Task<WriteResult<bool>>>();
                foreach (var kvp in kvps)
                {
                    tasks.Add(kvClient.Put(kvp));
                }
                return true;
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        private string ToResult<T>(KVPair result)
        {
            var json = Encoding.UTF8.GetString(result.Value);
            dynamic nodeVaule = JsonConvert.DeserializeObject(json);
            if (nodeVaule.ExpiryTime == null|| nodeVaule.ExpiryTime==DateTime.MinValue)
            {
                return nodeVaule.Value.ToString();
            }
            if (nodeVaule.ExpiryTime >= DateTime.Now)
            {
                return nodeVaule.Value;
            }
            else
            {
                Remove(result.Key);
                return default;
            }
        }

        private Dictionary<string, string> ToMap(Object o)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

            Type t = o.GetType();

            PropertyInfo[] pi = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in pi)
            {
                MethodInfo mi = p.GetGetMethod();

                if (mi != null && mi.IsPublic)
                {
                    var value = mi.Invoke(o, new Object[] { });

                    var valueString = string.Empty;
                    if (value is ValueType || value is string)
                    {
                        valueString = value.ToString();
                    }
                    else
                    {
                        valueString = JsonConvert.SerializeObject(value);
                    }

                    map.Add(p.Name, valueString);
                }
            }

            return map;
        }
    }

    /// <summary>
    /// 缓存链接对象
    /// </summary>
    sealed class ConsulCacheConnection
    {
        private static ConsulCacheConnection instance = null;
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        private static string ConnectionString
        {
            get
            {
                //读取配置文件
                var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json");
                var config = configBuilder.Build();
                var connString = config.GetSection("GatewayConfig:CacheAddress").Value;
                if (connString == null)
                {
                    throw new Exception("GatewayConfig:CacheAddress is undefined");
                }

                return connString;
            }
        }

        public ConsulClient Client { get; set; } = null;

        /// <summary>
        /// 缓存链接单例,
        /// </summary>
        /// <returns></returns>
        public static ConsulCacheConnection CreateInstance()
        {
            if (instance == null)
            {
                _connectionLock.Wait();
                try
                {
                    if (instance == null)
                    {
                        instance = new ConsulCacheConnection();
                        instance.Client = new ConsulClient(x => { x.Address = new Uri(ConnectionString); });
                    }
                }
                finally
                {
                    _connectionLock.Release();
                }
            }

            return instance;
        }
    }

    sealed class CacheValue<T>
    {
        public T Value { get; set; }

        public DateTime ExpiryTime { get; set; }
    }
}