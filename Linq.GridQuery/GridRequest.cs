using Linq.GridQuery.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Linq.GridQuery
{
    [DataContract]
    public class GridRequestWithAdditionalPayload<T> : GridRequest
    {
        public T Payload { get; set; }
    }

    [DataContract]
    public class GridRequest
    {
        [DataMember]
        public GridSort[] Sort { get; set; } = new GridSort[0];

        [DataMember]
        public FilterTreeNode Filter { get; set; }

        [DataMember]
        public int? Skip { get; set; }

        [DataMember]
        public int? Take { get; set; }

        public IQueryable<T> WrapQuery<T>(IQueryable<T> initialQuery)
        {
            var query = initialQuery;

            query = WrapFilter(query);

            query = WrapSort(query);

            query = WrapSkipTake(query);

            return query;
        }

        public WrappedQueryWithCount<T> WrapQueryWithCount<T>(IQueryable<T> initialQuery)
        {
            var result = new WrappedQueryWithCount<T>();

            var query = initialQuery;

            query = WrapFilter(query);

            query = WrapSort(query);

            result.Count = query.Count();

            query = WrapSkipTake(query);

            result.Query = query;

            return result;
        }

        private IQueryable<T> WrapSkipTake<T>(IQueryable<T> initialQuery)
        {
            var query = initialQuery;

            if (Skip.HasValue)
            {
                query = query.Skip(Skip.Value);
            }

            if (Take.HasValue)
            {
                query = query.Take(Take.Value);
            }

            return query;
        }

        private IQueryable<T> WrapFilter<T>(IQueryable<T> initialQuery)
        {
            if (Filter == null)
            {
                return initialQuery;
            }

            var newQuery = Filter.WrapFilter<T>(initialQuery);

            return newQuery;
        }

        private IQueryable<T> WrapSort<T>(IQueryable<T> initialQuery)
        {
            if (!Sort.Any())
            {
                return initialQuery;
            }

            var query = initialQuery;

            var isFirst = true;
            foreach (var sort in Sort)
            {
                query = sort.WrapSort(query, isFirst);
                isFirst = false;
            }

            return query;
        }
    }
}
