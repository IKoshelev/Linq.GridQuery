using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linq.GridQuery.Test
{
    public enum TestEnum
    {
        A = 1,
        B = 2,
        C = 3
    }

    public class TestSubject
    {
        public int A { get; set; }
        public string B { get; set; }
        public int? C { get; set; }
        public TestEnum D { get; set; }
        public TestEnum? E { get; set; }
    }
}
