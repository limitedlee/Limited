using Limited.Gateway.Cache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Limited.Gateway.Options
{
    public class Route
    {
        private ICache cacheProvider;
        public const string RouteCacheKey = "Gateway.RouteConfig.Cache";
        private static Route route = null;
        private static readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);

        public List<RouteOption> Cache { get; set; } = null;

        public Route(ICache _cache)
        {
            cacheProvider = _cache;

            if (Cache == null)
            {
                try
                {
                    _connectionLock.Wait();
                    if (Cache == null)
                    {
                        Cache = new List<RouteOption>();
                        Task<string> result = cacheProvider.Get(RouteCacheKey);

                        if (result.Result != null)
                        {
                            Cache = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RouteOption>>(result.Result);
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
            var node = new CacheNode<List<RouteOption>>()
            {
                Key = RouteCacheKey,
                Data = routes
            };

            await cacheProvider.Set(node);
        }
    }
}
