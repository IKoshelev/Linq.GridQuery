using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Linq.GridQuery.Model
{
    [DataContract]
    public class FilterTreeNode
    {
        public Func<string, Type, object> ValueDeserializationFunctionOverride { get; set; }
        public Dictionary<string, OperatorHandler> OperatorToExpressionConvertersOverrides;

        [DataMember]
        public readonly GridFilter Filter;
        [DataMember]
        public readonly FilterTreeNode LeftTreeNode;
        [DataMember]
        public readonly FilterTreeNode RightTreeNode;
        [DataMember]
        public readonly LogicalOpertor LogicalOperator;

        public FilterTreeNode(
            GridFilter filter,
            Func<string, Type, object> valueDeserializationFunctionOverride = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides = null)
        {
            Filter = filter;
            ValueDeserializationFunctionOverride = valueDeserializationFunctionOverride;
            OperatorToExpressionConvertersOverrides = operatorToExpressionConvertersOverrides;
        }

        public FilterTreeNode(
            FilterTreeNode leftTreeNode,
            LogicalOpertor logicalOperator,
            FilterTreeNode rightTreeNode,
            Func<string, Type, object> valueDeserializationFunctionOverride = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides = null)
        {
            LeftTreeNode = leftTreeNode;
            RightTreeNode = rightTreeNode;
            LogicalOperator = logicalOperator;
            ValueDeserializationFunctionOverride = valueDeserializationFunctionOverride;
            OperatorToExpressionConvertersOverrides = operatorToExpressionConvertersOverrides;
        }

        public FilterTreeNode(
            GridFilter leftGridFilter,
            LogicalOpertor logicalOperator,
            FilterTreeNode rightTreeNode,
            Func<string, Type, object> valueDeserializationFunctionOverride = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides = null):
                    this(   new FilterTreeNode(leftGridFilter),
                            logicalOperator,
                            rightTreeNode,
                            valueDeserializationFunctionOverride,
                            operatorToExpressionConvertersOverrides)
        {

        }

        public FilterTreeNode(
            FilterTreeNode leftGridFilter,
            LogicalOpertor logicalOperator,
            GridFilter rightTreeNode,
            Func<string, Type, object> valueDeserializationFunctionOverride = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides = null) :
                    this(   leftGridFilter,
                            logicalOperator,
                            new FilterTreeNode(rightTreeNode),
                            valueDeserializationFunctionOverride,
                            operatorToExpressionConvertersOverrides)
        {

        }

        public FilterTreeNode(
            GridFilter leftGridFilter,
            LogicalOpertor logicalOperator,
            GridFilter rightTreeNode,
            Func<string, Type, object> valueDeserializationFunctionOverride = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides = null) :
                    this(   new FilterTreeNode(leftGridFilter),
                            logicalOperator,
                            new FilterTreeNode(rightTreeNode),
                            valueDeserializationFunctionOverride,
                            operatorToExpressionConvertersOverrides)
        {

        }

        public IQueryable<T> WrapFilter<T>(IQueryable<T> query)
        {
            var type = typeof(T);
            ParameterExpression parameter = Expression.Parameter(type, "par");

            Expression filterExpr = GetExprssion<T>(parameter, ValueDeserializationFunctionOverride, OperatorToExpressionConvertersOverrides);

            var lambdaFilter = Expression.Lambda<Func<T, bool>>(filterExpr, new[] { parameter });

            var newQuery = query.Where(lambdaFilter);

            return newQuery;
        }

        internal Expression GetExprssion<T>(
            ParameterExpression parameter, 
            Func<string, Type, object> valueDeserializationFunctionFromUpTree = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertorsOverridesFromUpTree = null)
        {
            var deserializationFunction = ValueDeserializationFunctionOverride ?? valueDeserializationFunctionFromUpTree;
            var operatorToExpressionConvertersOverrides = OperatorToExpressionConvertersOverrides ?? operatorToExpressionConvertorsOverridesFromUpTree;

            if (Filter != null)
            {
                return GetExpresionFromCondition<T>(parameter, deserializationFunction, operatorToExpressionConvertersOverrides);
            }

            if(LeftTreeNode == null || RightTreeNode == null)
            {
                throw new InvalidOperationException("FilterTreeNode requires either Filter or LeftTreeNode and RightTreeNode.");
            }

            if (LogicalOperator == LogicalOpertor.AND)
            {
                return Expression.And(
                    LeftTreeNode.GetExprssion<T>(parameter, deserializationFunction, operatorToExpressionConvertersOverrides),
                    RightTreeNode.GetExprssion<T>(parameter, deserializationFunction, operatorToExpressionConvertersOverrides));
            }

            return Expression.Or(
                   LeftTreeNode.GetExprssion<T>(parameter, deserializationFunction, operatorToExpressionConvertersOverrides),
                   RightTreeNode.GetExprssion<T>(parameter, deserializationFunction, operatorToExpressionConvertersOverrides));
        }

        internal Expression GetExpresionFromCondition<T>(
            ParameterExpression parameter, 
            Func<string, Type, object> valueDeserializationFunction = null,
            Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides = null)
        {
            var prop = typeof(T).GetProperty(
                Filter.PropName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop == null)
            {
                throw new ArgumentException($"Property {Filter.PropName} not found on type {typeof(T).Name}");
            }

            var isNullableStruct = prop.PropertyType.IsNullableStruct();

            var propAccessExpr = Expression.MakeMemberAccess(parameter, prop);
            var propAccessExprWitNullUnwrapIfNeeded = GetPropAccessExprWithNullUnwrapIfNeeded(prop, isNullableStruct, propAccessExpr);
                
            OperatorHandler operatorHandler = ResolveOperatorHanlder(
                                                            Filter.Operator, 
                                                            operatorToExpressionConvertersOverrides);

            var propTypeUnwrapped = isNullableStruct
                                        ? prop.PropertyType.GetGenericArguments()[0]
                                        : prop.PropertyType;

            Func<string, Type, object> deserializationFunc = ResolveDeserializationFunc(valueDeserializationFunction);

            var operatorAdjustedPropType = operatorHandler.PropTypeConverter?.Invoke(propTypeUnwrapped, prop.PropertyType) 
                                                            ?? prop.PropertyType;

            object value = deserializationFunc(Filter.StringValue, operatorAdjustedPropType);

            if (operatorHandler.SkipNullCheck)
            {
                return operatorHandler.ExpressionFactory(
                                    propTypeUnwrapped,
                                    prop.PropertyType,
                                    propAccessExpr,
                                    Expression.Constant(value));
            }

            var comparisonWhenNotNull = operatorHandler.ExpressionFactory(
                                                propTypeUnwrapped,
                                                prop.PropertyType,
                                                propAccessExprWitNullUnwrapIfNeeded,
                                                Expression.Constant(value));

            return MakeComparisonWithNullChecksIfNeeded(prop, propAccessExpr, comparisonWhenNotNull);
        }

        private static Expression MakeComparisonWithNullChecksIfNeeded(PropertyInfo prop, MemberExpression propAccessExpr, Expression comparisonWhenNotNull)
        {
            if (prop.PropertyType.IsNullableStruct())
            {
                var hasValuePropForNullable = prop.PropertyType.GetProperty(
                         nameof(Nullable<int>.HasValue),
                         BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                var hasValueCheck = Expression.MakeMemberAccess(propAccessExpr, hasValuePropForNullable);
                return Expression.Condition(hasValueCheck, comparisonWhenNotNull, Expression.Constant(false));
            }

            if (prop.PropertyType.IsClass)
            {
                var nullCheck = Expression.NotEqual(propAccessExpr, Expression.Constant(null));
                return Expression.Condition(nullCheck, comparisonWhenNotNull, Expression.Constant(false));
            }

            return comparisonWhenNotNull;
        }

        private Func<string, Type, object> ResolveDeserializationFunc(Func<string, Type, object> valueDeserializationFunction)
        {
            Func<string, Type, object> deserializationFunc = valueDeserializationFunction ?? Config.DefaultValueDeserialiationFunction;

            if (deserializationFunc == null)
            {
                ThrowExceptionForDeserializationFunctionNotFound();
            }

            return deserializationFunc;
        }

        private static MemberExpression GetPropAccessExprWithNullUnwrapIfNeeded(PropertyInfo prop, bool isNullableStruct, MemberExpression propAccessExpr)
        {
            if (isNullableStruct == false)
            {
                return propAccessExpr;
            }

            var valuePropForNullable = prop.PropertyType.GetProperty(
                nameof(Nullable<int>.Value),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            return Expression.MakeMemberAccess(propAccessExpr, valuePropForNullable);    
        }

        private void ThrowExceptionForDeserializationFunctionNotFound()
        {
            throw new InvalidOperationException(
$@"Can't deserialize value {Filter.StringValue}, 
because both {nameof(Config)}.{nameof(Config.DefaultValueDeserialiationFunction)} 
and valueDeserializationFunction argument passed from up tree are null. 
Please set general function in config or provide one for 
partiucular instances (can be passed with constructor or set via {ValueDeserializationFunctionOverride} prop).");
        }

        private static OperatorHandler ResolveOperatorHanlder(string @operator, Dictionary<string, OperatorHandler> operatorToExpressionConvertersOverrides)
        {
            var hanlder = operatorToExpressionConvertersOverrides?.GetOrNull(@operator)
                            ?? Config.DefaultOperatorToExpressionConverters.GetOrNull(@operator);


            if (hanlder == null)
            {
                throw new ArgumentException(
$@"Could not find operator {@operator} in the {nameof(operatorToExpressionConvertersOverrides)} 
provided to the metod {nameof(ResolveOperatorHanlder)}
or global {nameof(Config)}.{nameof(Config.DefaultOperatorToExpressionConverters)} dictionaries.");
            }

            return hanlder;
        }
    }
}
