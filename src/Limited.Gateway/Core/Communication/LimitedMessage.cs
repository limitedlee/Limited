using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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

        public long ContentLength { get; set; } = 0;

        public string Body { get; set; }

        public ConcurrentDictionary<string, string> Cookies { get; set; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, string> Headers { get; set; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, string> Query { get; set; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, string> Forms { get; set; } = new ConcurrentDictionary<string, string>();

        public RouteOption Option { get; set; } = new RouteOption();
    }
}
