using System;
using System.Reflection;

namespace Taohua.CallSite
{
    internal class ConstructorCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind { get; } = CallSiteKind.Constructor;

        public override Type ServiceType { get; }

        public override Type ImplementationType => ConstructorInfo.DeclaringType;

        internal ConstructorInfo ConstructorInfo { get; }

        internal ServiceCallSite[] ParameterCallSites { get; }

        public ConstructorCallSite(Type serviceType, ConstructorInfo constructorInfo, ServiceCallSite[] parameterCallSites, ServiceLifetime lifetime)
            : base(lifetime)
        {
            ServiceType = serviceType;
            ConstructorInfo = constructorInfo;
            ParameterCallSites = parameterCallSites;
        }
    }
}
