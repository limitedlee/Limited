using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft;
using Newtonsoft.Json;

namespace ApiGateway.Core.ServiceDiscovery.Nacos
{
    public class NacosServiceList
    {
        [JsonProperty(PropertyName = "count")]
        public int Count { get; set; }

        [JsonProperty(PropertyName = "doms")]
        public string[] Doms { get; set; }
    }
}
