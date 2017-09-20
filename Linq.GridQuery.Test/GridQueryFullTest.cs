using Linq.GridQuery.Model;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Linq.GridQuery.Test
{
    [TestFixture]
    public class GridQueryFullTest
    {
        IQueryable<TestSubject> Collection;

        [SetUp]
        public void SetUp()
        {
            Config.DefaultValueDeserialiationFunction = (val, type) =>
            {
                return JsonConvert.DeserializeObject(val, type);
            };

            Collection = (
                 new[]
                 {
                        new TestSubject { A = 0 , C = null  , E = null       },
                        new TestSubject { A = 1 , C = null  , E = null       },
                        new TestSubject { A = 2 , C = 3     , E = TestEnum.B },
                        new TestSubject { A = 3 , C = 5     , E = TestEnum.C },
                        new TestSubject { A = 4 , C = null  , E = TestEnum.B },
                        new TestSubject { A = 5 , C = 3     , E = null       },
                        new TestSubject { A = 6 , C = 3     , E = TestEnum.C },
                        new TestSubject { A = 7 , C = 3     , E = TestEnum.A },
                        new TestSubject { A = 8 , C = null  , E = null       },
                        new TestSubject { A = 9 , C = 3     , E = TestEnum.B },
                        new TestSubject { A = 10 , C = 3    , E = TestEnum.C },
                        new TestSubject { A = 11 , C = null , E = null       },
                        new TestSubject { A = 12 , C = 3    , E = TestEnum.B },
                        new TestSubject { A = 13 , C = 3    , E = TestEnum.C }
                 })
                 .AsQueryable();
        }

        [Test]
        public void FullTest()
        {
            var filter =
                 new FilterTreeNode(
                     new GridFilter("A","gt","3"),
                     LogicalOpertor.AND,
                     new FilterTreeNode(
                             new GridFilter("E", "isnull", "null"),
                             LogicalOpertor.OR,
                             new GridFilter("E", "in", "[1,2]")));

            var sort = new[] 
            {
                new GridSort("C", isDescending: false, treatNullLowest: true),
                new GridSort("E", isDescending: true)
            };

            var gridQuery = new GridRequest()
            {
                Filter = filter,
                Sort = sort,
                Skip = 1,
                Take = 5
            };

            var query = gridQuery.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation =
                "Linq.GridQuery.Test.TestSubject[].Where(par => " +
                "((par.A > 3) And (Not(par.E.HasValue) " +
                "Or IIF(par.E.HasValue, value(Linq.GridQuery.Test.TestEnum[]).Contains(par.E.Value), False))))" +
                ".OrderBy(Param_0 => Param_0.C.HasValue)" + 
                ".ThenBy(Param_0 => Param_0.C)" + 
                ".ThenByDescending(Param_1 => Param_1.E)" + 
                ".Skip(1).Take(5)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new []
            {
                Collection.ElementAt(8),
                Collection.ElementAt(11),
                Collection.ElementAt(9),
                Collection.ElementAt(12),
                Collection.ElementAt(7)
            }));

            var queryWithCount =  gridQuery.WrapQueryWithCount(Collection);
            var result2 = queryWithCount.Query.ToArray();

            Assert.That(result, Is.EquivalentTo(result2));
            Assert.That(queryWithCount.Count, Is.EqualTo(7));

            var resultWithCount = gridQuery.GetQueryResultWithCount(Collection);
            var result3 = resultWithCount.Result.ToArray();

            Assert.That(result, Is.EquivalentTo(result3));
            Assert.That(resultWithCount.Count, Is.EqualTo(7));
        }
    }
}
