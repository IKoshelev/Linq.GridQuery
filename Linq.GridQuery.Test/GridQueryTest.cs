using System;
using System.Linq;
using NUnit;
using NUnit.Framework;
using Newtonsoft.Json;
using Linq.GridQuery.Model;

namespace Linq.GridQuery.Test
{
    [TestFixture]
    public class UnitTest
    {
        IQueryable<TestSubject> Collection;

        [SetUp]
        public void SetUp()
        {
            Config.ValueDeserialiationFunction = (val, type) =>
            {
                return JsonConvert.DeserializeObject(val, type);
            };

            Collection = (
                 new[] {
                        new TestSubject { A = 1 },
                        new TestSubject { A = 3 },
                        new TestSubject { A = 5 }
                 })
                 .AsQueryable();
        }

        [Test]
        public void SingleFitlerWorks()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "4",
                    Operand = "Gt"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A > 4))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(1));
            Assert.That(query.Single().A, Is.EqualTo(5));
        }

        [Test]
        public void TwoBranchesQueryWorks()
        {
            var filter = new FilterTreeNode(
               new FilterTreeNode(
                   new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "3",
                        Operand = "Eq"
                    }),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "5",
                        Operand = "Eq"
                    })
               );

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => ((par.A == 3) Or (par.A == 5)))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(2));
            Assert.That(query.ElementAt(0).A, Is.EqualTo(3));
            Assert.That(query.ElementAt(1).A, Is.EqualTo(5));
        }

        [Test]
        public void ThreeBranchesQueryWorks()
        {
            var filter = new FilterTreeNode(
               new FilterTreeNode(
                   new FilterTreeNode(
                       new GridFilter()
                       {
                           PropName = "A",
                           StringValue = "2",
                           Operand = "Gt"
                       }),
                        LogicalOpertor.AND,
                        new FilterTreeNode(
                            new GridFilter()
                            {
                                PropName = "A",
                                StringValue = "4",
                                Operand = "Lt"
                            })
                       ),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "5",
                        Operand = "Eq"
                    }));

                    var query = filter.WrapFilter(Collection);
                    var result = query.ToArray();

                    var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (((par.A > 2) And (par.A < 4)) Or (par.A == 5)))";
                    Assert.That(query.ToString(), Is.EqualTo(expectation));
                    Assert.That(query.Count, Is.EqualTo(2));
                    Assert.That(query.ElementAt(0).A, Is.EqualTo(3));
                    Assert.That(query.ElementAt(1).A, Is.EqualTo(5));
                }

        [Test]
        public void ThreeBranchesQueryWorks2()
        {
            var filter = new FilterTreeNode(
               new FilterTreeNode(
                   new FilterTreeNode(
                       new GridFilter()
                       {
                           PropName = "A",
                           StringValue = "2",
                           Operand = "Gt"
                       }),
                        LogicalOpertor.AND,
                        new FilterTreeNode(
                            new GridFilter()
                            {
                                PropName = "A",
                                StringValue = "4",
                                Operand = "Lt"
                            })
                       ),
                LogicalOpertor.AND,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "5",
                        Operand = "Eq"
                    }));

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (((par.A > 2) And (par.A < 4)) And (par.A == 5)))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(0));
        }
    }
}
