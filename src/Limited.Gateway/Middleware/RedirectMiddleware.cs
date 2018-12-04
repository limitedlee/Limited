using Limited.Gateway.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Middleware
{
    public class RedirectMiddleware
    {
        private readonly RequestDelegate next;
        //private readonly LimitedRequestDelegate next;
        private ILogger<RequestCheckMiddleware> logger;
        private RouteTable route;

        public RedirectMiddleware(
            RequestDelegate _next,
            //LimitedRequestDelegate _next, 
            ILogger<RequestCheckMiddleware> _logger,
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
