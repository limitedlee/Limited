using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var items = new List<Limited.Gateway.RouteOption>();

            var baseRoute = new Limited.Gateway.RouteOption()
            {
                SourcePathRegex = @"/a/{everything}",
                TargetService = "DownstreamService",
                TargetPathRegex = @"/api/{everything}",
                Version = "1.0"
            };

            items.Add(baseRoute);

            var orderRoute = new Limited.Gateway.RouteOption()
            {
                SourcePathRegex = @"OrderApi/\w*",
                TargetService = "Order",
                TargetPathRegex = @"Order/\w*",
                Version = "1.0"
            };
            items.Add(orderRoute);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(items);

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>();
    }
}
