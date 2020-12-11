using Limited.Gateway.Core.LoadBalance;
using Limited.Gateway.Core.ServiceDiscovery;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Limited.Gateway.Core.Route.LoadBalance
{
    public class RandomLoadBalance : ILoadBalance
    {
        public async Task<MicroService> Get(LimitedMessage message)
        {
            var microServices = ServiceCache.Services[message.Option.TargetService.ToLower()];
            var count = microServices.Count;
            Random random = new Random();
            var num = random.Next(0, count);
            var currentService = microServices.ToArray()[num];
            return currentService;
        }
    }
}
