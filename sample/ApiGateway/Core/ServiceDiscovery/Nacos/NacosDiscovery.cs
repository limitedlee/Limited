using ApiGateway.Core.ServiceDiscovery;
using ApiGateway.Core.ServiceDiscovery.Nacos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using WebApiClientCore;

namespace Limited.Gateway.Core.ServiceDiscovery
{
    public class NacosDiscovery : IDiscovery
    {
        string DiscoveryUrl = string.Empty;

        public NacosDiscovery()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            DiscoveryUrl = builder.GetSection("GatewayConfig:DiscoveryUrl").Value;
        }

        public List<MicroService> GetServices()
        {
            var httpClient = new HttpClient();

            var services = new ServiceCollection();

            var serviceProvider = services.BuildServiceProvider();

            var httpApiOpt = new HttpApiOptions { HttpHost = new Uri(DiscoveryUrl) };

            var nacosApi = HttpApi.Create<INacos>(httpClient, serviceProvider, httpApiOpt);
            NacosServiceList items = nacosApi.GetServiceList().Result;

            foreach (var s in items.Doms)
            {
                var instanceList = nacosApi.GetInstanceList(s).Result;

                Console.WriteLine("a");
            }

            return new List<MicroService>();
        }
    }
}
