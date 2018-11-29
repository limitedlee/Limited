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
            return ToResult<string>(result);
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
                list.Add(ToResult<string>(task.Result));
            }
            return list;
        }

        public async Task<Dictionary<string, string>> GetHash(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetHash(string key, string field)
        {
            throw new NotImplementedException();
        }

        public async Task<List<string>> GetHash(List<string> keys, string field)
        {
            throw new NotImplementedException();
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
                Vaule = node.Data
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
                     Vaule = node.Data
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

        public async Task<bool> SetHash<T>(CacheNode<T> node)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetHash<T>(List<CacheNode<T>> nodes)
        {
            throw new NotImplementedException();
        }

        private T ToResult<T>(QueryResult<KVPair> result)
        {
            if (result == null)
            {
                return default;
            }

            var json = Encoding.UTF8.GetString(result.Response.Value);
            var nodeVaule = Newtonsoft.Json.JsonConvert.DeserializeObject<CacheValue<T>>(json);
            if (nodeVaule.ExpiryTime == null)
            {
                return nodeVaule.Vaule;
            }
            if (nodeVaule.ExpiryTime >= DateTime.Now)
            {
                return nodeVaule.Vaule;
            }
            else
            {
                Remove(result.Response.Key);
                return default;
            }
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
        public T Vaule { get; set; }

        public DateTime ExpiryTime { get; set; }
    }
}