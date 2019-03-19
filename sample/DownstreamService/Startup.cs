using Limited.MicroService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DownstreamService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private ServiceConfig service;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            
            service=new ServiceConfig();
            service.Name = "order";
            service.DisplayName = "订单服务";
            service.Version = "1.0";
            service.XmlName = "DownstreamService.xml";
            service.LocalAddress = Configuration.GetSection("ServiceAddress").Value;
            service.DCAddress = "http://master.rpdns.com:8500";
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMicroService(service);
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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

            app.UseMicroService(env, lifetime, service);

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
