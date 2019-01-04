using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.MicroService
{
    public static class Extensions
    {
        /// <summary>  
        /// 根据GUID获取16位的唯一字符串  
        /// </summary>  
        /// <param name="guid"></param>  
        /// <returns></returns>  
        public static string To16String(this Guid guid)
        {
            long i = 1;
            foreach (byte b in guid.ToByteArray())
                i *= ((int)b + 1);
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }
    }
}
