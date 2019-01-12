using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace Limited.MicroService
{
    public class ServiceConfig
    {
        private string version;

        /// <summary>
        /// 服务英文名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 服务中文名
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///微服务所生成的XML文件名 
        /// </summary>
        public string XmlName { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version
        {
            get
            {
                return version;
            }
            set
            {
                var tempValue = value;
                var regex = new Regex(@"\d+(\.\d+){0,2}");
                if (!regex.IsMatch(tempValue))
                {
                    throw new Exception("版本号不符合规范.");
                }
                version = tempValue;
            }
        }

        /// <summary>
        /// 本地地址,用于回调
        /// </summary>
        public string LocalAddress { get; set; }

        /// <summary>
        /// Consul服务地址
        /// </summary>
        public string DCAddress { get; set; } = "http://127.0.0.1:8500";
    }
}
