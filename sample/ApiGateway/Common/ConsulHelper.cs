using Microsoft.AspNetCore.Builder;

namespace Limited.Gateway
{
    public static class DiscoveryHelper
    {
        public static string DiscoveryUrl { get; set; }

        public static void UseDiscovery(this IApplicationBuilder app, string discoveryUrl)
        {
            DiscoveryUrl = discoveryUrl;
        }
    }
}
