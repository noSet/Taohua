using System;

namespace Taohua
{
    public static class ServiceProviderExtensions
    {
        public static void AddSingleton<TService>(this IServiceCollection serviceCollection)
            where TService : class, new()
        {
            serviceCollection.AddService(typeof(TService), typeof(TService), ServiceLifetime.Singleton, null);
        }

        public static void AddSingleton<TService>(this IServiceCollection serviceCollection, TService instance)
            where TService : class
        {
            serviceCollection.AddService(typeof(TService), typeof(TService), ServiceLifetime.Singleton, () => instance);
        }

        public static void AddSingleton<TService>(this IServiceCollection serviceCollection, Func<TService> valueFactory)
            where TService : class
        {
            if (valueFactory is null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            serviceCollection.AddService(typeof(TService), typeof(TService), ServiceLifetime.Singleton, () => valueFactory());
        }

        public static void AddSingleton<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TImplementation : class, TService, new()
        {
            serviceCollection.AddService(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, null);
        }

        public static void AddSingleton<TService, TImplementation>(this IServiceCollection serviceCollection, TImplementation instance)
            where TImplementation : class, TService
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            serviceCollection.AddService(typeof(TService), typeof(TService), ServiceLifetime.Singleton, () => instance);
        }

        public static void AddSingleton<TService, TImplementation>(this IServiceCollection serviceCollection, Func<TService> valueFactory)
            where TImplementation : class, TService
        {
            if (valueFactory is null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            serviceCollection.AddService(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton, () => valueFactory);
        }

        public static void AddTransient<TService>(this IServiceCollection serviceCollection)
            where TService : class, new()
        {
            serviceCollection.AddService(typeof(TService), typeof(TService), ServiceLifetime.Transient, null);
        }

        public static void AddTransient<TService>(this IServiceCollection serviceCollection, Func<TService> valueFactory)
            where TService : class
        {
            if (valueFactory is null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            serviceCollection.AddService(typeof(TService), typeof(TService), ServiceLifetime.Transient, () => valueFactory());
        }

        public static void AddTransient<TService, TImplementation>(this IServiceCollection serviceCollection)
            where TImplementation : class, TService, new()
        {
            serviceCollection.AddService(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, null);
        }

        public static void AddTransient<TService, TImplementation>(this IServiceCollection serviceCollection, Func<TService> valueFactory)
            where TImplementation : class, TService
        {
            if (valueFactory is null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            serviceCollection.AddService(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient, () => valueFactory);
        }

        public static TService GetService<TService>(this IServiceProvider serviceProvider)
        {
            return (TService)serviceProvider.GetService(typeof(TService));
        }
    }
}
