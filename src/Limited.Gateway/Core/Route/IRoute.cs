using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Core.Route
{
    public interface IRoute
    {
        Task<LimitedMessage> Redirect(LimitedMessage message);
    }
}
