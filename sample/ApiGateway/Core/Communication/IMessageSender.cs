using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Limited.Gateway.Core.Communication
{
    public interface IMessageSender
    {
        Task<LimitedMessage> Sender(LimitedMessage message);
    }
}
