using Limited.Gateway.Core.Route.LoadBalance;
using Limited.Gateway.Core.ServiceDiscovery;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Core.Route
{
    public class SwaggerRouteRedirect : IRoute
    {
        public async Task<LimitedMessage> Redirect(LimitedMessage message)
        {
            var route = new RouteTable();

            //通过服务发现自动转发
            var serviceName = string.Empty;

            if (message.Context.Request.Path.Value.ToLower().IndexOf("swagger.json") == -1)
            {
                message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return message;
            }

            var indexFirstSeparator = message.Context.Request.Path.Value.IndexOf("-swagger.json");
            serviceName = message.Context.Request.Path.Value.Substring(1, indexFirstSeparator-1).ToLower();

            if (serviceName.Length == 0)
            {
                message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return message;
            }
            message.Option = new RouteOption
            {
                TargetService = serviceName
            };

            if (message.Option == null)
            {
                message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return message;
            }

            if (!ServiceCache.Services.ContainsKey(message.Option.TargetService.ToLower()))
            {
                message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return message;
            }

            var loadBalance = new RandomLoadBalance();
            var currentService = await loadBalance.Get(message);

            var targethost = new HostString(currentService.Address, currentService.Port);
            var targetPath = message.Context.Request.Path.Value;

            var urlString = $"{message.Context.Request.Scheme}://{targethost.Value}{targetPath}";
            message.RequestMessage.RequestUri = new Uri(urlString);

            return message;
        }
    }
}
