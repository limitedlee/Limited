using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Limited.Gateway.Cache
{
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

    public class ConsulCache : ICache
    {
        public async Task<bool> Exists(string key)
        {
            var client = ConsulCacheConnection.CreateInstance().Client;
            var kvClient = client.KV;
            var result = await kvClient.Get(key);

            if (result.Response==null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<string> Get(string key)
        {
            var client = ConsulCacheConnection.CreateInstance().Client;
            var kvClient = client.KV;
            var response = await kvClient.Get(key);
            throw new NotImplementedException();
        }

        public async Task<List<string>> Get(List<string> keys)
        {
            throw new NotImplementedException();
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

        public async Task<bool> Remove(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<List<bool>> Remove(List<string> keys)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Set<T>(CacheNode<T> node)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> Set<T>(List<CacheNode<T>> nodes)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetHash<T>(CacheNode<T> node)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> SetHash<T>(List<CacheNode<T>> nodes)
        {
            throw new NotImplementedException();
        }
    }
}