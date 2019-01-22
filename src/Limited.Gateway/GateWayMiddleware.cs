using Consul;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Net;
using System.Text;
using Limited.Gateway.Core.ServiceDiscovery;
using Limited.Gateway.Core.Communication;
using Limited.Gateway.Core.Route;

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
            var message = new LimitedMessage(context);
            await message.MapRequest(context);

            message = await route.Redirect(message);

            message = await messageSender.Sender(message);

            message.MapResponse(ref context);

            //await next(context);
        }

    }
}
