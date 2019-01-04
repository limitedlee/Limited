using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Limited.MicroService
{
    public static class MicroServiceExtension
    {
        public static void UseMicroService(this IApplicationBuilder app, IApplicationLifetime lifetime, Action<ServiceInfo, ServiceDiscoveryConfig> action)
        {
            //读取配置文件
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = configBuilder.Build();

            ServiceInfo serviceInfo = new ServiceInfo();
            ServiceDiscoveryConfig discoveryConfig = new ServiceDiscoveryConfig();

            action.Invoke(serviceInfo, discoveryConfig);

            var serviceId = $" {serviceInfo.ServiceName}-{ Guid.NewGuid().To16String()}";
            var ip = discoveryConfig.ServiceAddress.Split(':')[0];
            var port = discoveryConfig.ServiceAddress.Split(':')[1];

            using (var consulClient = new ConsulClient(x => { x.Address = new Uri(discoveryConfig.DCAddress); }))
            {
                var asr = new AgentServiceRegistration
                {
                    Address = ip,
                    Port = int.Parse(port),
                    ID = serviceId,
                    Name = serviceInfo.Title,
                    Check = new AgentServiceCheck
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                        HTTP = $"http://{discoveryConfig.ServiceAddress}/health",//健康检查访问的地址
                        Interval = TimeSpan.FromSeconds(5),   //健康检查的间隔时间
                        Timeout = TimeSpan.FromSeconds(1),     //多久代表超时
                    },
                };
                consulClient.Agent.ServiceRegister(asr).Wait();
            }
            //注销Consul 
            lifetime.ApplicationStopped.Register(() =>
            {
                using (var consulClient = new ConsulClient(x => { x.Address = new Uri(discoveryConfig.DCAddress); }))
                {
                    consulClient.Agent.ServiceDeregister(serviceId).Wait();  //从consul集群中移除服务
                }
            });

            app.Map("/health", ab =>
            {
                ab.Run(async context =>
                {
                    Console.WriteLine("健康检查" + DateTime.Now);
                    await context.Response.WriteAsync("ok");
                });
            });
        }
    }
}
