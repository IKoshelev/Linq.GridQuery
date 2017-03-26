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
    class GridFilterPredefinedOperatorsTest
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
                 new[] {
                        new TestSubject { A = 1, B = "AAA" , E = null },
                        new TestSubject { A = 3, B = "BCB" , E = TestEnum.B},
                        new TestSubject { A = 5, B = null , E = TestEnum.C}
                 })
                 .AsQueryable();
        }

        [Test]
        public void EqTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "3",
                    Operator = "eq"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A == 3))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1).Take(1)));
        }

        [Test]
        public void NeqTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "1",
                    Operator = "neq"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A != 1))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1)));
        }

        [Test]
        public void GtTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "3",
                    Operator = "gt"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A > 3))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(2)));
        }

        [Test]
        public void GteTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "3",
                    Operator = "gte"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A >= 3))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1)));
        }

        [Test]
        public void LtTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "3",
                    Operator = "lt"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A < 3))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(1)));
        }

        [Test]
        public void LteTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "3",
                    Operator = "lte"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A <= 3))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(2)));
        }

        [Test]
        public void InTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "[1,3]",
                    Operator = "in"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => value(System.Int32[]).Contains(par.A))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(2)));
        }

        [Test]
        public void InOrNullTest()
        {
            var filter = 
                new FilterTreeNode(
                    new FilterTreeNode(
                        new GridFilter()
                        {
                            PropName = "E",
                            StringValue = "null",
                            Operator = "isnull"
                        }),
                        LogicalOpertor.OR,
                        new FilterTreeNode(
                            new GridFilter()
                            {
                                PropName = "E",
                                StringValue = "[1,2]",
                                Operator = "in"
                            }));

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation =
                "Linq.GridQuery.Test.TestSubject[].Where(par => (Not(par.E.HasValue) Or IIF(par.E.HasValue, value(Linq.GridQuery.Test.TestEnum[]).Contains(par.E.Value), False)))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(2)));
        }

        [Test]
        public void NotInTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "[1,5]",
                    Operator = "notin"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => Not(value(System.Int32[]).Contains(par.A)))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1).Take(1)));
        }

        [Test]
        public void NotInTestNullableEnum()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "E",
                    StringValue = "[1,3]",
                    Operator = "notin"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = 
                "Linq.GridQuery.Test.TestSubject[].Where(par => IIF(par.E.HasValue, Not(value(Linq.GridQuery.Test.TestEnum[]).Contains(par.E.Value)), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1).Take(1)));
        }

        [Test]
        public void IsNull()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "E",
                    StringValue = "null",
                    Operator = "isnull"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => Not(par.E.HasValue))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(1)));

            filter = new FilterTreeNode(
              new GridFilter()
              {
                  PropName = "B",
                  StringValue = "null",
                  Operator = "isnull"
              });

            query = filter.WrapFilter(Collection);
            result = query.ToArray();

            expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.B == null))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(2).Take(1)));
        }

        [Test]
        public void IsNullThrowsForNonNullableTypes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "0",
                    Operator = "isnull"
                });

                var query = filter.WrapFilter(Collection);
                var result = query.ToArray();
            });
        }

        [Test]
        public void IsNotNull()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "E",
                    StringValue = "null",
                    Operator = "isnotnull"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => par.E.HasValue)";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1)));

            filter = new FilterTreeNode(
            new GridFilter()
            {
                PropName = "B",
                StringValue = "null",
                Operator = "isnotnull"
            });

            query = filter.WrapFilter(Collection);
            result = query.ToArray();

            expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.B != null))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(2)));
        }

        [Test]
        public void IsNotNullThrowsForNonNullableTypes()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "0",
                    Operator = "isnotnull"
                });

                var query = filter.WrapFilter(Collection);
                var result = query.ToArray();
            });
        }

        [Test]
        public void ContainsTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "B",
                    StringValue = "'C'",
                    Operator = "contains"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => IIF((par.B != null), par.B.Contains(\"C\"), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(1).Take(1)));
        }

        [Test]
        public void DoesNotContainTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "B",
                    StringValue = "'C'",
                    Operator = "doesnotcontain"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => IIF((par.B != null), Not(par.B.Contains(\"C\")), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(1)));
        }

        [Test]
        public void StartsWithTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "B",
                    StringValue = "'A'",
                    Operator = "startswith"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => IIF((par.B != null), par.B.StartsWith(\"A\"), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(1)));
        }

        [Test]
        public void EndsWithTest()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "B",
                    StringValue = "'A'",
                    Operator = "endswith"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => IIF((par.B != null), par.B.EndsWith(\"A\"), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Take(1)));
        }
    }
}
