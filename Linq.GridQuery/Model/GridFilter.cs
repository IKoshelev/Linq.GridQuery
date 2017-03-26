using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linq.GridQuery.Model
{
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

        public string PropName { get; set; }
        public string StringValue { get; set; }
        public string Operator { get; set; }
    }
}
