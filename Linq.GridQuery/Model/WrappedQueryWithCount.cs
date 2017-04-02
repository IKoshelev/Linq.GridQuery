using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linq.GridQuery.Model
{
    public class WrappedQueryWithCount<T>
    {
        public IQueryable<T> Query { get; set; }

        public long Count { get; set; }
    }
}
