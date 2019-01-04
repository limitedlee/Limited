using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Limited.MicroService
{
    public class ServiceDiscoveryConfig
    {
        /// <summary>
        /// Consul服务地址
        /// </summary>
        public string DCAddress { get; set; } = "http://127.0.0.1:8500";

        /// <summary>
        /// 终结点
        /// </summary>
        public string ServiceAddress { get; set; }
    }
}
