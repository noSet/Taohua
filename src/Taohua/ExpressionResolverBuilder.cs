using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Taohua.CallSite;

namespace Taohua
{
    internal class ExpressionResolverBuilder
    {
        private readonly ServiceProvider _serviceProvider;

        public ExpressionResolverBuilder(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Func<IServiceProvider, object> Build(ServiceCallSite callSite)
        {
            return Expression.Lambda<Func<IServiceProvider, object>>(BuildCallSite(callSite), Expression.Parameter(typeof(IServiceProvider))).Compile();
        }

        private Expression BuildCallSite(ServiceCallSite callSite)
        {
            if (callSite == null)
            {
                return null;
            }

            switch (callSite.Kind)
            {
                case CallSiteKind.Factory:
                    return BuildFactory((FactoryCallSite)callSite);
                case CallSiteKind.Constructor:
                    return BuildConstructor((ConstructorCallSite)callSite);
                case CallSiteKind.Constant:
                    return BuildConstant((ConstantCallSite)callSite);
                case CallSiteKind.IEnumerable:
                    return BuildIEnumerable((IEnumerableCallSite)callSite);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Expression BuildFactory(FactoryCallSite callSite)
        {
            return Expression.Invoke(Expression.Constant(callSite.Factory), Expression.Parameter(typeof(IServiceProvider)));
        }

        private Expression BuildConstructor(ConstructorCallSite callSite)
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
                    parameterExpressions[i] = Convert(BuildCallSite(ParameterCallSites[i]), parameters[i].ParameterType);
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

        private Expression BuildConstant(ConstantCallSite callSite)
        {
            return Expression.Constant(callSite.DefaultValue);
        }

        private Expression BuildIEnumerable(IEnumerableCallSite callSite)
        {
            return Expression.NewArrayInit(callSite.ItemType, callSite.ServiceCallSites.Select(cs => Convert(BuildCallSite(cs), callSite.ItemType)));
        }
    }
}
