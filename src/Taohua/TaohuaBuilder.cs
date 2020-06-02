using System;
using System.Collections.Generic;

namespace Taohua
{
    public class TaohuaBuilder
    {
        private readonly List<ServiceDescriptor> _serviceDescriptors = new List<ServiceDescriptor>();

        internal void AddService(ServiceDescriptor service)
        {
            _serviceDescriptors.Add(service);
        }

        public IServiceProvider Build()
        {
            return default;
        }
    }
}
