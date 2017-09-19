using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Linq.GridQuery.Model
{
    [DataContract]
    public class GridFilter
    {
        public GridFilter()
        {

        }

        public GridFilter(string propName,
                          string @operator,
                          string stringValue)
        {
            PropName = propName;
            Operator = @operator;
            StringValue = stringValue;
        }

        [DataMember]
        public string PropName { get; set; }
        [DataMember]
        public string StringValue { get; set; }
        [DataMember]
        public string Operator { get; set; }
    }
}
