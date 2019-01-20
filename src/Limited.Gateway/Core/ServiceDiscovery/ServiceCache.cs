using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway.Core.ServiceDiscovery
{
    public static class ServiceCache
    {
        public static ConcurrentDictionary<string, ConcurrentBag<MicroService>> Services { get; set; } = new ConcurrentDictionary<string, ConcurrentBag<MicroService>>();
    }
}
