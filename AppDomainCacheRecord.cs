using System;

namespace AppDomainMessaging
{
    internal sealed class AppDomainCacheRecord
    {
        public AppDomain Domain { get; }

        public AppDomainProxy Proxy { get; }

        public int TTL { get; }

        public DateTime ExpiredAfter { get; }

        public bool Expired => DateTime.Now.Subtract(ExpiredAfter).Ticks > 0;

        public AppDomainCacheRecord(AppDomain domain, AppDomainProxy proxy, int ttl = 30000)
        {
            Domain = domain;
            Proxy = proxy;
            TTL = ttl;
            ExpiredAfter = DateTime.Now.AddMilliseconds(ttl);
        }
    }
}
