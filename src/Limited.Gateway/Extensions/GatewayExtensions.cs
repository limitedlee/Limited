using Limited.Gateway.Core.Communication;
using Limited.Gateway.Core.LoadBalance;
using Limited.Gateway.Core.Route;
using Limited.Gateway.Core.Route.LoadBalance;
using Limited.Gateway.Core.ServiceDiscovery;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public static class GatewayExtensions
    {
        private static string ConsulUrl = default;

        public static void AddGateway(this IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = configBuilder.Build();

            if (configuration.GetSection("GatewayConfig:ConsulUrl").Value == null)
            {
                throw new ConfigException("undefine 'GatewayConfig' node in appsettings.json file!");
            }
            services.AddSwaggerGen(opt => { opt.SwaggerDoc("Gate", new Info { Title = "网关服务", Version = "v1" }); });

            ConsulUrl = configuration.GetSection("GatewayConfig:ConsulUrl").Value;
            services.AddSingleton<RouteTable>();
            services.AddTransient<Microsoft.Extensions.Hosting.IHostedService, ServiceTask>();
            services.AddSingleton<IMessageSender, DefaultHttpMessageSender>();
            services.AddSingleton<ILoadBalance, RandomLoadBalance>();
            services.AddSingleton<IRoute, DefaultRouteRedirect>();
            services.AddHttpClient();
        }

        public static void UseGateway(this IApplicationBuilder app)
        {
            app.UseMiddleware<GateWayMiddleware>();
            app.UseConsul(ConsulUrl);

            ServiceTask.RoundRobinService();
            app.UseSwagger();
            app.UseSwaggerUI(opts =>
            {
                foreach (var s in ServiceCache.Services)
                {
                    var v = s.Value.ToArray()[0];
                    opts.SwaggerEndpoint($"/{s.Key}/swagger.json", v.DisplayName);
                }
            });
        }
    }
}
