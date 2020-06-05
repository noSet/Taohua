using System;
using System.Collections.Generic;

namespace Taohua.CallSite
{
    internal class CallSiteChain
    {
        private readonly Type _serviceType;
        private readonly HashSet<Type> _callSiteChain = new HashSet<Type>();

        public CallSiteChain(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public void CheckCircularDependency(Type serviceType)
        {
            if (_callSiteChain.Contains(serviceType))
            {
                throw new InvalidOperationException($"创建{_serviceType}类型时发现循环依赖");
            }
        }

        public void Add(Type serviceType)
        {
            _callSiteChain.Add(serviceType);
        }

        public void Remove(Type serviceType)
        {
            _callSiteChain.Remove(serviceType);
        }
    }
}
