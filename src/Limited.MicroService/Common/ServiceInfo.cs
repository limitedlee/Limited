using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.MicroService
{
    public class ServiceInfo
    {
        /// <summary>
        /// 服务名,需全英文
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// 服务展示标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public Version Version { get; set; } = new Version(1, 0);

        /// <summary>
        ///微服务所生成的XML文件名 
        /// </summary>
        public string XmlName { get; set; }
    }
}
