using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace DownstreamService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            //读取配置文件
            var configBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            var config = configBuilder.Build();

            var ip = config.GetSection("ServiceAddress").Value.Split(':')[0];
            var port = config.GetSection("ServiceAddress").Value.Split(':')[1];

            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://{ip}:{port}")
                .UseStartup<Startup>();
        }
    }
}
