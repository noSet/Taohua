using System;

namespace Taohua.CallSite
{
    internal class FactoryCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind { get; } = CallSiteKind.Factory;

        public override Type ServiceType { get; }

        public override Type ImplementationType => null;

        public Func<IServiceProvider, object> Factory { get; }

        public FactoryCallSite(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
            : base(lifetime)
        {
            ServiceType = serviceType;
            Factory = factory;
        }
    }
}
