using Limited.Gateway.Core.Communication;
using Limited.Gateway.Core.Route;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public class GateWayMiddleware
    {
        private readonly RequestDelegate next;
        private ILogger<GateWayMiddleware> logger;
        private IMessageSender messageSender;
        private IRoute route;

        public GateWayMiddleware(
            RequestDelegate _next,
            ILogger<GateWayMiddleware> _logger,
            IMessageSender _messageSender,
            IRoute _route)
        {
            logger = _logger;
            next = _next;
            messageSender = _messageSender;
            route = _route;
        }

        public async Task Invoke(HttpContext context)
        {
            var hasSwaggerJson = context.Request.Path.Value.ToLower().IndexOf("swagger.json") > -1;

            if (context.Request.Path.Value.ToLower().IndexOf("/swagger") > -1
                && !hasSwaggerJson)
            {
                await next(context);
            }
            else
            {
                var message = new LimitedMessage(context);
                await message.MapRequest(context);

                if (hasSwaggerJson)
                {
                    var swaggerRoute = new SwaggerRouteRedirect();
                    message = await swaggerRoute.Redirect(message);
                }
                else
                {
                    message = await route.Redirect(message);
                }

                if (message.ResponseMessage.StatusCode == HttpStatusCode.OK)
                {
                    message = await messageSender.Sender(message);
                }

                message.MapResponse(ref context);
            }
        }

    }
}
