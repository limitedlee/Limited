using Limited.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Middleware
{
    public class RouteMiddleware
    {
        private ILogger<RequestCheckMiddleware> logger;
        private readonly RequestDelegate next;
        private Route route;

        public RouteMiddleware(RequestDelegate _next, ILogger<RequestCheckMiddleware> _logger, Route _route)
        {
            logger = _logger;
            next = _next;
            route = _route;
            Console.WriteLine("RouteMiddleware init");
        }

        public async Task Invoke(HttpContext context)
        {
            await next(context);
        }
    }
}
