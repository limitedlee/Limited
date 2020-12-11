using Limited.Gateway.Core.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Limited.Gateway.Core.LoadBalance;

namespace Limited.Gateway.Core.Route
{
    public class DefaultRouteRedirect : IRoute
    {

        public ILoadBalance loadBalance;

        public DefaultRouteRedirect(ILoadBalance _loadBalance)
        {
            loadBalance = _loadBalance;
        }

        public async Task<LimitedMessage> Redirect(LimitedMessage message)
        {
            var route = new RouteTable();

            //如果未配置服务,即通过服务发现自动转发
            if (route.Cache == null || route.Cache.Count == 0)
            {
                //以Request的Path的第一个节点作为服务名  
                //如 /base/member/add  服务名为base
                var serviceName = string.Empty;
                var path = message.Context.Request.Path.Value.TrimStart('/');
                if (path.Length == 0)
                {
                    message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.Forbidden;
                    return message;
                }

                var indexFirstSeparator = path.IndexOf('/');
                serviceName = path.Substring(0, indexFirstSeparator).ToLower();
                if (serviceName.Length == 0)
                {
                    message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return message;
                }
                message.Option = new RouteOption
                {
                    HttpMethods = new string[2] { "Get", "Post" },
                    SourcePathRegex = "/" + serviceName + "/{something}",
                    TargetPathRegex = "/" + serviceName + "/{something}",
                    //TargetPathRegex = "/api/{something}",
                    TargetService = serviceName
                };
            }

            Parallel.ForEach(route.Cache, async (RouteOption c, ParallelLoopState state) =>
            {
                var sourceRegex = Regex.Replace(c.SourcePathRegex, "{[0-9a-zA-Z*#].*}", "[0-9a-zA-Z/].*$");
                sourceRegex = $"^{sourceRegex}";

                if (Regex.IsMatch(message.Context.Request.Path, sourceRegex))
                {
                    state.Stop();
                    message.Option = c;
                }
            });

            if (message.Option == null)
            {
                message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return message;
            }

            if (!ServiceCache.Services.ContainsKey(message.Option.TargetService.ToLower()))
            {
                message.ResponseMessage.StatusCode = System.Net.HttpStatusCode.NotFound;
                return message;
            }

            var currentService = await loadBalance.Get(message);

            var requestRegex = Regex.Replace(message.Option.SourcePathRegex, "{[0-9a-zA-Z*#].*}", "[0-9a-zA-Z/].*$");
            requestRegex = $"^{requestRegex}";
            //取最大公共子串
            var maxSubStr = GetMaxSubStr(requestRegex, message.Context.Request.Path)[0];
            var maxSubStrIndex = message.Context.Request.Path.Value.IndexOf(maxSubStr);
            //找到公共子串后面的通配符匹配的内容
            var matchValue = message.Context.Request.Path.Value.Substring(maxSubStrIndex + maxSubStr.Length);
            var targethost = new HostString(currentService.Address, currentService.Port);
            var targetPath = Regex.Replace(message.Option.TargetPathRegex, "{[0-9a-zA-Z*#].*}", matchValue);

            var urlString = $"{message.Context.Request.Scheme}://{targethost.Value}{targetPath}";
            message.RequestMessage.RequestUri = new Uri(urlString);

            return message;
        }


        /// <summary>
        /// 求最长公共子串
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <returns></returns>
        private List<string> GetMaxSubStr(string s1, string s2)
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
