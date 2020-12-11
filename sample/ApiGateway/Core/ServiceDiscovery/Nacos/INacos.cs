using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiClientCore.Attributes;

namespace ApiGateway.Core.ServiceDiscovery
{
    public interface INacos
    {
        [HttpGet("/nacos/v1/ns/service/list?pageNo=1&pageSize=999")]
        Task<Nacos.NacosServiceList> GetServiceList();

        [HttpGet("/nacos/v1/ns/instance/list?serviceName={serviceName}")]
        Task<Nacos.NacosInstanceList> GetInstanceList(string serviceName);
    }
}
