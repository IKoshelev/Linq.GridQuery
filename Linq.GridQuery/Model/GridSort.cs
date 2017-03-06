using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Linq.GridQuery.Model
{
    public class GridSort
    {
        public string PropName { get; set; }
        public bool IsDescending { get; set; }

        public IQueryable<T> WrapSort<T>(
            IQueryable<T> query,
            bool isFirst = false)
        {
            var propAccessExpr = GetPropAccesssLambdaExpr(typeof(T), PropName);
            var orderMethodName = "";
            if (isFirst)
            {
                orderMethodName = IsDescending ? "OrderByDescending" : "OrderBy";
            }
            else
            {
                orderMethodName = IsDescending ? "ThenByDescending" : "ThenBy";
            }

            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == orderMethodName && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), propAccessExpr.ReturnType);
            var newQuery = (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, propAccessExpr });
            return newQuery;
        }

        private static LambdaExpression GetPropAccesssLambdaExpr(Type type, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var param = Expression.Parameter(type);
            var propAccess = Expression.Property(param, prop.Name);
            var expr = Expression.Lambda(propAccess, param);
            return expr;
        }
    }
}
