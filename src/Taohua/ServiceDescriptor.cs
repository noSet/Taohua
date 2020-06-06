using System;
using System.Diagnostics;

namespace Taohua
{
    internal class ServiceDescriptor
    {
        private Type _implementationType;

        public Type ServiceType { get; }

        public ServiceLifetime Lifetime { get; }

        /// <summary>
        /// 第三优先级
        /// </summary>
        public Type ImplementationType { get; }

        /// <summary>
        /// 第一优先级 只能是单例模式
        /// </summary>
        public object ImplementationInstance { get; internal set; }

        /// <summary>
        /// 第二优先级
        /// </summary>
        public Func<IServiceProvider, object> ImplementationFactory { get; }

        public static ServiceDescriptor Describe(Type serviceType, object instance)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return new ServiceDescriptor(serviceType, instance.GetType(), ServiceLifetime.Singleton, instance, null);
        }

        public static ServiceDescriptor Describe(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            return new ServiceDescriptor(serviceType, implementationType, lifetime, null, null);
        }

        public static ServiceDescriptor Describe(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            if (factory is null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            return new ServiceDescriptor(serviceType, factory.GetType().GetGenericArguments()[1], lifetime, null, factory);
        }

        private ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime, object implementationInstance, Func<IServiceProvider, object> implementationFactory)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Lifetime = lifetime;
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            ImplementationInstance = implementationInstance;
            ImplementationFactory = implementationFactory;

            CheckDescriptor();
        }

        private void CheckDescriptor()
        {
            if (ImplementationType == typeof(object) && ImplementationInstance is null && ImplementationFactory is null)
            {
                throw new ArgumentException($"无法为服务类型“{ServiceType}”实例化实现类型“{ImplementationType}”！");
            }

            if (ServiceType.IsGenericTypeDefinition)
            {
                if (ImplementationType == typeof(object))
                {
                    throw new ArgumentException($"开放泛型服务类型“{ServiceType}”不能通过ImplementationFactory注入！");
                }

                // todo 如果判断开放泛型的继承关系
                if (!ImplementationType.IsGenericTypeDefinition)
                {
                    throw new ArgumentException($"开放泛型服务类型“{ServiceType}”实现类型“{ImplementationType}”必须也是开放泛型！");
                }

                if (ImplementationType.IsAbstract || ImplementationType.IsInterface)
                {
                    throw new ArgumentException($"无法为服务类型“{ServiceType}”实例化实现类型“{ImplementationType}”！该实现类型可能是抽象类或者接口");
                }
            }
            else if (ImplementationInstance == null && ImplementationFactory == null)
            {
                Debug.Assert(ImplementationType != typeof(object));

                // 通过第三优先级注入的服务需要检查第三优先级是否可以实例化
                if (ImplementationType.IsGenericTypeDefinition || ImplementationType.IsAbstract || ImplementationType.IsInterface)
                {
                    throw new ArgumentException($"无法为服务类型“{ServiceType}”实例化实现类型“{ImplementationType}”！");
                }
            }
        }
    }
}
