using Consul;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway
{
    /// <summary>
    /// 服务表
    /// </summary>
    public class ServiceTable
    {
        public static ConcurrentDictionary<string, ConcurrentBag<AgentService>> Services { get; set; } = new ConcurrentDictionary<string, ConcurrentBag<AgentService>>();
    }
}
