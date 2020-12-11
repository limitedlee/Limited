using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway
{
    public class SystemParameter
    {
        /// <summary>
        /// 并行计算任务数
        /// </summary>
        public static int TaskNumber
        {
            get
            {
                var taskNum = Environment.ProcessorCount;
                if (taskNum >= 3)
                {
                    return taskNum - 2;
                }
                else
                {
                    return 2;
                }
            }
        }
    }
}
