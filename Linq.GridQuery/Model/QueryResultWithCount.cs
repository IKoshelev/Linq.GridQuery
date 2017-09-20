using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Linq.GridQuery.Model
{
    [DataContract]
    public class QueryResultWithCount<T>
    {
        [DataMember]
        public T[] Result { get; set; }
        [DataMember]
        public long Count { get; set; }
    }
}
