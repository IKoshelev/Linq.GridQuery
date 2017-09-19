using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Linq.GridQuery.Model
{
    [DataContract]
    public class GridSort
    {
        public GridSort()
        {

        }

        public GridSort(string propName, bool isDescending = false, bool treatNullLowest = false)
        {
            PropName = propName;
            IsDescending = isDescending;
            TreatNullLowest = treatNullLowest;
        }

        [DataMember]
        public string PropName { get; set; }
        [DataMember]
        public bool IsDescending { get; set; }
        [DataMember]
        public bool TreatNullLowest { get; set; }

        public IQueryable<T> WrapSort<T>(
            IQueryable<T> query,
            bool isFirst = false)
        {
            var propAccessExpr = GetPropAccesssLambdaExpr(typeof(T), PropName);
            IQueryable<T> newQuery = query;

            var orderMethodName = "";
            orderMethodName = GetSortingMethodName(isFirst);

            if (TreatNullLowest)
            {
                var nullCheckExpression = GetNullCheckLambdaExpression(propAccessExpr);
                newQuery = WrapQueryByMethodName(newQuery, nullCheckExpression, orderMethodName);
                orderMethodName = GetSortingMethodName(false);           
            }

            newQuery = WrapQueryByMethodName(newQuery, propAccessExpr, orderMethodName);
            return newQuery;
        }

        private LambdaExpression GetNullCheckLambdaExpression(LambdaExpression propAccessLambdaExpr)
        {
            var param = propAccessLambdaExpr.Parameters[0];
            var memberExpr = (MemberExpression)propAccessLambdaExpr.Body;
            var type = memberExpr.Type;

            if (type.IsNullableStruct())
            {
                var hasValuePropForNullable = type.GetProperty(
                             nameof(Nullable<int>.HasValue),
                             BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var newMemberAccess = Expression.MakeMemberAccess(memberExpr, hasValuePropForNullable);
                return Expression.Lambda(newMemberAccess, param);
            }

            var nullComp = Expression.Equal(memberExpr, Expression.Constant(null));
            return Expression.Lambda(nullComp, param);
        }

        private static IQueryable<T> WrapQueryByMethodName<T>(IQueryable<T> query, LambdaExpression sortExpr, string orderMethodName)
        {
            var method = typeof(Queryable).GetMethods().FirstOrDefault(m => m.Name == orderMethodName && m.GetParameters().Length == 2);
            var genericMethod = method.MakeGenericMethod(typeof(T), sortExpr.ReturnType);
            var newQuery = (IQueryable<T>)genericMethod.Invoke(null, new object[] { query, sortExpr });
            return newQuery;
        }

        private string GetSortingMethodName(bool isFirst)
        {
            if (isFirst)
            {
                return IsDescending ? "OrderByDescending" : "OrderBy";
            }

           return IsDescending ? "ThenByDescending" : "ThenBy";
        }

        private LambdaExpression GetPropAccesssLambdaExpr(Type type, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop == null)
            {
                throw new ArgumentException($"Property {name} not found on type {type.Name}");
            }

            if (TreatNullLowest && prop.PropertyType.IsNullable() == false)
            {
                throw new ArgumentException($"{TreatNullLowest} is set to true " + 
                                            $"for a property {name} of type {type.Name} " +
                                            $"which has a type {prop.PropertyType.Name} tha is not nullable.");
            }

            var param = Expression.Parameter(type);
            
            var propAccess = Expression.Property(param, prop.Name);
            var expr = Expression.Lambda(propAccess, param);
            return expr;
        }
    }
}
