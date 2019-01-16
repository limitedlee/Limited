using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public class GateWayMiddleware
    {
        private readonly RequestDelegate next;
        //private readonly LimitedRequestDelegate next;
        private ILogger<GateWayMiddleware> logger;
        private RouteTable route;

        public GateWayMiddleware(
            RequestDelegate _next,
            //LimitedRequestDelegate _next, 
            ILogger<GateWayMiddleware> _logger,
            RouteTable _route)
        {
            logger = _logger;
            next = _next;
            route = _route;
            Console.WriteLine("RouteMiddleware init");
        }

        public async Task Invoke(HttpContext context)
        {

            //  context.RequestServices.

            await next(context);
        }
    }
}
