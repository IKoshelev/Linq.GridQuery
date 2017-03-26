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
    public class GridSortTest
    {
        IQueryable<TestSubject> Collection;

        [SetUp]
        public void SetUp()
        {
            Config.DefaultOperatorToExpressionConverters.Clear();

            Config.DefaultValueDeserialiationFunction = (val, type) =>
            {
                return JsonConvert.DeserializeObject(val, type);
            };

            Collection = (
                 new[]
                 {
                        new TestSubject { A = 1 , B = null  , C = null  , D = TestEnum.A , E = null       },
                        new TestSubject { A = 3 , B = "BBB" , C = 3     , D = TestEnum.B , E = TestEnum.B },
                        new TestSubject { A = 5 , B = "AAA" , C = 5     , D = TestEnum.C , E = TestEnum.C }
                 })
                 .AsQueryable();
        }

        [Test]
        public void SingleSortWorks()
        {
            var sort = new[] { new GridSort("E") };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].OrderBy(Param_0 => Param_0.E)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(1),
                Collection.ElementAt(2),
                Collection.ElementAt(0)
            }));
        }

        [Test]
        public void SortOnWrongPropertyThrows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var sort = new[] { new GridSort("X") };

                var request = new GridRequest()
                {
                    Sort = sort
                };

                var query = request.WrapQuery(Collection);
            });
        }

        [Test]
        public void SingleSortDescWorks()
        {
            var sort = new[] { new GridSort("E", true) };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].OrderByDescending(Param_0 => Param_0.E)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(0),
                Collection.ElementAt(2),
                Collection.ElementAt(1)               
            }));
        }

        [Test]
        public void MultiSortcWorks()
        {
            var sort = new[] 
            {
                new GridSort("E", true),
                new GridSort("C" ),
                new GridSort("D", true)
            };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[]" +
                              ".OrderByDescending(Param_0 => Param_0.E)" + 
                              ".ThenBy(Param_1 => Param_1.C)" + 
                              ".ThenByDescending(Param_2 => Param_2.D)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(2),
                Collection.ElementAt(1),
                Collection.ElementAt(0)
            }));
        }

        [Test]
        public void SingleSortNullLowestWorks()
        {
            var sort = new[] { new GridSort("E", treatNullLowest: true) };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[]" + 
                ".OrderBy(Param_0 => Param_0.E.HasValue)" + 
                ".ThenBy(Param_0 => Param_0.E)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(0),
                Collection.ElementAt(1),
                Collection.ElementAt(2)
            }));
        }

        [Test]
        public void NullLowestOnNoNullableTrhows()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var sort = new[] { new GridSort("A", treatNullLowest: true) };

                var request = new GridRequest()
                {
                    Sort = sort
                };

                var query = request.WrapQuery(Collection);
            });
        }

        [Test]
        public void SingleSortNullLowestWorksWithClass()
        {
            var sort = new[] { new GridSort("B", treatNullLowest: true) };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[]" +
                ".OrderBy(Param_0 => (Param_0.B == null))" +
                ".ThenBy(Param_0 => Param_0.B)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(0),
                Collection.ElementAt(1),
                Collection.ElementAt(2)
            }));
        }

        [Test]
        public void SingleSortDescNullLowestWorksWithClass()
        {
            var sort = new[] { new GridSort("B", true, true) };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[]" +
                ".OrderByDescending(Param_0 => (Param_0.B == null))" +
                ".ThenByDescending(Param_0 => Param_0.B)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(2),
                Collection.ElementAt(1),
                Collection.ElementAt(0)
            }));
        }

        [Test]
        public void MultiSortcNullLowesWorks()
        {
            var sort = new[]
            {
                new GridSort("E", true, true),
                new GridSort("C", false, true ),
                new GridSort("B", true, true)
            };

            var request = new GridRequest()
            {
                Sort = sort
            };

            var query = request.WrapQuery(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[]"+
                ".OrderByDescending(Param_0 => Param_0.E.HasValue)"+
                ".ThenByDescending(Param_0 => Param_0.E)"+
                ".ThenBy(Param_1 => Param_1.C.HasValue)" +
                ".ThenBy(Param_1 => Param_1.C)" +
                ".ThenByDescending(Param_2 => (Param_2.B == null))" +
                ".ThenByDescending(Param_2 => Param_2.B)";

            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(new[]
            {
                Collection.ElementAt(2),
                Collection.ElementAt(1),
                Collection.ElementAt(0)
            }));
        }
    }
}
