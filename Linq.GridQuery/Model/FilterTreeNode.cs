using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Linq.GridQuery.Model
{
    public enum LogicalOpertor
    {
        AND, OR
    }

    public class FilterTreeNode
    {
        public Func<string, Type, object> ValueDeserializationFunctionOverride { get; set; }

        public readonly GridFilter Filter;
        public readonly FilterTreeNode LeftTreeNode;
        public readonly FilterTreeNode RightTreeNode;
        public readonly LogicalOpertor LogicalOperator;

        public FilterTreeNode(GridFilter filter,
            Func<string, Type, object> valueDeserializationFunctionOverride = null)
        {
            Filter = filter;
            ValueDeserializationFunctionOverride = valueDeserializationFunctionOverride;
        }

        public FilterTreeNode(
            FilterTreeNode leftTreeNode,
            LogicalOpertor logicalOperator,
            FilterTreeNode rightTreeNode,
            Func<string, Type, object> valueDeserializationFunctionOverride = null)
        {
            LeftTreeNode = leftTreeNode;
            RightTreeNode = rightTreeNode;
            LogicalOperator = logicalOperator;
            ValueDeserializationFunctionOverride = valueDeserializationFunctionOverride;
        }

        public IQueryable<T> WrapFilter<T>(IQueryable<T> query)
        {
            var type = typeof(T);
            ParameterExpression parameter = Expression.Parameter(type, "par");

            Expression filterExpr = GetExprssion<T>(parameter, ValueDeserializationFunctionOverride);

            var lambdaFilter = Expression.Lambda<Func<T, bool>>(filterExpr, new[] { parameter });

            var newQuery = query.Where(lambdaFilter);

            return newQuery;
        }

        internal Expression GetExprssion<T>(ParameterExpression parameter, Func<string, Type, object> valueDeserializationFunctionFromUpTree = null)
        {
            var deserializationFunction = ValueDeserializationFunctionOverride ?? valueDeserializationFunctionFromUpTree;

            if (Filter != null)
            {
                return GetExpresionFromCondition<T>(parameter, deserializationFunction);
            }

            if(LeftTreeNode == null || RightTreeNode == null)
            {
                throw new InvalidOperationException("FilterTreeNode requires either Filter or LeftTreeNode and RightTreeNode.");
            }

            if (LogicalOperator == LogicalOpertor.AND)
            {
                return Expression.And(
                    LeftTreeNode.GetExprssion<T>(parameter, deserializationFunction),
                    RightTreeNode.GetExprssion<T>(parameter, deserializationFunction));
            }

            return Expression.Or(
                   LeftTreeNode.GetExprssion<T>(parameter, deserializationFunction),
                   RightTreeNode.GetExprssion<T>(parameter, deserializationFunction));
        }

        internal Expression GetExpresionFromCondition<T>(ParameterExpression parameter, Func<string, Type, object> valueDeserializationFunction = null)
        {
            var type = typeof(T);
            var prop = type.GetProperty(
                Filter.PropName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            var propAccessExpr = Expression.MakeMemberAccess(parameter, prop);
            Func<string, Type, object> deserializationFunc = valueDeserializationFunction ?? Config.DefaultValueDeserialiationFunction;

            if (deserializationFunc == null)
            {
                throw new InvalidOperationException(
$@"Can't deserialize value {Filter.StringValue}, 
because both {nameof(Config)}.{nameof(Config.DefaultValueDeserialiationFunction)} 
and {nameof(valueDeserializationFunction)} argument passed from up tree are null. 
Please set general function in config or provide one for 
partiucular instances (can be passed with constructor or set via {ValueDeserializationFunctionOverride} prop).");
            }

            object value = deserializationFunc(Filter.StringValue, prop.PropertyType);

            var comparison = GetComparator(
                Filter.Operand,
                prop.PropertyType,
                propAccessExpr,
                Expression.Constant(value));

            if (prop.PropertyType.IsNullable())
            {
                var nullCheck = Expression.NotEqual(propAccessExpr, Expression.Constant(null));
                comparison = Expression.And(nullCheck, comparison);
            }

            return comparison;
        }

        private static Expression GetComparator(string operand, Type propType, Expression left, Expression right)
        {
            switch (operand)
            {
                case "Gt":
                    return Expression.GreaterThan(left, right);
                case "Eq":
                    return Expression.Equal(left, right);
                case "Lt":
                    return Expression.LessThan(left, right);
                default:
                    // try to find a method by the name of operand
                    var MethodInfo = propType.GetMethod(operand, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    return Expression.Call(left, MethodInfo, right);
            }
        }
    }
}
