using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public class RouteTable
    {
        public const string RouteCacheKey = "Gateway.RouteConfig.Cache";
        private static RouteTable route = null;
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public List<RouteOption> Cache { get; set; } = null;

        public RouteTable()
        {
            if (Cache == null)
            {
                try
                {
                    _connectionLock.Wait();
                    if (Cache == null)
                    {
                        Cache = new List<RouteOption>();
                        var result = ConsulHelper.Client.KV.Get(RouteCacheKey);
                        Task.WaitAny(result);

                        if (result.Result.Response != null)
                        {
                            Cache = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RouteOption>>(result.Result.Response.ToString());
                        }
                        Console.WriteLine("route cache init");
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
            var node = new CacheValue<List<RouteOption>>()
            {
                Value = routes,
                ExpiryTime = DateTime.Now.AddYears(99)
            };

            await ConsulHelper.Set(RouteCacheKey, node);
        }
    }
}
