using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.MicroService
{
    public class ServiceInfo
    {
        public string ServiceName { get; set; }

        public string Title { get; set; }

        public Version Version { get; set; } = new Version(1, 0);

        public string XmlName { get; set; }

        public string DCAddress { get; set; } = "http://127.0.0.1:8500";
    }
}
