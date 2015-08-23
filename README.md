# EPiCode.DynamicMultiSearch

An extension for EPiServer Find's MultiSearch which enables multisearch queries without projections. This means that you can execute several searches for several types, returned in the original state.
Automatic projection handling for IContent items has also been added, so you may search for any type of IContent and not get serialization errors.

##Usage


```c#

var result = client.DynamicMultiSearch()
    .Search<Product>(x => x.Filter(p => p.ProductTypeId.Match(5)))
    .Search<ArticlePage>(Language.English, x => x.For("some search query"))
    .Search<Book>(Language.Norwegian, x => x.Filter(b => b.Title.Match("Necronomicon")))
    .Search<Person>(x => x.For("Per").Select(p => p.Name))
    .GetDynamicResult();
```



In order to retrieve the result sets, use *GetResultsSet*, with the corresponding index number.

```c#

var products = result.GetResultSet<Product>(0);
var articlePages = result.GetResultSet<ArticlePage>(1);
var books = result.GetResultSet<Book>(2);
var personNames = result.GetResultSet<string>(3);
```