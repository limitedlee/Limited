using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Core.LoadBalance
{
    public interface ILoadBalance
    {
        Task<MicroService> Get(LimitedMessage message);
    }
}
