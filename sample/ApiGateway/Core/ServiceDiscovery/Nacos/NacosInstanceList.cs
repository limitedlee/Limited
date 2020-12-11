using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Core.ServiceDiscovery.Nacos
{
    public class NacosInstanceList
    {
        [JsonProperty(PropertyName = "dom")]
        public string Dom { get; set; }

        [JsonProperty(PropertyName = "cacheMillis")]
        public int CacheMillis { get; set; }

        [JsonProperty(PropertyName = "useSpecifiedURL")]
        public bool UseSpecifiedURL { get; set; }

        [JsonProperty(PropertyName = "hosts")]
        public Host[] Hosts { get; set; }
    }


    public class Host {
        [JsonProperty(PropertyName = "valid")]
        public bool Valid { get; set; }

        [JsonProperty(PropertyName = "marked")]
        public bool Marked { get; set; }

        [JsonProperty(PropertyName = "instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty(PropertyName = "port")]
        public int Port { get; set; }

        [JsonProperty(PropertyName = "ip")]
        public string IP { get; set; }

        [JsonProperty(PropertyName = "weight")]
        public float Weight { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public dynamic Metadata { get; set; }
    }
}
