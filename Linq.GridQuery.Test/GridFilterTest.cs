using System;
using System.Linq;
using NUnit;
using NUnit.Framework;
using Newtonsoft.Json;
using Linq.GridQuery.Model;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Linq.GridQuery.Test
{
    [TestFixture]
    public class GridFilterTest
    {
        IQueryable<TestSubject> Collection;

        Dictionary<string, OperatorHandler> pristineDefaultOperatorToExpressionConverters;

        public GridFilterTest()
        {
            pristineDefaultOperatorToExpressionConverters = Config.DefaultOperatorToExpressionConverters.ToDictionary(x => x.Key, x => x.Value);
        }

        [SetUp]
        public void SetUp()
        {
            Config.DefaultOperatorToExpressionConverters.Clear();

            pristineDefaultOperatorToExpressionConverters.ToList().ForEach(x =>
            {
                Config.DefaultOperatorToExpressionConverters.Add(x.Key, x.Value);
            });

            Config.DefaultValueDeserialiationFunction = (val, type) =>
            {
                return JsonConvert.DeserializeObject(val, type);
            };

            Collection = (
                 new[] 
                 {
                        new TestSubject { A = 1 , C = null  , D = TestEnum.A , E = null       },
                        new TestSubject { A = 3 , C = 3     , D = TestEnum.B , E = TestEnum.B },
                        new TestSubject { A = 5 , C = 5     , D = TestEnum.C , E = TestEnum.C }
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
                    Operator = "gt"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.A > 4))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(1));
            Assert.That(query.Single().A, Is.EqualTo(5));
        }

        [Test]
        public void CanHanldeNullables()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "C",
                    StringValue = "3",
                    Operator = "gt"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => IIF(par.C.HasValue, (par.C.Value > 3), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(2)));
        }

        [Test]
        public void CanHanldeEnums()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "D",
                    StringValue = "3",
                    Operator = "eq"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (par.D == C))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(2)));
        }

        [Test]
        public void CanHanldeNullableEnums()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "E",
                    StringValue = "3",
                    Operator = "eq"
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => IIF(par.E.HasValue, (par.E.Value == C), False))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(result, Is.EquivalentTo(Collection.Skip(2)));
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
                       Operator = "eq"
                   }),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "5",
                        Operator = "eq"
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
                           Operator = "gt"
                       }),
                        LogicalOpertor.AND,
                        new FilterTreeNode(
                            new GridFilter()
                            {
                                PropName = "A",
                                StringValue = "4",
                                Operator = "lt"
                            })
                       ),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "5",
                        Operator = "eq"
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
                           Operator = "gt"
                       }),
                        LogicalOpertor.AND,
                        new FilterTreeNode(
                            new GridFilter()
                            {
                                PropName = "A",
                                StringValue = "4",
                                Operator = "lt"
                            })
                       ),
                LogicalOpertor.AND,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = "5",
                        Operator = "eq"
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
                    Operator = "eq"
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
        public void NoValueDeserializationFunctionFoundThrowsOnWrap()
        {
            Config.DefaultValueDeserialiationFunction = null;

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "1",
                    Operator = "eq"
                });

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
                    Operator = "eq"
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
                       Operator = "eq"
                   }),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue = marker2,
                        Operator = "eq"
                    }),
                deserializationFunc1);

            var branchWithoutOwnFunction = new FilterTreeNode(
                new FilterTreeNode(
                       new GridFilter()
                       {
                           PropName = "A",
                           StringValue = "2",
                           Operator = "eq"
                       }),
                    LogicalOpertor.OR,
                    new FilterTreeNode(
                        new GridFilter()
                        {
                            PropName = "A",
                            StringValue = "4",
                            Operator = "eq"
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
            Assert.That(valCapture, Is.EquivalentTo(new[] { marker1, marker2 }));
            Assert.That(typeCapture, Is.EquivalentTo(new[] { typeof(int), typeof(int) }));
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(2));
            Assert.That(query.ElementAt(0).A, Is.EqualTo(3));
            Assert.That(query.ElementAt(1).A, Is.EqualTo(5));
        }

        [Test]
        public void ByDefaultOperatorToExpressionConvertersAreTakenFromStaticProperty()
        {
            var marker1 = "MARKER1";
            var marker2 = "MARKER2";
            var wasCalled = false;
            Type typePassed = null;
            Config.DefaultOperatorToExpressionConverters.Add(marker2, new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
            {
                wasCalled = true;
                typePassed = propTypeUnwrapped;
                return Expression.Equal(Expression.Constant(marker1), Expression.Constant(marker1));
            }));

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "0",
                    Operator = marker2
                });

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (\"MARKER1\" == \"MARKER1\"))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(wasCalled, Is.EqualTo(true));
            Assert.That(typePassed, Is.EqualTo(typeof(Int32)));
            Assert.That(result, Is.EquivalentTo(Collection));
        }

        [Test]
        public void ThrowsIfOperatorToExpressionConverterIsNotFound()
        {
            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "0",
                    Operator = "IGNORE"
                });

            Assert.Throws<ArgumentException>(() =>
            {
                var query = filter.WrapFilter(Collection);
            });        
        }

        [Test]
        public void AdditionalOperatorToExpressionConvertersCanBeProvidedOnPerInstanceBasis()
        {
            var marker1 = "MARKER1";
            var marker2 = "MARKER2";
            var wasCalled = false;
            Type typePassed = null;

            var additionalOperatorToExpressionConverters =
                new Dictionary<string, OperatorHandler>()
                {
                   { marker2,  new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                    {
                        wasCalled = true;
                        typePassed = propTypeUnwrapped;
                        return Expression.Equal(Expression.Constant(marker1), Expression.Constant(marker1));
                    })}
                };

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "0",
                    Operator = marker2
                },
                operatorToExpressionConvertersOverrides: additionalOperatorToExpressionConverters);

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (\"MARKER1\" == \"MARKER1\"))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(wasCalled, Is.EqualTo(true));
            Assert.That(typePassed, Is.EqualTo(typeof(Int32)));
            Assert.That(result, Is.EquivalentTo(Collection));
        }

        [Test]
        public void AdditionalOperatorToExpressionConvertersProvidedOnPerInstanceBasisOverrideBaseOnesOfTheSameName()
        {
            var marker1 = "MARKER1";
            var marker2 = "MARKER2";
            var wasCalled = false;
            Type typePassed = null;

            Config.DefaultOperatorToExpressionConverters.Add(marker2, new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
            {
                throw new Exception("This should never be called.");
            }));

            var additionalOperatorToExpressionConverters =
                new Dictionary<string, OperatorHandler>()
                {
                    { marker2, new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                    {
                        wasCalled = true;
                        typePassed = propTypeUnwrapped;
                        return Expression.Equal(Expression.Constant(marker1), Expression.Constant(marker1));
                    })}
                };

            var filter = new FilterTreeNode(
                new GridFilter()
                {
                    PropName = "A",
                    StringValue = "0",
                    Operator = marker2
                },
                operatorToExpressionConvertersOverrides: additionalOperatorToExpressionConverters);

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (\"MARKER1\" == \"MARKER1\"))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(wasCalled, Is.EqualTo(true));
            Assert.That(typePassed, Is.EqualTo(typeof(Int32)));
            Assert.That(result, Is.EquivalentTo(Collection));
        }

        [Test]
        public void AdditionalOperatorToExpressionConvertersCanBeSetForRootOfTreeAndFunctionOfEachBranchFlowsDowntreeForNodesWithoutTheirOwn()
        {
            var marker2 = "MARKER2";
            var marker3 = "MARKER3";

            Type typePassed = null;

            var additionalOperatorToExpressionConverters1 =
                new Dictionary<string, OperatorHandler>()
                {
                    { marker3, new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                    {
                        typePassed = propTypeUnwrapped;
                        return Expression.Equal(Expression.Constant(marker3), Expression.Constant(marker3));
                    })},
                    {
                        marker2, new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                        {
                            throw new Exception("This should never be called.");
                        })}
                };

            var additionalOperatorToExpressionConverters2 =
                new Dictionary<string, OperatorHandler>()
                {
                    { marker2, new OperatorHandler((propTypeUnwrapped, propTypeRaw, left, right) =>
                    {
                        typePassed = propTypeUnwrapped;
                        return Expression.Equal(Expression.Constant(marker2), Expression.Constant(marker2));
                    })}
                };

            var branchWithOwnFunction = new FilterTreeNode(
                new FilterTreeNode(
                   new GridFilter()
                   {
                       PropName = "A",
                       StringValue = "1",
                       Operator = "eq"
                   }),
                LogicalOpertor.OR,
                new FilterTreeNode(
                    new GridFilter()
                    {
                        PropName = "A",
                        StringValue ="0",
                        Operator = marker2
                    }),
                operatorToExpressionConvertersOverrides: additionalOperatorToExpressionConverters2);

            var branchWithoutOwnFunction = new FilterTreeNode(
                new FilterTreeNode(
                       new GridFilter()
                       {
                           PropName = "A",
                           StringValue = "2",
                           Operator = "eq"
                       }),
                    LogicalOpertor.OR,
                    new FilterTreeNode(
                        new GridFilter()
                        {
                            PropName = "A",
                            StringValue = "0",
                            Operator = marker3
                        }));

            var filter = new FilterTreeNode(
                branchWithOwnFunction,
                LogicalOpertor.OR,
                branchWithoutOwnFunction,
                operatorToExpressionConvertersOverrides: additionalOperatorToExpressionConverters1);

            var query = filter.WrapFilter(Collection);
            var result = query.ToArray();

            var expectation = "Linq.GridQuery.Test.TestSubject[].Where(par => (((par.A == 1) Or (\"MARKER2\" == \"MARKER2\")) Or ((par.A == 2) Or (\"MARKER3\" == \"MARKER3\"))))";
            Assert.That(query.ToString(), Is.EqualTo(expectation));
            Assert.That(query.Count, Is.EqualTo(3));
            Assert.That(query.ElementAt(0).A, Is.EqualTo(1));
            Assert.That(query.ElementAt(1).A, Is.EqualTo(3));
            Assert.That(query.ElementAt(2).A, Is.EqualTo(5));
        }

    }
}
