using Limited.Gateway.Cache;
using Limited.Gateway.Extensions;
using Limited.Gateway.Security;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Middleware
{
    public class RequestCheckMiddleware
    {
        private readonly IHostingEnvironment env;
        private readonly RequestDelegate next;
        //private readonly LimitedRequestDelegate next;
        private ILogger<RequestCheckMiddleware> logger;
        private ICache cache;

        /// <summary>
        /// </summary>
        /// <param name="next"></param>
        /// <param name="env"></param>
        public RequestCheckMiddleware(
            RequestDelegate next,
            //LimitedRequestDelegate next,
            IHostingEnvironment env,
            ILogger<RequestCheckMiddleware> logger,
            ICache cache)
        {
            this.next = next;
            this.env = env;
            this.logger = logger;
            this.cache = cache;
            Console.WriteLine("RouteMiddleware init");
        }

        public async Task Invoke(HttpContext context)
        {
            var host = new HostString("127.0.0.1", 8000);
            context.Request.Host = host;

            var request = context.Request;
            if (request.Method.ToLower() == "post" && request.Headers.ContainsKey("timestamp") && request.Headers.ContainsKey("sn"))
            {
                #region check request is expired

                DateTime requestTime = ConvertStringToDateTime(request.Headers["timestamp"].ToString().SafeToLong());

                //define time out -- 设置过期时间
                var timeout = 30;

                //过期时间
                if (requestTime < DateTime.Now.AddMinutes(-timeout) || requestTime > DateTime.Now.AddMinutes(timeout))
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("请求已过期");
                }

                #endregion

                #region check request is repeat

                var key = $"sn-{request.Headers["sn"]}";

                if (await cache.Exists(key))
                {
                    context.Response.StatusCode = 406;
                    await context.Response.WriteAsync("重复请求");
                }
                else
                {
                    var node = new CacheNode<string>()
                    {
                        Key = key,
                        Data = "0",
                        CacheTime = new TimeSpan(0, timeout, 0)
                    };
                    await cache.Set(node);
                }

                await next(context);

                if (context.Response.StatusCode != 200)
                {
                    cache.Remove(key);
                }
                #endregion
            }
            else
            {
                await next(context);
            }
        }

        private bool BadRequest(HttpContext context, string message)
        {
            context.Response.StatusCode = 400;
            context.Response.WriteAsync(message);
            return false;
        }

        private DateTime ConvertStringToDateTime(long timeStamp)
        {
            try
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                DateTime dt = startTime.AddSeconds(timeStamp);
                return dt;
            }
            catch
            {
                throw new Exception("时间戳错误");
            }
        }
    }
}
