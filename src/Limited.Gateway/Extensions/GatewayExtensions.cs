using Limited.Gateway.Middleware;
using Microsoft.AspNetCore.Builder;

namespace Limited.Gateway
{
    public static class GatewayExtensions
    {
        public static void UseLmtGateway(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestCheckMiddleware>();
        }
    }
}
