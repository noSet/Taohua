using System;
using System.Threading;

namespace Taohua
{
    internal class ServiceDescriptor
    {
        private object _implementationInstance;
        private Type _implementationType;

        public Type ServiceType { get; }

        public Type ImplementationType => GetImplementationType();

        public ServiceLifetime Lifetime { get; }

        public object ImplementationInstance => GetService();

        public Func<IServiceProvider, object> ImplementationFactory { get; }

        public ServiceDescriptor(Type serviceType, object instance)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _implementationInstance = instance;
        }

        public ServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            _implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            Lifetime = lifetime;
        }

        public ServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        {
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ImplementationFactory = factory ?? throw new ArgumentNullException(nameof(factory));
            Lifetime = lifetime;
        }

        private Type GetImplementationType()
        {
            if (_implementationType != null)
            {
                return _implementationType;
            }

            if (_implementationInstance != null)
            {
                _implementationType = _implementationInstance.GetType();
            }
            else if (ImplementationFactory != null)
            {
                _implementationType = ImplementationFactory.GetType().GenericTypeArguments[1];
            }

            return _implementationType;
        }
    }
}
