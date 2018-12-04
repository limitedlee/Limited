using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Limited.MicroService
{
   public static class MicroServiceExtension
    {
        public static void UseMicroService(this IApplicationBuilder app, IApplicationLifetime lifetime,ServiceInfo service)
        {
            //读取配置文件
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = configBuilder.Build();

            //using (var consulClient = new ConsulClient(x => { x.Address = new Uri(service.DCAddress); }))
            //{
            //    EnvironmentVariableTarget asr = new AgentServiceRegistration
            //    {
            //        Address = ip,
            //        Port = Convert.ToInt32(port),
            //        ID = serviceId,
            //        Name = serviceName,
            //        Check = new AgentServiceCheck
            //        {
            //            DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
            //            HTTP = $"http://{ip}:{port}/api/Health",//健康检查访问的地址
            //            Interval = TimeSpan.FromSeconds(5),   //健康检查的间隔时间
            //            Timeout = TimeSpan.FromSeconds(2),     //多久代表超时
            //        },
            //    };
            //    consulClient.Agent.ServiceRegister(asr).Wait();
            //}
            ////注销Consul 
            //lifetime.ApplicationStopped.Register(() =>
            //{
            //    using (var consulClient = new ConsulClient(ConsulConfig))
            //    {
            //        consulClient.Agent.ServiceDeregister(serviceId).Wait();  //从consul集群中移除服务
            //    }
            //});

            app.Map("/Health", ab =>
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
