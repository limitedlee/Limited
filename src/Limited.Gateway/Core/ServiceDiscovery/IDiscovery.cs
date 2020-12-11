using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway.Core.ServiceDiscovery
{
    public interface IDiscovery
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        List<MicroService> GetServices();
    }
}
