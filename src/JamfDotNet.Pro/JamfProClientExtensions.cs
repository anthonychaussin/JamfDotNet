using JamfDotNet.Core;
using JamfDotNet.Pro.Generated.Models;
using Microsoft.Kiota.Abstractions;

namespace JamfDotNet.Pro;

/// <summary>
/// High-level helpers on top of the Kiota Jamf Pro surface.
/// </summary>
public static class JamfProClientExtensions
{
    /// <summary>
    /// Enumerates all computer inventory records from <c>/api/v4/computers-inventory</c>, following page / page-size pagination.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">
    /// Optional extra query configuration applied on every page (sections, sort, …).
    /// Page and page size are set by this helper after <paramref name="configure"/> runs.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of inventory records.</returns>
    public static IAsyncEnumerable<ComputerInventoryV4> EnumerateComputersInventoryAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V4.ComputersInventory.ComputersInventoryRequestBuilder.ComputersInventoryRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);

        return JamfPagination.EnumerateByPageAsync(
            async (page, size, ct) =>
            {
                Action<RequestConfiguration<Generated.V4.ComputersInventory.ComputersInventoryRequestBuilder.ComputersInventoryRequestBuilderGetQueryParameters>> pageConfigure = config =>
                {
                    configure?.Invoke(config);
                    config.QueryParameters.Page = page;
                    config.QueryParameters.PageSize = size;
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        config.QueryParameters.Filter = filter;
                    }
                };

                var results = await client.Api.V4.ComputersInventory.GetAsync(pageConfigure, ct).ConfigureAwait(false);

                IReadOnlyList<ComputerInventoryV4> items = results?.Results is { Count: > 0 } list
                    ? list
                    : Array.Empty<ComputerInventoryV4>();
                return (items, results?.TotalCount);
            },
            pageSize,
            cancellationToken);
    }
}
