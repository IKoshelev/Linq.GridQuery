using System;
using System.Linq;
using NUnit;
using NUnit.Framework;
using Newtonsoft.Json;
using Linq.GridQuery.Model;
using System.Collections.Generic;

namespace Linq.GridQuery.Test
{
    [TestFixture]
    public class GridFilterTest
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

        [Test]
        public void ByDefaultValueDeserializationFunctionIsTakenFromConfig()
        {
            var marker1 = "MARKER1";
            string valCapture = null;
            Type typeCapture = null;
            Config.DefaultValueDeserialiationFunction = (val, type) => 
            {
                valCapture = val;
                typeCapture = type;
                return 5;
            }; 

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = marker1,
                    Operand = "Eq"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A == 5))";
            Assert.That(valCapture, Is.EqualTo(marker1));
            Assert.That(typeCapture, Is.EqualTo(typeof(int)));
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(1));
            Assert.That(query.Single().A, Is.EqualTo(5));
        }

        [Test]
        public void NoValueDeserializationThrowsOnWrap()
        {
            Config.DefaultValueDeserialiationFunction = null;

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "1",
                    Operand = "Eq"
                } );

            Assert.Throws<InvalidOperationException>(() =>
           {
               var query = filter.WrapFilter(Collection);
           });
        }

        [Test]
        public void ValueDeserializationFunctionOverrideCanBeSet()
        {
            var marker1 = "MARKER1";
            string valCapture = null;
            Type typeCapture = null;
            Config.DefaultValueDeserialiationFunction = (val, type) =>
            {
                throw new InvalidOperationException("This should not be called.");
            };

            Func<string, Type, object> actuallyUsedFunc = (val, type) =>
             {
                 valCapture = val;
                 typeCapture = type;
                 return 5;
             };

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = marker1,
                    Operand = "Eq"
                },
                actuallyUsedFunc);

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A == 5))";
            Assert.That(filter.ValueDeserializationFunctionOverride, Is.EqualTo(actuallyUsedFunc));
            Assert.That(valCapture, Is.EqualTo(marker1));
            Assert.That(typeCapture, Is.EqualTo(typeof(int)));
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(1));
            Assert.That(query.Single().A, Is.EqualTo(5));
        }

        [Test]
        public void ValueDeserializationFunctionCanBeSetForRootOfTreeAndFunctionOfEachBranchFlowsDowntreeForNodesWithoutTheirOwn()
        {
            var marker1 = "MARKER1";
            var marker2 = "MARKER2";
            var valCapture = new List<string>();
            var typeCapture = new List<Type>();
            Config.DefaultValueDeserialiationFunction = (val, type) =>
            {
                throw new InvalidOperationException("This should not be called.");
            };

            Func<string, Type, object> deserializationFunc1 = (val, type) =>
            {
                valCapture.Add(val);
                typeCapture.Add(type);
                return val == marker1 ? 3 :
                       val == marker2 ? 5 :
                                        0;
            };

            Func<string, Type, object> deserializationFunc2 = (val, type) =>
            {
                return JsonConvert.DeserializeObject(val, type);
            };

            var branchWithOwnFunction = new FilterTreeNode(
                new FilterTreeNode(
                   new GridFilter()
                   {
                       PropName = "A",
                       StringValue = marker1,
                       Operand = "Eq"
                   }),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = marker2,
                        Operand = "Eq"
                    }),
                deserializationFunc1);

            var branchWithoutOwnFunction = new FilterTreeNode(
                new FilterTreeNode(
                       new GridFilter()
                       {
                           PropName = "A",
                           StringValue = "2",
                           Operand = "Eq"
                       }),
                    LogicalOpertor.OR,
                    new FilterTreeNode(
                        new GridFilter()
                        {
                            PropName = "A",
                            StringValue = "4",
                            Operand = "Eq"
                        }));

            var filter = new FilterTreeNode(
                branchWithOwnFunction,
                LogicalOpertor.OR,
                branchWithoutOwnFunction,
                deserializationFunc2);

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (((par.A == 3) Or (par.A == 5)) Or ((par.A == 2) Or (par.A == 4))))";
            Assert.That(filter.ValueDeserializationFunctionOverride, Is.EqualTo(deserializationFunc2));
            Assert.That(valCapture, Is.EquivalentTo(new[] { marker1, marker2}));
            Assert.That(typeCapture, Is.EquivalentTo(new[] { typeof(int), typeof(int) }));
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(2));
            Assert.That(query.ElementAt(0).A, Is.EqualTo(3));
            Assert.That(query.ElementAt(1).A, Is.EqualTo(5));
        }
    }
}
