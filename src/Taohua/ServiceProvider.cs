using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Taohua.CallSite;

namespace Taohua
{
    public interface IServiceCollection
    {
        void AddService(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime, Func<object> valueFactory);
    }

    internal class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceDescriptor[]> _serviceDescriptors;
        private readonly CallSiteFactory _callSiteFactory;
        private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object>> _realizedServices = new ConcurrentDictionary<Type, Func<IServiceProvider, object>>();
        private readonly ExpressionResolverBuilder _resolverBuilder;

        public ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            if (serviceDescriptors is null)
            {
                throw new ArgumentNullException(nameof(serviceDescriptors));
            }

            _serviceDescriptors = serviceDescriptors.GroupBy(des => des.ServiceType).ToDictionary(des => des.Key, des => des.ToArray());

            _callSiteFactory = new CallSiteFactory(serviceDescriptors);

            _resolverBuilder = new ExpressionResolverBuilder(this);
        }

        public object GetService(Type serviceType)
        {
            var realizedService = _realizedServices.GetOrAdd(serviceType, RealizeService);
            return realizedService?.Invoke(this);
        }

        private Func<IServiceProvider, object> RealizeService(Type serviceType)
        {
            var callSite = _callSiteFactory.GetCallSite(serviceType, new CallSiteChain(serviceType));
            var realizeService = _resolverBuilder.Build(callSite);
            _realizedServices[serviceType] = realizeService;

            return realizeService;
        }
    }
}
