using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
