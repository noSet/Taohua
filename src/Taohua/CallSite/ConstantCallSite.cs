using System;

namespace Taohua.CallSite
{
    internal class ConstantCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind { get; } = CallSiteKind.Constant;

        public override Type ServiceType { get; }

        public override Type ImplementationType => DefaultValue.GetType();

        internal object DefaultValue { get; }

        public ConstantCallSite(Type serviceType, object defaultValue)
            : base(ServiceLifetime.Singleton)
        {
            ServiceType = serviceType;
            DefaultValue = defaultValue;
        }
    }
}
