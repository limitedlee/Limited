using System;
using System.Collections.Generic;
using System.Text;

namespace Limited.Gateway
{
    public class CacheValue<T>
    {
        public T Value { get; set; }

        public DateTime ExpiryTime { get; set; } = default;
    }
}
