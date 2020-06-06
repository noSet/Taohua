using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Taohua.CallSite
{
    internal class CallSiteFactory
    {
        private readonly IEnumerable<ServiceDescriptor> _serviceDescriptors;
        private readonly Dictionary<Type, ServiceDescriptorCacheItem> _descriptorLookup = new Dictionary<Type, ServiceDescriptorCacheItem>();
        private readonly ConcurrentDictionary<Type, ServiceCallSite> _callSiteCache = new ConcurrentDictionary<Type, ServiceCallSite>();

        public CallSiteFactory(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;

            foreach (var descriptor in _serviceDescriptors)
            {
                Type serviceType = descriptor.ServiceType;
                _descriptorLookup.TryGetValue(serviceType, out var cacheItem);
                _descriptorLookup[serviceType] = cacheItem.AddItem(descriptor);
            }
        }

        internal ServiceCallSite GetCallSite(Type serviceType, CallSiteChain callSiteChain)
        {
            return _callSiteCache.GetOrAdd(serviceType, type => CreateCallSite(type, callSiteChain));
        }

        private ServiceCallSite CreateCallSite(Type serviceType, CallSiteChain callSiteChain)
        {
            var callSite = TryCreateExact(serviceType, callSiteChain)
                        ?? TryCreateOpenGeneric(serviceType, callSiteChain)
                        ?? TryCreateEnumerable(serviceType, callSiteChain);

            _callSiteCache[serviceType] = callSite;

            return callSite;
        }

        private ServiceCallSite TryCreateExact(Type serviceType, CallSiteChain callSiteChain)
        {
            if (_descriptorLookup.TryGetValue(serviceType, out var descriptor))
            {
                return TryCreateExact(descriptor.Last, serviceType, callSiteChain);
            }

            return null;
        }

        private ServiceCallSite TryCreateExact(ServiceDescriptor descriptor, Type serviceType, CallSiteChain callSiteChain)
        {
            if (serviceType == descriptor.ServiceType)
            {
                ServiceCallSite callSite = null;
                if (descriptor.ImplementationInstance != null)
                {
                    callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.ImplementationInstance);
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    callSite = new FactoryCallSite(descriptor.ServiceType, descriptor.ImplementationFactory, descriptor.Lifetime);
                }
                else if (descriptor.ImplementationType != null)
                {
                    callSite = CreateConstructorCallSite(descriptor.ServiceType, descriptor.ImplementationType, descriptor.Lifetime, callSiteChain);
                }
                else
                {
                    Debug.Assert(false, "无效的服务描述！");
                }

                return callSite;
            }

            return null;
        }

        private ServiceCallSite CreateConstructorCallSite(Type serviceType, Type implementationType, ServiceLifetime lifetime, CallSiteChain callSiteChain)
        {
            try
            {
                callSiteChain.Add(serviceType);

                ServiceCallSite[] parameterCallSites = null;

                var constructors = implementationType.GetConstructors().Where(ctor => ctor.IsPublic).ToArray();

                switch (constructors.Length)
                {
                    case 0:
                        throw new InvalidOperationException($"{implementationType.Name}没有公开的构造函数！");
                    case 1:
                        {
                            var constructor = constructors[0];
                            var parameters = constructor.GetParameters();

                            if (parameters.Length == 0)
                            {
                                return new ConstructorCallSite(serviceType, constructor, new ServiceCallSite[0], lifetime);
                            }

                            parameterCallSites = CreateArgumentCallSites(serviceType, implementationType, callSiteChain, parameters);
                            return new ConstructorCallSite(serviceType, constructor, parameterCallSites, lifetime);
                        }

                    default:
                        {
                            ConstructorInfo bestConstructor = null;
                            HashSet<Type> bestConstructorParameterTypes = null;

                            foreach (var constructor in constructors.OrderByDescending(ctor => ctor.GetParameters().Length))
                            {
                                var parameters = constructor.GetParameters();

                                // 匹配构造函数，参数最多的构造函数的参数必须是其他构造函数参数的超集
                                if (bestConstructor == null)
                                {
                                    bestConstructor = constructor;
                                }
                                else
                                {
                                    if (bestConstructorParameterTypes == null)
                                    {
                                        bestConstructorParameterTypes = new HashSet<Type>(bestConstructor.GetParameters().Select(p => p.ParameterType));
                                    }

                                    if (!bestConstructorParameterTypes.IsSupersetOf(parameters.Select(p => p.ParameterType)))
                                    {
                                        throw new InvalidOperationException("没有匹配到最佳构造函数！");
                                    }
                                }
                            }

                            parameterCallSites = CreateArgumentCallSites(serviceType, implementationType, callSiteChain, bestConstructor.GetParameters());
                            return new ConstructorCallSite(serviceType, bestConstructor, parameterCallSites, lifetime);
                        }
                }
            }
            finally
            {
                callSiteChain.Remove(serviceType);
            }
        }

        private ServiceCallSite[] CreateArgumentCallSites(Type serviceType, Type implementationType, CallSiteChain callSiteChain, ParameterInfo[] parameters)
        {
            var parameterCallSites = new ServiceCallSite[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameterType = parameter.ParameterType;
                var callSite = GetCallSite(parameterType, callSiteChain);
                parameterCallSites[i] = callSite ?? throw new InvalidOperationException($"服务{serviceType.Name}的实现类型{implementationType.Name}没有注册{parameterType}服务！");
            }

            return parameterCallSites;
        }

        private ServiceCallSite TryCreateOpenGeneric(Type serviceType, CallSiteChain callSiteChain)
        {
            if (serviceType.IsConstructedGenericType && _descriptorLookup.TryGetValue(serviceType.GetGenericTypeDefinition(), out var descriptor))
            {
                return TryCreateOpenGeneric(descriptor.Last, serviceType, callSiteChain);
            }

            return null;
        }

        private ServiceCallSite TryCreateOpenGeneric(ServiceDescriptor descriptor, Type serviceType, CallSiteChain callSiteChain)
        {
            if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() == descriptor.ServiceType)
            {
                Debug.Assert(descriptor.ImplementationType != null, "descriptor.ImplementationType != null");
                var closedType = descriptor.ImplementationType.MakeGenericType(serviceType.GenericTypeArguments);
                return CreateConstructorCallSite(serviceType, closedType, descriptor.Lifetime, callSiteChain);
            }

            return null;
        }

        private ServiceCallSite TryCreateEnumerable(Type serviceType, CallSiteChain callSiteChain)
        {
            try
            {
                callSiteChain.Add(serviceType);

                if (serviceType.IsConstructedGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var itemType = serviceType.GenericTypeArguments[0];

                    var callSites = new List<ServiceCallSite>();

                    if (!itemType.IsGenericType && _descriptorLookup.TryGetValue(itemType, out var descriptors))
                    {
                        for (int i = 0; i < descriptors.Count; i++)
                        {
                            var callSite = TryCreateExact(descriptors[i], itemType, callSiteChain);
                            Debug.Assert(callSite != null);
                            callSites.Add(callSite);
                        }
                    }
                    else
                    {
                        foreach (var descriptor in _serviceDescriptors)
                        {
                            var callSite = TryCreateExact(descriptor, itemType, callSiteChain)
                                ?? TryCreateOpenGeneric(descriptor, itemType, callSiteChain);

                            if (callSite != null)
                            {
                                callSites.Add(callSite);
                            }
                        }
                    }

                    return new IEnumerableCallSite(itemType, callSites.ToArray());
                }

                return null;
            }
            finally
            {
                callSiteChain.Remove(serviceType);
            }
        }

        private struct ServiceDescriptorCacheItem
        {
            private ServiceDescriptor _item;
            private List<ServiceDescriptor> _items;

            public ServiceDescriptor Last => _item;

            public int Count => _items?.Count ?? (_item is null ? 0 : 1);

            public ServiceDescriptor this[int index] => GetDescriptorByIndex(index);

            public ServiceDescriptorCacheItem AddItem(ServiceDescriptor descriptor)
            {
                var newCacheItem = new ServiceDescriptorCacheItem();

                if (_item == null)
                {
                    newCacheItem._item = descriptor;
                }
                else
                {
                    newCacheItem._item = descriptor;
                    newCacheItem._items = _items ?? new List<ServiceDescriptor>();
                    newCacheItem._items.Add(descriptor);
                }

                return newCacheItem;
            }

            private ServiceDescriptor GetDescriptorByIndex(int index)
            {
                if (index > Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (index == 0)
                {
                    return _item;
                }

                return _items[index];
            }
        }
    }
}
