using System.IO;
using Limited.Gateway.Cache;
using Limited.Gateway.Extensions;
using Limited.Gateway.Middleware;
using Limited.Gateway.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Limited.Gateway
{
    public static class GatewayExtensions
    {
        public static void AddGateway(this IServiceCollection services)
        {
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var configuration = configBuilder.Build();
            
            if (configuration.GetSection("GatewayConfig:ServiceDiscoveryProvider").Value == null)
            {
                throw new ConfigException("undefine 'GatewayConfig' node in appsettings.json file!");
            }

            var config = new GatewayConfig()
            {
                CacheProvider = configuration.GetSection("GatewayConfig:CacheProvider").Value,
                CacheAddress = configuration.GetSection("GatewayConfig:CacheAddress").Value,
                ServiceDiscoveryProvider =
                    configuration.GetSection("GatewayConfig:ServiceDiscoveryProvider").Value,
                ServiceDiscoveryAddress = configuration.GetSection("GatewayConfig:ServiceDiscoveryAddress").Value
            };

            if (config.CacheProvider.ToLower() == CacheProvider.Consul.ToLower())
            {
                services.AddSingleton<ICache, ConsulCache>();
            }
            else if (config.CacheProvider.ToLower() == CacheProvider.Redis.ToLower())
            {
                services.AddSingleton<ICache, RedisCache>();
            }
            else
            {
                throw new ConfigException($"nonsupport '{config.CacheProvider}' cache provider ");
            }

            services.AddSingleton<RouteTable>();
        }

        public static void UseGateway(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestCheckMiddleware>();
            app.UseMiddleware<RedirectMiddleware>();
        }
    }
}
