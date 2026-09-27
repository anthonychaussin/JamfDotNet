using JamfDotNet.Core;
using JamfDotNet.Platform.Generated.Blueprints.Models;
using JamfDotNet.Platform.Generated.DeviceGroups.Models;
using JamfDotNet.Platform.Generated.Devices.Models;
using Microsoft.Kiota.Abstractions;

namespace JamfDotNet.Platform;

/// <summary>
/// High-level pagination helpers for Jamf Platform Gateway tenant-scoped list APIs.
/// </summary>
public static class JamfPlatformClientExtensions
{
    /// <summary>
    /// Enumerates devices for the configured tenant from the Platform devices inventory API.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of devices.</returns>
    public static IAsyncEnumerable<DeviceListReadRepresentationV1> EnumerateDevicesAsync(
        this JamfPlatformClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.Devices.V1.Tenant.Item.Devices.DevicesRequestBuilder.DevicesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        var devices = client.ForTenant().Devices.Devices;

        return JamfPagination.EnumerateByPageAsync(
            async (page, size, ct) =>
            {
                Action<RequestConfiguration<Generated.Devices.V1.Tenant.Item.Devices.DevicesRequestBuilder.DevicesRequestBuilderGetQueryParameters>> pageConfigure = config =>
                {
                    configure?.Invoke(config);
                    config.QueryParameters.Page = page;
                    config.QueryParameters.PageSize = size;
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        config.QueryParameters.Filter = filter;
                    }
                };

                var results = await devices.GetAsync(pageConfigure, ct).ConfigureAwait(false);
                IReadOnlyList<DeviceListReadRepresentationV1> items = results?.Results is { Count: > 0 } list
                    ? list
                    : Array.Empty<DeviceListReadRepresentationV1>();
                return (items, results?.TotalCount);
            },
            pageSize,
            cancellationToken);
    }

    /// <summary>
    /// Enumerates device groups for the configured tenant.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional filter expression.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of device groups.</returns>
    public static IAsyncEnumerable<DeviceGroupListReadRepresentationV1> EnumerateDeviceGroupsAsync(
        this JamfPlatformClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.DeviceGroups.V1.Tenant.Item.DeviceGroups.DeviceGroupsRequestBuilder.DeviceGroupsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        var groups = client.ForTenant().DeviceGroups.DeviceGroups;

        return JamfPagination.EnumerateByPageAsync(
            async (page, size, ct) =>
            {
                Action<RequestConfiguration<Generated.DeviceGroups.V1.Tenant.Item.DeviceGroups.DeviceGroupsRequestBuilder.DeviceGroupsRequestBuilderGetQueryParameters>> pageConfigure = config =>
                {
                    configure?.Invoke(config);
                    config.QueryParameters.Page = page;
                    config.QueryParameters.PageSize = size;
                    if (!string.IsNullOrWhiteSpace(filter))
                    {
                        config.QueryParameters.Filter = filter;
                    }
                };

                var results = await groups.GetAsync(pageConfigure, ct).ConfigureAwait(false);
                IReadOnlyList<DeviceGroupListReadRepresentationV1> items = results?.Results is { Count: > 0 } list
                    ? list
                    : Array.Empty<DeviceGroupListReadRepresentationV1>();
                return (items, results?.TotalCount);
            },
            pageSize,
            cancellationToken);
    }

    /// <summary>
    /// Enumerates blueprints for the configured tenant.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="search">Optional search string.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of blueprint overviews.</returns>
    public static IAsyncEnumerable<BlueprintOverview> EnumerateBlueprintsAsync(
        this JamfPlatformClient client,
        int pageSize = 100,
        string? search = null,
        Action<RequestConfiguration<Generated.Blueprints.V1.Tenant.Item.Blueprints.BlueprintsRequestBuilder.BlueprintsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        var blueprints = client.ForTenant().Blueprints.Blueprints;

        return JamfPagination.EnumerateByPageAsync(
            async (page, size, ct) =>
            {
                Action<RequestConfiguration<Generated.Blueprints.V1.Tenant.Item.Blueprints.BlueprintsRequestBuilder.BlueprintsRequestBuilderGetQueryParameters>> pageConfigure = config =>
                {
                    configure?.Invoke(config);
                    config.QueryParameters.Page = page;
                    config.QueryParameters.PageSize = size;
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        config.QueryParameters.Search = search;
                    }
                };

                var results = await blueprints.GetAsync(pageConfigure, ct).ConfigureAwait(false);
                IReadOnlyList<BlueprintOverview> items = results?.Results is { Count: > 0 } list
                    ? list
                    : Array.Empty<BlueprintOverview>();
                int? total = results?.TotalCount is { } longTotal && longTotal <= int.MaxValue
                    ? (int)longTotal
                    : results?.TotalCount is null ? null : int.MaxValue;
                return (items, total);
            },
            pageSize,
            cancellationToken);
    }
}
