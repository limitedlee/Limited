using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public class LimitedMessage
    {
        public HttpContext Context { get; set; }

        public HttpRequestMessage RequestMessage { get; set; } = new HttpRequestMessage();

        public HttpResponseMessage ResponseMessage { get; set; } = new HttpResponseMessage();

        //public bool IsHttps { get; set; }

        //public string Scheme { get; set; }

        //public string Method { get; set; }

        //public string ContentType { get; set; }


        //public string QueryString { get; set; }

        //public long ContentLength { get; set; } = 0;

        //public string Body { get; set; }

        //public HttpStatusCode StatusCode { get; set; }

        //public ConcurrentDictionary<string, string> Cookies { get; set; } = new ConcurrentDictionary<string, string>();

        //public ConcurrentDictionary<string, string> Headers { get; set; } = new ConcurrentDictionary<string, string>();

        //public ConcurrentDictionary<string, string> Query { get; set; } = new ConcurrentDictionary<string, string>();

        //public ConcurrentDictionary<string, string> Forms { get; set; } = new ConcurrentDictionary<string, string>();

        public RouteOption Option { get; set; } = new RouteOption();

        public LimitedMessage(HttpContext _Context)
        {
            Context = _Context;
        }

        public async Task Map(HttpContext context)
        {
            RequestMessage.Method = new HttpMethod(context.Request.Method);

            if (context.Request.Body!=null)
            {
                var bodyBuffer = new byte[(long)context.Request.ContentLength];
                context.Request.Body.ReadAsync(bodyBuffer, 0, (int)context.Request.ContentLength);
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
    }
}
