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
        public readonly GridFilter Filter;
        public readonly FilterTreeNode LeftTreeNode;
        public readonly FilterTreeNode RightTreeNode;
        public readonly LogicalOpertor LogicalOperator;

        public FilterTreeNode(GridFilter filter)
        {
            Filter = filter;
        }

        public FilterTreeNode(
            FilterTreeNode leftTreeNode,
            LogicalOpertor logicalOperator,
            FilterTreeNode rightTreeNode)
        {
            LeftTreeNode = leftTreeNode;
            RightTreeNode = rightTreeNode;
            LogicalOperator = logicalOperator;
        }

        public IQueryable<T> WrapFilter<T>(IQueryable<T> query)
        {
            var type = typeof(T);
            ParameterExpression parameter = Expression.Parameter(type, "par");

            var filterExpr = GetExprssion<T>(parameter);

            var lambdaFilter = Expression.Lambda<Func<T, bool>>(filterExpr, new[] { parameter });

            var newQuery = query.Where(lambdaFilter);

            return newQuery;
        }

        internal Expression GetExprssion<T>(ParameterExpression parameter)
        {
            if (Filter != null)
            {
                return GetExpresionFromCondition<T>(parameter);
            }

            if(LeftTreeNode == null || RightTreeNode == null)
            {
                throw new InvalidOperationException("FilterTreeNode requires either Filter or LeftTreeNode and RightTreeNode.");
            }

            if (LogicalOperator == LogicalOpertor.AND)
            {
                return Expression.And(
                    LeftTreeNode.GetExprssion<T>(parameter),
                    RightTreeNode.GetExprssion<T>(parameter));
            }

            return Expression.Or(
                   LeftTreeNode.GetExprssion<T>(parameter),
                   RightTreeNode.GetExprssion<T>(parameter));
        }

        internal Expression GetExpresionFromCondition<T>(ParameterExpression parameter)
        {
            var type = typeof(T);
            var prop = type.GetProperty(
                Filter.PropName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            
            var propAccessExpr = Expression.MakeMemberAccess(parameter, prop);
            object value = Config.ValueDeserialiationFunction(Filter.StringValue, prop.PropertyType);
            Expression<Func<T, bool>> filterExpr;

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
