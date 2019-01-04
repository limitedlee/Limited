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

            return WebHost.CreateDefaultBuilder(args)
                .UseUrls($"http://0.0.0.0:{config.GetSection("config").Value}")
                .UseStartup<Startup>();
        }
    }
}
