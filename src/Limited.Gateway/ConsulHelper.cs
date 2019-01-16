using Consul;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public static class ConsulHelper
    {
        public static ConsulClient Client { get; set; }

        public static void UseConsul(this IApplicationBuilder app, string consulUrl)
        {
            Client = new ConsulClient(x => { x.Address = new Uri(consulUrl); });
        }

        //done
        public static async Task<bool> Exists(string key)
        {
            var kvClient = Client.KV;
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
        public static async Task<string> Get(string key)
        {
            var kvClient = Client.KV;
            var result = await kvClient.Get(key);
            if (result.Response == null)
            {
                return default;
            }
            return ToResult<string>(result.Response);
        }

        //done
        public static async Task<List<string>> Get(List<string> keys)
        {
            var kvClient = Client.KV;
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
        public static async Task<Dictionary<string, string>> GetHash(string key)
        {
            var kvClient = Client.KV;
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
        public static async Task<string> GetHash(string key, string field)
        {
            var kvClient = Client.KV;
            var result = await kvClient.Get($"{key}/{field}");
            if (result.Response == null)
            {
                return default;
            }
            return Encoding.UTF8.GetString(result.Response.Value);
        }

        //done
        public static async Task<List<string>> GetHash(List<string> keys, string field)
        {
            var kvClient = Client.KV;
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
        public static async Task<bool> Remove(string key)
        {
            var kvClient = Client.KV;
            var result = await kvClient.Delete(key);
            return result.Response;
        }

        //done
        public static async Task<bool> Remove(List<string> keys)
        {
            var kvClient = Client.KV;
            var tasks = new List<Task>();
            foreach (var key in keys)
            {
                tasks.Add(kvClient.Delete(key));
            };

            return true;
        }

        //done
        public static async Task<bool> Set<T>(string key, CacheValue<T> node)
        {
            var kvClient = Client.KV;
            var dataBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(node));

            var kvp = new KVPair(key)
            {
                Value = dataBuffer
            };

            var result = await kvClient.Put(kvp);
            return result.Response;
        }

        //done
        public static async Task<bool> Set<T>(Dictionary<string, CacheValue<T>> nodes)
        {
            var kvClient = Client.KV;

            var kvps = new ConcurrentBag<KVPair>();
            Parallel.ForEach(nodes, node =>
            {
                var dataBuffer = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(node.Value));
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
        public static async Task<bool> SetHash<T>(string key, CacheValue<T> node)
        {
            if (key.Substring(key.Length - 1, 1) == "/")
            {
                throw new Exception("nonsupport the key has '/' in the end");
            }

            try
            {
                var kvClient = Client.KV;
                var dic = ToMap(node.Value);
                Parallel.ForEach(dic, x =>
                {
                    var cache = new CacheValue<string>();
                    cache.Value = x.Value;
                    cache.ExpiryTime = node.ExpiryTime;
                    var json = JsonConvert.SerializeObject(cache);
                    var dataBuffer = Encoding.UTF8.GetBytes(json);
                    var kvp = new KVPair($"{key}/{x.Key}")
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
        public static async Task<bool> SetHash<T>(Dictionary<string, CacheValue<T>> nodes)
        {
            var kvClient = Client.KV;
            var kvps = new ConcurrentBag<KVPair>();
            try
            {
                Parallel.ForEach(nodes, node =>
                {
                    var dic = ToMap(node.Value.Value);
                    Parallel.ForEach(dic, x =>
                    {
                        var cache = new CacheValue<string>();
                        cache.Value = x.Value;
                        cache.ExpiryTime = node.Value.ExpiryTime;
                        var json = JsonConvert.SerializeObject(cache);
                        var dataBuffer = Encoding.UTF8.GetBytes(json);
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

        private static string ToResult<T>(KVPair result)
        {
            var json = Encoding.UTF8.GetString(result.Value);
            dynamic nodeVaule = JsonConvert.DeserializeObject(json);
            if (nodeVaule.ExpiryTime == null || nodeVaule.ExpiryTime == DateTime.MinValue)
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

        private static Dictionary<string, string> ToMap(Object o)
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
}
