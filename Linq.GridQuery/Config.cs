using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linq.GridQuery
{
    public static class Config
    {
        public static Func<string, Type, object> ValueDeserialiationFunction { get; set; }
    }
}
