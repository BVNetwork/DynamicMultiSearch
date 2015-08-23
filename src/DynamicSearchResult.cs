using System.Collections;
using System.Collections.Generic;
using EPiServer.Find;

namespace EPiCode.DynamicMultiSearch
{
    public class DynamicSearchResult<T> : IEnumerable<T>
    {
        public DynamicSearchResult(SearchResults<dynamic> results, IEnumerable<T> items)
        {
            SearchResult = results;
            Items = items;
        }

        protected IEnumerable<T> Items { get; set; }
        public SearchResults<dynamic> SearchResult { get; set; }
        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}