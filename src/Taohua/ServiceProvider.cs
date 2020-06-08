using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly ConcurrentDictionary<ServiceCallSite, object> _realizedSingletonServices = new ConcurrentDictionary<ServiceCallSite, object>();
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
            _expressionResolverBuilder = new ExpressionResolverBuilder(new SingletonResolverBuilder(this, _realizedSingletonServices));
        }

        public object GetService(Type serviceType)
        {
            var realizedService = _realizedServices.GetOrAdd(serviceType, RealizeService);
            return realizedService?.Invoke(this);
        }


        private Func<IServiceProvider, object> RealizeService(Type serviceType)
        {
            var callSite = _callSiteFactory.GetCallSite(serviceType, new CallSiteChain(serviceType));
            var expression = Expression.Lambda<Func<IServiceProvider, object>>(_expressionResolverBuilder.Build(callSite), ExpressionResolverBuilder.Parameter);
            var realizeService = expression.Compile();
            _realizedServices[serviceType] = realizeService;

            return realizeService;
        }
    }
}
