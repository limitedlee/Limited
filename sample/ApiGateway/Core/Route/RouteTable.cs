using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public class RouteTable
    {
        public const string RouteCacheKey = "Gateway.RouteConfig.Cache";
        private static RouteTable route = null;
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public ConcurrentBag<RouteOption> Cache { get; set; } = null;

        public RouteTable()
        {
            if (Cache == null)
            {
                try
                {
                    _connectionLock.Wait();
                    if (Cache == null)
                    {
                        Cache = new ConcurrentBag<RouteOption>();
                        //var result = ConsulHelper.Client.KV.Get(RouteCacheKey);
                        //Task.WaitAny(result);

                        //if (result.Result.Response != null)
                        //{
                        //    var json = Encoding.UTF8.GetString(result.Result.Response.Value);
                        //    var routes = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RouteOption>>(json);

                        //    Parallel.ForEach(routes, route =>
                        //     {
                        //         route.TargetService = route.TargetService.ToLower();
                        //         Cache.Add(route);
                        //     });
                        //}

                    }
                }
                finally
                {
                    _connectionLock.Release();
                }
            }
        }

        public async Task Push(List<RouteOption> routes)
        {
            //var kvp = new KVPair(RouteCacheKey);
            //var dataBuffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(routes));
            //kvp.Value = dataBuffer;

            //await ConsulHelper.Client.KV.Put(kvp);
        }
    }
}
