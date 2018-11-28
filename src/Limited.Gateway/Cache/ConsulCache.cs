using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Cache
{
    public class ConsulCache : ICache
    {
        public Task<bool> Exists(string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> Get(string key)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> Get(List<string> keys)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetHash(string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHash(string key, string field)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetHash(List<string> keys, string field)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Remove(string key)
        {
            throw new NotImplementedException();
        }

        public Task<List<bool>> Remove(List<string> keys)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Set<T>(CacheNode<T> node)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Set<T>(List<CacheNode<T>> nodes)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetHash<T>(CacheNode<T> node)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SetHash<T>(List<CacheNode<T>> nodes)
        {
            throw new NotImplementedException();
        }
    }
}
