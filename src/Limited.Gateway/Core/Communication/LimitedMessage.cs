using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Limited.Gateway
{
    public class LimitedMessage
    {
        public HttpContext Context { get; set; }

        public HttpRequestMessage RequestMessage { get; set; } = new HttpRequestMessage();

        public HttpResponseMessage ResponseMessage { get; set; } = new HttpResponseMessage();

        public RouteOption Option { get; set; } = new RouteOption();

        public LimitedMessage(HttpContext _Context)
        {
            Context = _Context;
        }

        public async Task MapRequest(HttpContext context)
        {
            RequestMessage.Method = new HttpMethod(context.Request.Method);

            if (context.Request.Body != null && context.Request.ContentLength != null)
            {
                var bodyBuffer = new byte[(long)context.Request.ContentLength];
                await context.Request.Body.ReadAsync(bodyBuffer, 0, (int)context.Request.ContentLength);
                RequestMessage.Content = new ByteArrayContent(bodyBuffer);

                RequestMessage.Content.Headers.TryAddWithoutValidation("Content-Type", new[] { context.Request.ContentType });
                if (context.Request.Headers.ContainsKey("Content-Language"))
                {
                    RequestMessage.Content.Headers.TryAddWithoutValidation("Content-Language", context.Request.Headers["Content-Language"].ToString());
                }
                if (context.Request.Headers.ContainsKey("Content-Location"))
                {
                    RequestMessage.Content.Headers.TryAddWithoutValidation("Content-Location", context.Request.Headers["Content-Location"].ToString());
                }
                if (context.Request.Headers.ContainsKey("Content-Range"))
                {
                    RequestMessage.Content.Headers.TryAddWithoutValidation("Content-Range", context.Request.Headers["Content-Range"].ToString());
                }
                if (context.Request.Headers.ContainsKey("Content-MD5"))
                {
                    RequestMessage.Content.Headers.TryAddWithoutValidation("Content-MD5", context.Request.Headers["Content-MD5"].ToString());
                }
                if (context.Request.Headers.ContainsKey("Content-Disposition"))
                {
                    RequestMessage.Content.Headers.TryAddWithoutValidation("Content-Disposition", context.Request.Headers["Content-Disposition"].ToString());
                }
                if (context.Request.Headers.ContainsKey("Content-Encoding"))
                {
                    RequestMessage.Content.Headers.TryAddWithoutValidation("Content-Encoding", context.Request.Headers["Content-Encoding"].ToString());
                }
            }

            foreach (var header in context.Request.Headers)
            {
                if (header.Key != "host")
                {
                    RequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
        }

        public void MapResponse(ref HttpContext context)
        {
            foreach (var httpResponseHeader in ResponseMessage.Headers)
            {
                if (!context.Response.Headers.ContainsKey(httpResponseHeader.Key))
                {
                    context.Response.Headers.Add(httpResponseHeader.Key, new StringValues(httpResponseHeader.Value.ToString()));
                }
            }

            foreach (var httpResponseHeader in ResponseMessage.Content.Headers)
            {
                if (!context.Response.Headers.ContainsKey(httpResponseHeader.Key))
                {
                    context.Response.Headers.Add(httpResponseHeader.Key, new StringValues(httpResponseHeader.Value.ToList()[0]));
                }
            }

            var content = ResponseMessage.Content.ReadAsByteArrayAsync().Result;
            if (!context.Response.Headers.ContainsKey("Content-Length"))
            {
                context.Response.Headers.Add("Content-Length", new StringValues(content.Length.ToString()));
            }

            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;

                httpContext.Response.StatusCode = (int)ResponseMessage.StatusCode;

                return Task.CompletedTask;
            }, context);

            using (Stream stream = new MemoryStream(content))
            {
                if (ResponseMessage.StatusCode != HttpStatusCode.NotModified && context.Response.ContentLength != 0)
                {
                     stream.CopyToAsync(context.Response.Body);
                }
            }
        }
    }
}
