using System;
using System.Linq;
using System.Linq.Expressions;
using Taohua.CallSite;

namespace Taohua.ResolverBuilder
{
    internal class ExpressionResolverBuilder : ServiceResolverBuilder<Expression>
    {
        private readonly SingletonResolverBuilder _singletonResolverBuilder;

        public ExpressionResolverBuilder(SingletonResolverBuilder singletonResolverBuilder)
        {
            _singletonResolverBuilder = singletonResolverBuilder;
        }

        protected override Expression BuildSingleton(ServiceCallSite singletonCallSite)
        {
            return Expression.Constant(_singletonResolverBuilder.Build(singletonCallSite));
        }

        protected override Expression BuildFactory(FactoryCallSite callSite)
        {
            return Expression.Invoke(Expression.Constant(callSite.Factory), Expression.Parameter(typeof(IServiceProvider)));
        }

        protected override Expression BuildConstructor(ConstructorCallSite callSite)
        {
            var ParameterCallSites = callSite.ParameterCallSites;
            Expression[] parameterExpressions = null;

            if (ParameterCallSites.Length == 0)
            {
                parameterExpressions = Array.Empty<Expression>();
            }
            else
            {
                var parameters = callSite.ConstructorInfo.GetParameters();

                parameterExpressions = new Expression[ParameterCallSites.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameterExpressions[i] = Convert(Build(ParameterCallSites[i]), parameters[i].ParameterType);
                }
            }

            return Expression.New(callSite.ConstructorInfo, parameterExpressions);
        }

        private static Expression Convert(Expression expression, Type type)
        {
            if (type.IsAssignableFrom(expression.Type))
            {
                return expression;
            }

            // todo 泛型？？？
            return Expression.Convert(expression, type);
        }

        protected override Expression BuildConstant(ConstantCallSite callSite)
        {
            return Expression.Constant(callSite.DefaultValue);
        }

        protected override Expression BuildIEnumerable(IEnumerableCallSite callSite)
        {
            return Expression.NewArrayInit(callSite.ItemType, callSite.ServiceCallSites.Select(cs => Convert(Build(cs), callSite.ItemType)));
        }
    }
}
