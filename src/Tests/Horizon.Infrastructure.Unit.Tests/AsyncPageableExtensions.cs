using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Moq;

namespace Horizon.Infrastructure.Unit.Tests;

public static class AsyncPageableExtensions
{
    public static AsyncPageable<T> ToAsyncPageable<T>(this IEnumerable<T> source, int pageSize = 10) where T : notnull
    {
        return new MockAsyncPageable<T>(source, pageSize);
    }

    private class MockAsyncPageable<T> : AsyncPageable<T> where T: notnull
    {
        private readonly IEnumerable<T> _source;
        private readonly int _pageSize;

        public MockAsyncPageable(IEnumerable<T> source, int pageSize)
        {
            _source = source;
            _pageSize = pageSize;
        }

        public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            foreach (var item in _source)
            {
                yield return item;
                await Task.Yield(); // Simulate async behavior
            }
        }

        public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
        {
            var pageSize = pageSizeHint ?? _pageSize;
            var enumerator = _source.GetEnumerator();

            while (enumerator.MoveNext())
            {
                var items = new List<T>();

                do
                {
                    items.Add(enumerator.Current);
                }
                while (items.Count < pageSize && enumerator.MoveNext());

                yield return Page<T>.FromValues(items, continuationToken: null, response: Mock.Of<Response>());

                await Task.Yield(); // Simulate async behavior
            }
        }
    }
}