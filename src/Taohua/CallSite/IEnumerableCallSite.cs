using System;
using System.Collections.Generic;

namespace Taohua.CallSite
{
    internal class IEnumerableCallSite : ServiceCallSite
    {
        public override CallSiteKind Kind { get; } = CallSiteKind.IEnumerable;

        public override Type ServiceType => typeof(IEnumerable<>).MakeGenericType(ItemType);

        public override Type ImplementationType => ItemType.MakeArrayType();

        internal Type ItemType { get; }

        internal ServiceCallSite[] ServiceCallSites { get; }

        public IEnumerableCallSite(Type itemType, ServiceCallSite[] serviceCallSites)
            : base(ServiceLifetime.Transient)
        {
            ItemType = itemType;
            ServiceCallSites = serviceCallSites;
        }
    }
}
