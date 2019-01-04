using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Limited.MicroService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace DownstreamService
{
    public class Startup
    {
        private readonly ServiceInfo info;
        private readonly ServiceDiscoveryConfig sdConfig;
        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            info = new ServiceInfo
            {
                ServiceName = "DownstreamService",
                Title = "订单服务",
                Version = new Version(1, 0),
                XmlName = "DownstreamService.xml"
            };

            var address = configuration.GetSection("ServiceAddress").Value;

            sdConfig = new ServiceDiscoveryConfig
            {
                // EndPoint = new System.Net.IPEndPoint( new IPAddress( address.Split(":"))
                ServiceAddress = address
            };
        }



        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseMicroService(lifetime, (service,config)=> 
            {
                service = info;
                config = sdConfig;
            } );

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
