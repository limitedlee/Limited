using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

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

            ConsulUrl = configuration.GetSection("GatewayConfig:ConsulUrl").Value;
            services.AddSingleton<RouteTable>();
        }

        public static void UseGateway(this IApplicationBuilder app)
        {
            app.UseMiddleware<GateWayMiddleware>();
            app.UseConsul(ConsulUrl);
        }
    }
}
