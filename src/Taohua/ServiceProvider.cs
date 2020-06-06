using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Taohua.CallSite;
using Taohua.ResolverBuilder;

namespace Taohua
{
    public interface IServiceCollection
    {
        void AddService(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime, Func<object> valueFactory);
    }

    internal class ServiceProvider : IServiceProvider
    {
        private readonly ConcurrentDictionary<Type, object> _realizedSingletonServices = new ConcurrentDictionary<Type, object>();
        private readonly ConcurrentDictionary<Type, Func<IServiceProvider, object>> _realizedServices = new ConcurrentDictionary<Type, Func<IServiceProvider, object>>();
        private readonly CallSiteFactory _callSiteFactory;
        private readonly SingletonResolverBuilder _singletonResolverBuilder;
        private readonly ExpressionResolverBuilder _expressionResolverBuilder;

        public ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            if (serviceDescriptors is null)
            {
                throw new ArgumentNullException(nameof(serviceDescriptors));
            }

            _callSiteFactory = new CallSiteFactory(serviceDescriptors);
            _singletonResolverBuilder = new SingletonResolverBuilder(this, _realizedSingletonServices);
            _expressionResolverBuilder = new ExpressionResolverBuilder(_singletonResolverBuilder);
        }

        public object GetService(Type serviceType)
        {
            var realizedService = _realizedServices.GetOrAdd(serviceType, RealizeService);
            return realizedService?.Invoke(this);
        }

        private Func<IServiceProvider, object> RealizeService(Type serviceType)
        {
            var callSite = _callSiteFactory.GetCallSite(serviceType, new CallSiteChain(serviceType));
            var realizeService = Expression.Lambda<Func<IServiceProvider, object>>(_expressionResolverBuilder.Build(callSite), Expression.Parameter(typeof(IServiceProvider))).Compile();
            _realizedServices[serviceType] = realizeService;

            return realizeService;
        }
    }
}
