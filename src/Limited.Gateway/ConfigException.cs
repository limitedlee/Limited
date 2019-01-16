using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway
{
    public class ConfigException:Exception
    {

        public ConfigException(string message) : base(message)
        {
        }


    }
}
