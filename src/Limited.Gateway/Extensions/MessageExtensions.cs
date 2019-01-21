using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway
{
    public static class MessageExtensions
    {
        public static async Task ReadFromHttpContext(this LimitedMessage message, HttpContext context)
        {
            //message.ContentLength = context.Request.ContentLength ?? 0;
            //message.ContentType = context.Request.ContentType;
            //message.IsHttps = context.Request.IsHttps;
            //message.Method = context.Request.Method;
            //message.QueryString = context.Request.QueryString.Value;
            //message.Scheme = context.Request.Scheme;
            //message.SourceHost = context.Request.Host.Value;
            //message.SourcePath = context.Request.Path.Value;

            //if ( context.Request.Cookies.Count > 0)
            //{
            //    foreach (var c in context.Request.Cookies)
            //    {
            //        message.Cookies.TryAdd(c.Key, c.Value);
            //    }
            //}
            //if (context.Request.Headers.Count > 0)
            //{
            //    foreach (var key in context.Request.Headers.Keys)
            //    {
            //        message.Headers.TryAdd(key, context.Request.Headers[key].ToString());
            //    }
            //}
            //if (context.Request.Query.Count > 0)
            //{
            //    foreach (var key in context.Request.Query.Keys)
            //    {
            //        message.Query.TryAdd(key, context.Request.Query[key].ToString());
            //    }
            //}
            //if (context.Request.HasFormContentType)
            //{
            //    var forms = await context.Request.ReadFormAsync();

            //    foreach (var form in forms)
            //    {
            //        message.Forms.TryAdd(form.Key, form.Value);
            //    }
            //}
        }

        public static async Task ReadFromResponseMessage(this LimitedMessage message, HttpResponseMessage responseMessage)
        {

        }
    }
}
