using Limited.Gateway.Cache;
using Limited.Gateway.Extensions;
using Limited.Gateway.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Limited.Gateway
{
    public static class GatewayExtensions
    {
        public static void AddLimitedGateway(this IServiceCollection services, IConfiguration configuration)
        {
            var json = configuration.GetSection("GatewayConfig");

            var config = new GatewayConfig();
            if (config == null)
            {
                throw new ConfigException("undefine 'GatewayConfig' node in appsettings.json file!");
            }

            if (config.CacheProvider.ToLower() == CacheProvider.Consul.ToLower())
            {

            }
            else if (config.CacheProvider.ToLower() == CacheProvider.Redis.ToLower())
            {
                services.AddSingleton<ICache, RedisCache>();
            }
            else
            {
                throw new ConfigException($"nonsupport '{config.CacheProvider}' cache provider ");
            }
        }

        public static void UseLimitedGateway(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestCheckMiddleware>();
        }
    }
}
