using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway.Extensions
{
    public class GatewayConfig : IOptions<GatewayConfig>
    {
        public string CacheProvider { get; set; }

        public string CacheAddress { get; set; }

        public string ServiceDiscoveryProvider { get; set; }

        public string ServiceDiscoveryAddress { get; set; }

        public GatewayConfig Value => this;
    }

    public class CacheProvider
    {
        public const string Consul = "Consul";

        public const string Redis = "Redis";
    }
}
