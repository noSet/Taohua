using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Taohua
{
    public interface IServiceCollection
    {
        void AddService(Type serviceType, Type implementationType, ServiceLifetime serviceLifetime, Func<object> valueFactory);
    }

    internal class Taohua : IServiceProvider
    {
        private readonly Dictionary<Type, ServiceDescriptor[]> _serviceDescriptors;

        public Taohua(IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            if (serviceDescriptors is null)
            {
                throw new ArgumentNullException(nameof(serviceDescriptors));
            }

            _serviceDescriptors = serviceDescriptors.GroupBy(des => des.ServiceType).ToDictionary(des => des.Key, des => des.ToArray());
        }

        public object GetService(Type serviceType)
        {
            // todo 在单例下初始化实例的时候注意线程安全

            if (serviceType.IsGenericTypeDefinition)
            {
                throw new ArgumentException("不能提供开放泛型服务实例！");
            }

            ServiceDescriptor[] serviceDescriptors = null;

            // 已注册的服务直接提供
            if (_serviceDescriptors.TryGetValue(serviceType, out serviceDescriptors))
            {
                return GetService(serviceDescriptors[^1]);
            }

            // 如果是IEnumerable<>类型，则提供多个服务
            if (serviceType.IsConstructedGenericType && typeof(IEnumerable<>).IsAssignableFrom(serviceType.GetGenericTypeDefinition()))
            {
                if (_serviceDescriptors.TryGetValue(serviceType.GenericTypeArguments[0], out serviceDescriptors))
                {
                    return serviceDescriptors.Select(sd => GetService(sd)).ToArray();
                }

                return null;
            }

            if (serviceType.IsConstructedGenericType && _serviceDescriptors.TryGetValue(serviceType.GetGenericTypeDefinition(), out serviceDescriptors))
            {
                ServiceDescriptor serviceDescriptor = serviceDescriptors[^1];

                ServiceDescriptor newServiceDescriptor = new ServiceDescriptor(serviceType, serviceDescriptor.ImplementationType.MakeGenericType(serviceType.GetGenericArguments()), serviceDescriptor.Lifetime);

                if (newServiceDescriptor.Lifetime == ServiceLifetime.Singleton)
                {
                    _serviceDescriptors.Add(newServiceDescriptor.ServiceType, new[] { newServiceDescriptor });
                }

                return GetService(newServiceDescriptor);
            }

            return null;
        }

        private object GetService(ServiceDescriptor serviceDescriptor)
        {

        }
    }
}
