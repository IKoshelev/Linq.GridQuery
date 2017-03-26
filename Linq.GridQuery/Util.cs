using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linq.GridQuery
{
    public static class Util
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsClass
                    || type.IsNullableStruct();
        }

        public static bool IsNullableStruct(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static TValue GetOrNull<TKev, TValue>(this IDictionary<TKev, TValue> dict, TKev key)
            where TValue : class
        {
            TValue value = null;
            dict.TryGetValue(key, out value);
            return value;
        }
    }
}
