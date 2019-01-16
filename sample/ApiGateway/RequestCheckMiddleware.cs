using Limited.Gateway;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ApiGateway
{
    public class RequestCheckMiddleware
    {
        private readonly IHostingEnvironment env;
        private readonly RequestDelegate next;
        //private readonly LimitedRequestDelegate next;
        private ILogger<RequestCheckMiddleware> logger;

        /// <summary>
        /// </summary>
        /// <param name="next"></param>
        /// <param name="env"></param>
        public RequestCheckMiddleware(
            RequestDelegate next,
            //LimitedRequestDelegate next,
            IHostingEnvironment env,
            ILogger<RequestCheckMiddleware> logger)
        {
            this.next = next;
            this.env = env;
            this.logger = logger;
            Console.WriteLine("RouteMiddleware init");
        }

        public async Task Invoke(HttpContext context)
        {
            var request = context.Request;
            if (request.Method.ToLower() == "post" && request.Headers.ContainsKey("timestamp") && request.Headers.ContainsKey("sn"))
            {
                #region check request is expired

                if (!long.TryParse(request.Headers["timestamp"].ToString(), out long timestamp))
                {
                    context.Response.StatusCode = 400;
                    context.Response.WriteAsync("timestamp参数不合法");
                    return;
                }

                DateTime requestTime = ConvertStringToDateTime(timestamp);

                //define time out -- 设置过期时间
                var timeout = 15;

                //过期时间
                if (requestTime < DateTime.Now.AddMinutes(-timeout) || requestTime > DateTime.Now.AddMinutes(timeout))
                {
                    context.Response.StatusCode = 403;
                    context.Response.WriteAsync("请求已过期");
                    return;
                }

                #endregion

                #region check request is repeat

                var key = $"gateway/serialnumber/{request.Headers["sn"]}";

                if (await ConsulHelper.Exists(key))
                {
                    context.Response.StatusCode = 406;
                    await context.Response.WriteAsync("重复请求");
                }
                else
                {
                    var node = new CacheValue<string>()
                    {
                        Value = "0",
                        ExpiryTime = DateTime.Now.AddMinutes(timeout)
                    };
                    await ConsulHelper.Set(key, node);
                }

                await next(context);

                if (context.Response.StatusCode != 200)
                {
                    ConsulHelper.Remove(key);
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
