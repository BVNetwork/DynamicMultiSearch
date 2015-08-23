using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiCode.DynamicMultiSearch
{
    public static class DynamicMultiSearchExtensions
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        public static DynamicSearchResult<T> GetResultSet<T>(this IEnumerable<SearchResults<dynamic>> result, int index)
        {
            var resultSet = result.Skip(index).First();
            if (typeof(IContent).IsAssignableFrom(typeof(T)))
            {
                var contentInLanguageReferences = resultSet.Cast<ContentInLanguageReference>().ToList();
                var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                var dictionary = new Dictionary<ContentInLanguageReference, IContent>();
                foreach (IGrouping<string, ContentInLanguageReference> grouping in contentInLanguageReferences.GroupBy(x => x.Language))
                {
                    foreach (IContent content in contentRepository.GetItems(grouping.Select(x => x.ContentLink).ToList(), new LanguageSelector(grouping.Key)))
                        dictionary[new ContentInLanguageReference(content)] = content;
                }
                var contentList = new List<T>();
                foreach (var key in contentInLanguageReferences)
                {
                    IContent content;
                    if (dictionary.TryGetValue(key, out content) && content != null)
                    {
                        contentList.Add((T)content);
                    }
                    else
                    {
                        Logger.Warning("Search results contain reference to a content with reference \"{0}\" in language \"{1}\" but no such content could been found.", key.ContentLink, key.Language);
                    }
                }
                return new DynamicSearchResult<T>(resultSet, contentList);
            }
            IEnumerable<T> items = resultSet.Cast<T>();
            return new DynamicSearchResult<T>(resultSet, items);
        }

        public static IMultiSearch<dynamic> DynamicMultiSearch(this IClient client)
        {
            return client.MultiSearch<dynamic>();
        }

        public static IEnumerable<SearchResults<TResult>> GetDynamicResult<TResult>(
            this IMultiSearch<TResult> multiSearch)
        {
            var unprojectedSearches = multiSearch.Searches.ToList();
            multiSearch.Searches.Clear();
            foreach (var originalSearch in unprojectedSearches)
            {
                var projectedSearch = originalSearch;
                var searchContext = new SearchContext();
                projectedSearch.ApplyActions(searchContext);
                if (searchContext.Projections.Count == 0)
                {
                    if (originalSearch is ITypeSearch<IContent>)
                    {
                        var search = originalSearch as ITypeSearch<IContent>;
                        if (typeof(IContent).IsAssignableFrom(typeof(IContent)))
                            search = search.Filter(x => ((IContentData)x).MatchTypeHierarchy(typeof(IContent)));
                        ISearch<ContentInLanguageReference> contentInLanguageReferenceSearch = search.Select(x => new ContentInLanguageReference(new ContentReference(x.ContentLink.ID, x.ContentLink.ProviderName), ((ILocalizable)x).Language.Name));
                        projectedSearch = contentInLanguageReferenceSearch as ITypeSearch<TResult>;
                    }
                    else
                    {
                        projectedSearch = (projectedSearch as ITypeSearch<TResult>).Select(x => x);
                    }
                }
                multiSearch.Searches.Add(projectedSearch);
            }
            return multiSearch.GetResult();
        }

        public static IMultiSearch<dynamic> Search<TSource>(this IMultiSearch<dynamic> multiSearch, Language language, Func<ITypeSearch<TSource>, ISearch<dynamic>> searchFunction)
        {
            return MultiSearchExtensions.Search(multiSearch,language, searchFunction);
        }

        public static IMultiSearch<dynamic> Search<TSource>(this IMultiSearch<dynamic> multiSearch, Func<ITypeSearch<TSource>, ISearch<dynamic>> searchFunction)
        {
            return multiSearch.Search(Language.None, searchFunction);
        }
    }
}