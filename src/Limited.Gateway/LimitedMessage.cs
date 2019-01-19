using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Limited.Gateway
{
    public class LimitedMessage
    {
        public bool IsHttps { get; set; }

        public string Scheme { get; set; }

        public string Method { get; set; }

        public string ContentType { get; set; }

        public string SourceHost { get; set; }

        public string TargetHost { get; set; }

        public string SourcePath { get; set; }

        public string TargetPath { get; set; }

        public string QueryString { get; set; }

        public int ContentLength { get; set; } = 0;

        public string Body { get; set; }

        public Dictionary<string, string> Cookies { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public Dictionary<string, string> Query { get; set; }

        public Dictionary<string, string> Forms { get; set; }

        public RouteOption Option { get; set; }

        public void ReadFromHttpContext(HttpContext context)
        {

        }

        public void ReadFromResponseMessage(HttpResponseMessage message)
        {

        }
    }
}
