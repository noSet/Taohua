using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

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
