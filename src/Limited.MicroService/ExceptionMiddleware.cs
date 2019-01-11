//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;

//namespace Limited.MicroService
//{
//    public class ExceptionMiddleware
//    {
//        private readonly IHostingEnvironment env;
//        private readonly ILogger _logger;
//        private readonly RequestDelegate _next;

//        /// <summary>
//        /// </summary>
//        /// <param name="next"></param>
//        /// <param name="env"></param>
//        /// <param name="logService"></param>
//        public ExceptionMiddleware(RequestDelegate next,
//            IHostingEnvironment env,
//            ILogger<ExceptionMiddleware> logger)
//        {
//            _next = next;
//            this.env = env;
//            _logger = logger;
//        }

//        private void AddLog(RequestInfo ri, Exception ex, int level)
//        {
//            var message = GetExceptionStr(ri, ex);
//            _logger.LogError(message);
//        }


//        public async Task Invoke(HttpContext context)
//        {
//            try
//            {
//                await _next(context);
//            }
//            catch (Exception ex)
//            {
//                #region remove if exist sn

//                if (context.Request.Headers.ContainsKey("sn"))
//                {
//                    var redis = new RedisCache();

//                    var key = $"sn-{context.Request.Headers["sn"]}";
//                    if (redis.Exists(key))
//                    {
//                        redis.RemoveAsync(key);
//                    }
//                }

//                #endregion

//                #region handler error
//                var innerEx = ex.GetBaseException();
//                var request = context.Request;
//                var ri = new RequestInfo
//                {
//                    Path = $"{RequestInfo.ServerUrl}{request.Path}",
//                    Query = request.GetQueryStr(),
//                    FormData = request.GetFormStr(),
//                    Header = request.GetHeaderStr()
//                };

//                if (innerEx is MsgException)
//                {
//                    AddLog(ri, innerEx, 2);
//                    await HandleMsgExceptionAsync(context, ((MsgException)innerEx).Message, ((MsgException)innerEx).Code);
//                }
//                else
//                {
//                    await HandlerExcepitionAsync(context, innerEx);
//                    AddLog(ri, innerEx, 3);
//                }
//                #endregion
//            }
//        }

//        private string GetExceptionStr(RequestInfo ri, Exception ex)
//        {
//            var newLine = Environment.NewLine;

//            var sb = new StringBuilder();
//            sb.Append("//****************************************************\r\n");
//            sb.Append($"异常时间：({DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")})\r\n");
//            sb.Append($"出错页面：({ri.Path})\r\n");
//            sb.Append($"form参数信息：({ri.Query})\r\n");
//            sb.Append($"form参数信息：({ri.FormData})\r\n");
//            sb.Append($"header参数信息：({ri.Header})\r\n");
//            var message = ex.Message;
//            if (ex is MsgException) message = $"{((MsgException)ex).Message} {message}\r\n";

//            if (ex is ValidationException) message = $"{((ValidationException)ex).Message} {message}\r\n";

//            sb.Append($"异常信息：({message})\r\n");
//            sb.Append($"异常方法：({ex.TargetSite})\r\n");
//            sb.Append($"异常来源：({ex.Source})\r\n");
//            sb.Append("异常处理：" + newLine + ex.StackTrace.Replace("   ", "\r\n   ").Replace("--- ", "\r\n--- ") +
//                      "\r\n");
//            sb.Append("异常实例：" + newLine + ex.InnerException + "\r\n");
//            sb.Append("//*******************************************************\r\n");
//            return sb.ToString();
//        }


//        /// <summary>
//        ///     普通错误,记录日志
//        /// </summary>
//        /// <param name="context"></param>
//        /// <param name="ex"></param>
//        /// <returns></returns>
//        private Task HandlerExcepitionAsync(HttpContext context, Exception ex)
//        {
//            context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
//            return context.Response.WriteAsync(ex.Message);
//        }

//        /// <summary>
//        ///     错误验证错误
//        /// </summary>
//        /// <param name="context"></param>
//        /// <param name="field"></param>
//        /// <param name="message"></param>
//        /// <returns></returns>
//        private Task HandleValidationExceptionAsync(HttpContext context, string field, string message)
//        {
//            return context.Response.WriteAsync($"{field} {message}");
//        }

//        /// <summary>
//        ///     操作错误
//        /// </summary>
//        /// <param name="context"></param>
//        /// <param name="msg"></param>
//        /// <returns></returns>
//        private Task HandleMsgExceptionAsync(HttpContext context, string msg, int code)
//        {
//            if (code > 0 && code == 200)
//            {
//                context.Response.StatusCode = code;
//            }
//            else
//            {
//                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
//            }
//            return context.Response.WriteAsync(msg);
//        }
//    }
//}
