using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Core.Communication
{
    public class DefaultHttpMessageSender : IMessageSender
    {
        private readonly IHttpClientFactory httpClientFactory;

        public DefaultHttpMessageSender(IHttpClientFactory _httpClientFactory)
        {
            this.httpClientFactory = _httpClientFactory;
        }

        public async Task<LimitedMessage> Sender(LimitedMessage message)
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = message.RequestMessage.RequestUri;

            if (message.Context.Request.QueryString.HasValue)
            {
                var url = message.RequestMessage.RequestUri.AbsoluteUri + message.Context.Request.QueryString.Value;
                message.RequestMessage.RequestUri = new Uri(url);
            }
            
            HttpResponseMessage responseMessage = null;

            message.ResponseMessage = await client.SendAsync(message.RequestMessage);



            return message;

        }
    }
}
