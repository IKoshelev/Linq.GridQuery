using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Linq.GridQuery.Model
{
    [DataContract(Name = "LogicalOpertor")]
    public enum LogicalOpertor
    {
        [EnumMember]AND,
        [EnumMember]OR
    }
}
