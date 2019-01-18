using Consul;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Net;

namespace Limited.Gateway
{
    public class GateWayMiddleware
    {
        private readonly RequestDelegate next;
        //private readonly LimitedRequestDelegate next;
        private ILogger<GateWayMiddleware> logger;
        private RouteTable route;
        private readonly IHttpClientFactory httpClientFactory;

        public GateWayMiddleware(
            RequestDelegate _next,
            //LimitedRequestDelegate _next, 
            ILogger<GateWayMiddleware> _logger,
            RouteTable _route,
            IHttpClientFactory _httpClientFactory)
        {
            logger = _logger;
            next = _next;
            route = _route;
            httpClientFactory = _httpClientFactory;
            Console.WriteLine("RouteMiddleware init");
        }


        private async Task<HttpContext> Redirect(HttpContext context)
        {
            var path = context.Request.Path;
            var verb = context.Request.Method;

            var dic = new ConcurrentDictionary<string, HostString>();

            Parallel.ForEach(route.Cache, async (RouteOption c, ParallelLoopState state) =>
            {
                var sourceRegex = Regex.Replace(c.SourcePathRegex, "{[0-9a-zA-Z*#].*}", "[0-9a-zA-Z/].*$");
                sourceRegex = $"^{sourceRegex}";

                if (Regex.IsMatch(path, sourceRegex))
                {
                    state.Stop();
                    //取最大公共子串
                    var maxSubStr = SearchMaxSubStr(sourceRegex, path)[0];
                    var maxSubStrIndex = path.Value.IndexOf(maxSubStr);
                    //找到公共子串后面的通配符匹配的内容
                    var matchValue = path.Value.Substring(maxSubStrIndex + maxSubStr.Length);

                    var targetPath = Regex.Replace(c.TargetPathRegex, "{[0-9a-zA-Z*#].*}", matchValue);


                    Random random = new Random();
                    var a = random.Next(0, ServiceTable.Services[c.TargetService.ToLower()].Count);

                    var currentService = ServiceTable.Services[c.TargetService.ToLower()].ToArray()[a];

                    var targethost = new HostString(currentService.Address, currentService.Port);
                    dic.TryAdd(targetPath, targethost);
                }
            });


            context.Request.Host = dic.First().Value;
            context.Request.Path = new PathString(dic.First().Key);


            return context;
        }


        public async Task Invoke(HttpContext context)
        {

            //1.匹配服务
            await Redirect(context);

            var client = httpClientFactory.CreateClient();

            var url = new Uri($"{context.Request.Scheme}://{context.Request.Host.Host}:{context.Request.Host.Port}");
            client.BaseAddress = url;
            if (context.Request.Method.ToLower() == "get")
            {
                var response = await client.GetAsync(context.Request.Path);
                var content = await response.Content.ReadAsByteArrayAsync();

                using (Stream stream = new MemoryStream(content))
                {
                    if (response.StatusCode != HttpStatusCode.NotModified && context.Response.ContentLength != 0)
                    {
                        await stream.CopyToAsync(context.Response.Body);
                    }
                }
            }

            //  context.RequestServices.

            await next(context);
        }

        /// <summary>
        /// 求最长公共子串
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private List<string> SearchMaxSubStr(string s1, string s2)
        {
            List<string> maxSubStr = new List<string>();
            if (s1 == s2)
            {
                maxSubStr.Add(s1);
                return maxSubStr;
            }

            int len1 = s1.Length;
            int len2 = s2.Length;
            int maxLen = 0;
            List<string> subStr = new List<string>();
            for (int i = 0; i < len2; i++)
            {
                for (int k = 0; k < len1; k++)
                {
                    if (s2[i] == s1[k])
                    {
                        int n = i;
                        int m = k;
                        int subLen = 0;
                        while (s2[n] == s1[m])
                        {
                            n++;
                            m++;
                            subLen++;
                            if (m == len1 || n == len2)
                            {
                                break;
                            }
                        }

                        if (maxLen < subLen)
                        {
                            maxLen = subLen;
                            subStr.Clear();
                            subStr.Add((n - 1) + "," + subLen);

                        }
                        else if (maxLen == subLen)
                        {
                            if (!subStr.Contains((n - 1) + "," + subLen))
                                subStr.Add((n - 1) + "," + subLen);
                        }
                    }
                }
            }
            for (int j = 0; j < subStr.Count; j++)
            {
                string[] subStrIndex = subStr[j].Split(',');
                int len = int.Parse(subStrIndex[1]);
                int index = int.Parse(subStrIndex[0]);
                string sub = "";
                for (int k = index - len + 1; k <= index; k++)
                {
                    sub = sub + s2.Substring(k, 1);
                }
                maxSubStr.Add(sub);
            }

            return maxSubStr;
        }
    }
}
