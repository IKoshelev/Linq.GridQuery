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
                    || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
