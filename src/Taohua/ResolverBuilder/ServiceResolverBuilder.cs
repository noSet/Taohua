using System;
using Taohua.CallSite;

namespace Taohua.ResolverBuilder
{
    internal abstract class ServiceResolverBuilder<TResult>
    {
        public virtual TResult Build(ServiceCallSite serviceCallSite)
        {
            switch (serviceCallSite.Lifetime)
            {
                case ServiceLifetime.Transient:
                    return BuildTransient(serviceCallSite);
                case ServiceLifetime.Singleton:
                    return BuildSingleton(serviceCallSite);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual TResult BuildTransient(ServiceCallSite transientCallSite)
        {
            return BuildService(transientCallSite);
        }

        protected virtual TResult BuildSingleton(ServiceCallSite singletonCallSite)
        {
            return BuildService(singletonCallSite);
        }

        protected virtual TResult BuildService(ServiceCallSite serviceCallSite)
        {
            switch (serviceCallSite.Kind)
            {
                case CallSiteKind.Factory:
                    return BuildFactory((FactoryCallSite)serviceCallSite);
                case CallSiteKind.Constructor:
                    return BuildConstructor((ConstructorCallSite)serviceCallSite);
                case CallSiteKind.Constant:
                    return BuildConstant((ConstantCallSite)serviceCallSite);
                case CallSiteKind.IEnumerable:
                    return BuildIEnumerable((IEnumerableCallSite)serviceCallSite);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected abstract TResult BuildFactory(FactoryCallSite factoryCallSite);

        protected abstract TResult BuildConstructor(ConstructorCallSite constructorCallSite);

        protected abstract TResult BuildConstant(ConstantCallSite constantCallSite);

        protected abstract TResult BuildIEnumerable(IEnumerableCallSite enumerableCallSite);
    }
}
