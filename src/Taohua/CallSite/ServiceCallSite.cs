using System;

namespace Taohua.CallSite
{
    internal abstract class ServiceCallSite
    {
        public abstract CallSiteKind Kind { get; }

        public abstract Type ServiceType { get; }

        public abstract Type ImplementationType { get; }

        public ServiceLifetime Lifetime { get; }

        public ServiceCallSite(ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }
}
