using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.Extensions.PlatformAbstractions;

namespace Limited.MicroService
{
    public static class MicroServiceExtension
    {
        public static void AddMicroService(this IServiceCollection services, ServiceConfig serviceConfig)
        {
            //Swagger配置
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc(serviceConfig.Name.ToLower(), new Info
                {
                    Title = serviceConfig.DisplayName,
                    Version = serviceConfig.Version
                });
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = Path.Combine(basePath, serviceConfig.XmlName);
                option.IncludeXmlComments(xmlPath);
            });

           // services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "My API", Version = "v1"}); });

            //跨域配置
            services.AddCors(opt =>
            {
                opt.AddPolicy("AllowDomain",
                    builder => { builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().AllowCredentials(); });
            });
        }


        public static void UseMicroService(this IApplicationBuilder app, IHostingEnvironment env,
            IApplicationLifetime lifetime, ServiceConfig serviceInfo)
        {
            //读取配置文件
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var config = configBuilder.Build();

            var serviceId = $" {serviceInfo.Name}-{serviceInfo.LocalAddress.Replace(':', '-')}";
            var ip = serviceInfo.LocalAddress.Split(':')[0];
            var port = serviceInfo.LocalAddress.Split(':')[1];

            using (var consulClient = new ConsulClient(x => { x.Address = new Uri(serviceInfo.DCAddress); }))
            {
                var asr = new AgentServiceRegistration
                {
                    Address = ip,
                    Port = int.Parse(port),
                    ID = serviceId,
                    Name = serviceInfo.Name,
                    Check = new AgentServiceCheck
                    {
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                        HTTP = $"http://{serviceInfo.LocalAddress}/health", //健康检查访问的地址
                        Interval = TimeSpan.FromSeconds(2), //健康检查的间隔时间
                        Timeout = TimeSpan.FromSeconds(1), //多久代表超时
                    },
                };
                consulClient.Agent.ServiceRegister(asr).Wait();
            }

            //注销Consul 
            lifetime.ApplicationStopped.Register(() =>
            {
                using (var consulClient = new ConsulClient(x => { x.Address = new Uri(serviceInfo.DCAddress); }))
                {
                    consulClient.Agent.ServiceDeregister(serviceId).Wait(); //从consul集群中移除服务
                }
            });

            app.Map("/health", ab => { ab.Run(async context => { await context.Response.WriteAsync("ok"); }); });

            if (!env.IsProduction())
            {
//                app.UseSwagger();
//                app.UseSwaggerUI(c => { c.SwaggerEndpoint($"/swagger/swagger.json", serviceInfo.Name); });
                app.UseSwagger(opt => { opt.RouteTemplate = "{documentName}/swagger.json"; });
                app.UseSwaggerUI(opt => { opt.SwaggerEndpoint($"/{serviceInfo.Name.ToLower()}/swagger.json", serviceInfo.DisplayName); });
            }

            app.UseAuthentication();
        }

        public static void UseMicroService(this IApplicationBuilder app, IHostingEnvironment env,
            IApplicationLifetime lifetime, Action<ServiceConfig> action)
        {
            var serviceInfo = new ServiceConfig();
            action.Invoke(serviceInfo);
            UseMicroService(app, env, lifetime, serviceInfo);
        }
    }
}