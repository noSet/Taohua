using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Taohua.Tests.Common;

namespace Taohua.Tests
{
    [TestClass()]
    public class ServiceProviderTests
    {
        [TestMethod()]
        public void GetServiceTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Boo), typeof(Boo), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            service.GetService(typeof(Boo));
        }

        [TestMethod()]
        public void LifetimeTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Boo), typeof(Boo), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            Boo transientBoo1 = service.GetService(typeof(Boo)) as Boo;
            Boo transientBoo2 = service.GetService(typeof(Boo)) as Boo;

            Foo singletonFoo1 = service.GetService(typeof(Foo)) as Foo;
            Foo singletonFoo2 = service.GetService(typeof(Foo)) as Foo;

            Assert.AreNotSame(transientBoo1, transientBoo2);

            Assert.AreSame(singletonFoo1, singletonFoo2);
            Assert.AreSame(singletonFoo1, transientBoo1.Foo);
            Assert.AreSame(transientBoo1.Foo, transientBoo2.Foo);
        }

        [TestMethod()]
        public void OpenGenericTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(ILogger<>), typeof(Logger<>), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            ILogger<Boo> logger = service.GetService(typeof(ILogger<Boo>)) as ILogger<Boo>;

            Assert.AreEqual(logger.Log("Test"), "BooTest");
        }

        [TestMethod()]
        public void OpenGenericAndConstructorTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(MyConverter<,>), typeof(MyConverter<,>), ServiceLifetime.Transient));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Boo), typeof(Boo), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            MyConverter<Foo, Boo> myConverter = service.GetService(typeof(MyConverter<Foo, Boo>)) as MyConverter<Foo, Boo>;

            Assert.IsNotNull(myConverter);

            Assert.AreSame(myConverter.From, service.GetService(typeof(Foo)));
            Assert.AreNotSame(myConverter.From, service.GetService(typeof(Boo)));
        }

        [TestMethod()]
        public void OpenGenericAndConstructorTest2()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(MyConverter<,>), typeof(MyConverter<,>), ServiceLifetime.Transient));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(MyConverter<Foo, Boo>), typeof(MyConverter<Foo, Boo>), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Boo), typeof(Boo), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            MyConverter<Foo, Boo> myConverter1 = service.GetService(typeof(MyConverter<Foo, Boo>)) as MyConverter<Foo, Boo>;
            MyConverter<Foo, Boo> myConverter2 = service.GetService(typeof(MyConverter<Foo, Boo>)) as MyConverter<Foo, Boo>;

            Assert.AreSame(myConverter1, myConverter2);

            IEnumerable<MyConverter<Foo, Boo>> myConverters = service.GetService(typeof(IEnumerable<MyConverter<Foo, Boo>>)) as IEnumerable<MyConverter<Foo, Boo>>;

            Assert.AreEqual(myConverters.Count(), 2);

            Assert.AreSame(myConverter1, myConverters.Last());
        }

        [TestMethod()]
        public void EnumerableTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Transient));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), new Foo()));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), new Foo()));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            IEnumerable<Foo> foos = service.GetService(typeof(IEnumerable<Foo>)) as IEnumerable<Foo>;

            Assert.AreEqual(foos.Count(), 4);
        }

        [TestMethod()]
        public void EnumerableTestAndOpenGeneric()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(ILogger<>), typeof(Logger<>), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            IEnumerable<ILogger<Boo>> loggers = service.GetService(typeof(IEnumerable<ILogger<Boo>>)) as IEnumerable<ILogger<Boo>>;

            Assert.AreEqual(loggers.Count(), 1);
            Assert.AreEqual(loggers.First().Log("Test"), "BooTest");
        }

        [TestMethod()]
        public void ManyConstructorTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(ManyCtor), typeof(ManyCtor), ServiceLifetime.Transient));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(ManyCtor2), typeof(ManyCtor2), ServiceLifetime.Transient));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Boo), typeof(Boo), ServiceLifetime.Transient));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);

            Assert.IsNotNull(service.GetService(typeof(ManyCtor)));
            Assert.ThrowsException<InvalidOperationException>(() => service.GetService(typeof(ManyCtor2)));
        }

        [TestMethod()]
        public void NotRegisterMustTypeTest()
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(ManyCtor), typeof(ManyCtor), ServiceLifetime.Transient));
            serviceDescriptors.Add(ServiceDescriptor.Describe(typeof(Foo), typeof(Foo), ServiceLifetime.Singleton));

            ServiceProvider service = new ServiceProvider(serviceDescriptors);
            Assert.ThrowsException<InvalidOperationException>(() => service.GetService(typeof(ManyCtor)));
        }
    }
}
