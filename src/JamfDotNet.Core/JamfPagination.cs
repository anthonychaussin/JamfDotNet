using System.Runtime.CompilerServices;

namespace JamfDotNet.Core;

/// <summary>
/// Helpers that turn paginated Jamf API calls into <see cref="IAsyncEnumerable{T}"/> streams.
/// </summary>
public static class JamfPagination
{
    /// <summary>
    /// Enumerates all items from a cursor-based connection (Protect-style <c>pageInfo.next</c>).
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="fetchPage">
    /// Fetches one page. Receives the opaque cursor (<see langword="null"/> for the first page)
    /// and returns the page items plus the next cursor (or <see langword="null"/> when finished).
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of items across all pages.</returns>
    public static async IAsyncEnumerable<T> EnumerateByCursorAsync<T>(
        Func<string?, CancellationToken, Task<(IReadOnlyList<T> Items, string? Next)>> fetchPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fetchPage);

        string? next = null;
        do
        {
            var (items, pageNext) = await fetchPage(next, cancellationToken).ConfigureAwait(false);
            if (items is { Count: > 0 })
            {
                foreach (var item in items)
                {
                    yield return item;
                }
            }

            next = pageNext;
        }
        while (!string.IsNullOrEmpty(next));
    }

    /// <summary>
    /// Enumerates all items from a zero-based page / page-size API (Jamf Pro inventory style).
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="fetchPage">
    /// Fetches one page. Receives zero-based page index and page size; returns page items and optional total count.
    /// </param>
    /// <param name="pageSize">Page size (must be greater than zero).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of items across all pages.</returns>
    public static async IAsyncEnumerable<T> EnumerateByPageAsync<T>(
        Func<int, int, CancellationToken, Task<(IReadOnlyList<T> Items, int? TotalCount)>> fetchPage,
        int pageSize = 100,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fetchPage);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        var page = 0;
        var yielded = 0;
        int? totalCount = null;

        while (true)
        {
            var (items, pageTotal) = await fetchPage(page, pageSize, cancellationToken).ConfigureAwait(false);
            totalCount ??= pageTotal;

            if (items is null || items.Count == 0)
            {
                yield break;
            }

            foreach (var item in items)
            {
                yield return item;
                yielded++;
            }

            if (items.Count < pageSize)
            {
                yield break;
            }

            if (totalCount is int total && yielded >= total)
            {
                yield break;
            }

            page++;
        }
    }
}
