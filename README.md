Library is available via NuGet prackage https://www.nuget.org/packages/Linq.GridQuery.NetStandard

Description for primary (master) project:


Library is available via NuGet prackage https://www.nuget.org/packages/Linq.GridQuery

Inner workings of the library are explained in articles:

http://ikoshelev.azurewebsites.net/search/id/1/Expression-trees-and-advanced-queries-in-CSharp-01-IQueryable-and-Expression-Tree-basics

http://ikoshelev.azurewebsites.net/search/id/2/Expression-trees-and-advanced-queries-in-CSharp-02-IQueryable-composition


# Linq.GridQuery
Library to wrap IQueryable with filter and sort queries received in serialized form (i.e. from JavaScript front-end). Works by way of expression trees.

1. You create an instance of GridRequest based on the string descriptions of the query you have, as showcased below.
2. You wrap your IQueryable (i.e. an Entity Framework query) with that GridRequest.
3. Your query is now filtered, sorted, and paged. As you would expect from an IQueryable, this is all done with strong-typed expression trees, and will be handled by your query provider, as if you wrote the tress yorself (normally, that means, no in-memory processing). 

# 
```C#
// provide a deserialization function usable with your particular serialization scenario 
// (you can also pass one to individual FilterTreeNodes if your app has multiple scenarios)
            Config.DefaultValueDeserialiationFunction = (val, type) =>
            {
                return JsonConvert.DeserializeObject(val, type);
            };

//filter object equivalent to 
//.Where(item => item.A > 3 
//               && (item.E.HasValue == false 
//                   || (new[] { TestEnum.A, TestEnum.B }).Contains(item.E.Value)))
            var filter =
                 new FilterTreeNode(
                     new GridFilter("A","gt","3"),
                     LogicalOpertor.AND,
                     new FilterTreeNode(
                             new GridFilter("E", "isnull", "null"),
                             LogicalOpertor.OR,
                             new GridFilter("E", "in", "[1,2]")));
                             
//sort object equvalent to
//.OrderBy(item => item.C.HasValue)   //nulls first
//.ThenBy(item => item.C)
//.ThenByDescending(item => item.E)
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
            
            var Collection = (new[]
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

            var query = gridQuery.WrapQuery(Collection);
            var result = query.ToArray(); // 5 items in result
```
