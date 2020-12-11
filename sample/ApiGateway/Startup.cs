using ApiGateway.Core.ServiceDiscovery;
using Limited.Gateway;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using WebApiClientCore;

namespace Limited
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        [System.Obsolete]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpApi<INacos>(x => 
            {
                x.HttpHost = new System.Uri(Configuration.GetSection("GatewayConfig:DiscoveryUrl").Value);
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddGateway();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //暂时废弃此中间件。意义不大
            //app.UseMiddleware<RequestCheckMiddleware>();
            app.UseGateway(Configuration.GetSection("GatewayConfig:ConsulUrl").Value);
            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
