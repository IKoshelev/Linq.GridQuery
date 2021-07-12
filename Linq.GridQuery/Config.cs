using Linq.GridQuery.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Linq.GridQuery
{
    public static class Config
    {
        public static Func<string, Type, object> DefaultValueDeserialiationFunction { get; set; }

        private static Dictionary<string, OperatorHandler> _defaultOperatorToExpressionConvertors;
        public static Dictionary<string, OperatorHandler> DefaultOperatorToExpressionConverters
        {
            get
            {
                if (_defaultOperatorToExpressionConvertors == null)
                {
                    _defaultOperatorToExpressionConvertors = new Dictionary<string, OperatorHandler>()
                    {
                        { "eq" , new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>  Expression.Equal(left, right)) },
                        { "neq" ,  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>  Expression.NotEqual(left, right)) },
                        { "gt" ,  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>  Expression.GreaterThan(left, right)) },
                        { "gte" ,  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>  Expression.GreaterThanOrEqual(left, right)) },
                        { "lt" ,  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>  Expression.LessThan(left, right)) },
                        { "lte" ,  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>  Expression.LessThanOrEqual(left, right)) },
                        { "in",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                if (typeof(IEnumerable).IsAssignableFrom(right.Type) == false)
                                {
                                    throw new Exception("In order to use 'in' operator you must supply an value that is an enumerable, "+
                                                        $"but value type {right.Type.Name} was received.");
                                }

                                var ContainsGeneric = typeof(Enumerable)
                                                        .GetMember("Contains", MemberTypes.Method,  BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                                                        .Cast<MethodInfo>()
                                                        .Single(x => x.GetParameters().Count() == 2 );

                                var ContainsConcrete = ContainsGeneric.MakeGenericMethod(new Type[] { propTypeUnwrapped });

                                return Expression.Call(ContainsConcrete, right, left);
                            },
                            (propTypeUnwrapped, propTypeRaw) => propTypeUnwrapped.MakeArrayType())
                            },
                            { "notin",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                if (typeof(IEnumerable).IsAssignableFrom(right.Type) == false)
                                {
                                    throw new Exception("In order to use 'notin' operator you must supply an value that is an enumerable, "+
                                                        $"but value type {right.Type.Name} was received.");
                                }

                                var ContainsGeneric = typeof(Enumerable)
                                                        .GetMember("Contains", MemberTypes.Method,  BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase)
                                                        .Cast<MethodInfo>()
                                                        .Single(x => x.GetParameters().Count() == 2 );

                                var ContainsConcrete = ContainsGeneric.MakeGenericMethod(new Type[] { propTypeUnwrapped });

                                return Expression.Not(Expression.Call(ContainsConcrete, right, left));
                            },
                            (propTypeUnwrapped, propTypeRaw) => propTypeUnwrapped.MakeArrayType())
                        },
                        { "contains",   new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                    var MethodInfo = propTypeUnwrapped.GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                                     , null,  new Type[] { propTypeUnwrapped }, null);
                                    return Expression.Call(left, MethodInfo, right);
                            })
                        },
                        { "doesnotcontain",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                 var MethodInfo = propTypeUnwrapped.GetMethod("Contains", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                                     , null,  new Type[] { propTypeUnwrapped }, null);
                                return Expression.Not(Expression.Call(left, MethodInfo, right));
                            })
                        },
                        { "startswith",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                 var MethodInfo = propTypeUnwrapped.GetMethod("StartsWith", new Type[] { propTypeUnwrapped });
                                 return Expression.Call(left, MethodInfo, right);
                            })
                        },
                       { "endswith",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                 var MethodInfo = propTypeUnwrapped.GetMethod("EndsWith", new Type[] { propTypeUnwrapped });
                                 return Expression.Call(left, MethodInfo, right);
                            })
                        },
                        {"isnull",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                if(propTypeRaw.IsNullableStruct())
                                {
                                    var hasValuePropForNullable = propTypeRaw.GetProperty(
                                             nameof(Nullable<int>.HasValue),
                                             BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                                    return Expression.Not(Expression.MakeMemberAccess(left, hasValuePropForNullable));
                                }

                                if (propTypeRaw.IsClass)
                                {
                                    return Expression.Equal(left, Expression.Constant(null));

                                }

                                 throw new ArgumentException($"A null check was passed for type {propTypeRaw.Name}, which is nullable.");
                            },
                            (propTypeUnwrapped, propTypeRaw) => propTypeRaw,
                            skipNullCheck: true)
                        },
                          {"isnotnull",  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                            {
                                if(propTypeRaw.IsNullableStruct())
                                {
                                    var hasValuePropForNullable = propTypeRaw.GetProperty(
                                             nameof(Nullable<int>.HasValue),
                                             BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                                    return Expression.MakeMemberAccess(left, hasValuePropForNullable);
                                }

                                if (propTypeRaw.IsClass)
                                {
                                    return Expression.NotEqual(left, Expression.Constant(null));

                                }

                                 throw new ArgumentException($"A null check was passed for type {propTypeRaw.Name}, which is nullable.");
                            },
                            (propTypeUnwrapped, propTypeRaw) => propTypeRaw,
                            skipNullCheck: true)
                        }
                    };
                }
                return _defaultOperatorToExpressionConvertors;
            }
        }
    }
}
