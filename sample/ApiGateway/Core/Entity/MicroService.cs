using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway.Core
{
    /// <summary>
    /// 微服务描述
    /// </summary>
    public class MicroService
    {
        /// <summary>
        /// 服务id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 展示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 服务端口
        /// </summary>
        public int Port { get; set; }
    }
}
