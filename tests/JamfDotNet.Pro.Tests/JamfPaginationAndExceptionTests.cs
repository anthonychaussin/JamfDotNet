using JamfDotNet.Core;
using Microsoft.Kiota.Abstractions;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfPaginationAndExceptionTests
{
    [Fact]
    public async Task EnumerateByPageAsync_Yields_All_Pages()
    {
        var pages = new Dictionary<int, (IReadOnlyList<int> Items, int? Total)>
        {
            [0] = ([1, 2], 3),
            [1] = ([3], 3),
        };

        var items = new List<int>();
        await foreach (var item in JamfPagination.EnumerateByPageAsync(
                           (page, size, _) =>
                           {
                               Assert.Equal(2, size);
                               return Task.FromResult(pages[page]);
                           },
                           pageSize: 2,
                           cancellationToken: TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        Assert.Equal([1, 2, 3], items);
    }

    [Fact]
    public async Task EnumerateByCursorAsync_Stops_When_Next_Is_Null()
    {
        var calls = 0;
        var items = new List<string>();
        await foreach (var item in JamfPagination.EnumerateByCursorAsync(
                           (next, _) =>
                           {
                               calls++;
                               if (next is null)
                               {
                                   return Task.FromResult<(IReadOnlyList<string>, string?)>(
                                       (["a", "b"], "cursor-1"));
                               }

                               return Task.FromResult<(IReadOnlyList<string>, string?)>(
                                   (["c"], null));
                           },
                           TestContext.Current.CancellationToken))
        {
            items.Add(item);
        }

        Assert.Equal(2, calls);
        Assert.Equal(["a", "b", "c"], items);
    }

    [Fact]
    public void FromApiException_Maps_Status_Code()
    {
        var api = new ApiException("boom") { ResponseStatusCode = 404 };
        var jamf = JamfApiException.FromApiException(api);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, jamf.StatusCode);
        Assert.Same(api, jamf.InnerException);
    }
}
