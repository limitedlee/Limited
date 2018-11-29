using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Cache
{
    public interface ICache
    {
        Task<bool> Exists(string key);

        Task<bool> Set<T>(CacheNode<T> node);

        Task<bool> Set<T>(List<CacheNode<T>> nodes);

        Task<bool> SetHash<T>(CacheNode<T> node);

        Task<bool> SetHash<T>(List<CacheNode<T>> nodes);

        Task<string> Get(string key);

        Task<Dictionary<string, string>> GetHash(string key);

        Task<string> GetHash(string key, string field);

        Task<List<string>> GetHash(List<string> keys, string field);

        Task<List<string>> Get(List<string> keys);

        Task<bool> Remove(string key);

        Task<bool> Remove(List<string> keys);
    }
}
