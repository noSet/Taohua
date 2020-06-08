using System;
using System.Collections.Concurrent;
using Taohua.CallSite;

namespace Taohua.ResolverBuilder
{
    internal class SingletonResolverBuilder : ServiceResolverBuilder<object>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<ServiceCallSite, object> _cache;

        public SingletonResolverBuilder(IServiceProvider serviceProvider, ConcurrentDictionary<ServiceCallSite, object> cache)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
        }

        public override object Build(ServiceCallSite serviceCallSite)
        {
            if (serviceCallSite.Lifetime == ServiceLifetime.Singleton)
            {
                return _cache.GetOrAdd(serviceCallSite, serviceType => base.Build(serviceCallSite));
            }
            else
            {
                return base.Build(serviceCallSite);
            }
        }

        protected override object BuildConstant(ConstantCallSite constantCallSite)
        {
            return constantCallSite.DefaultValue;
        }

        protected override object BuildConstructor(ConstructorCallSite constructorCallSite)
        {
            object[] parameterValues;

            if (constructorCallSite.ParameterCallSites.Length == 0)
            {
                parameterValues = Array.Empty<object>();
            }
            else
            {
                parameterValues = new object[constructorCallSite.ParameterCallSites.Length];

                for (var index = 0; index < parameterValues.Length; index++)
                {
                    parameterValues[index] = Build(constructorCallSite.ParameterCallSites[index]);
                }
            }

            return constructorCallSite.ConstructorInfo.Invoke(parameterValues);
        }

        protected override object BuildFactory(FactoryCallSite factoryCallSite)
        {
            return factoryCallSite.Factory(_serviceProvider);
        }

        protected override object BuildIEnumerable(IEnumerableCallSite enumerableCallSite)
        {
            var array = Array.CreateInstance(enumerableCallSite.ItemType, enumerableCallSite.ServiceCallSites.Length);

            for (var index = 0; index < enumerableCallSite.ServiceCallSites.Length; index++)
            {
                var value = Build(enumerableCallSite.ServiceCallSites[index]);
                array.SetValue(value, index);
            }

            return array;
        }
    }
}
