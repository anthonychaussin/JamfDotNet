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
    /// <param name="filter">Optional RSQL filter (see <see cref="JamfRsql"/>).</param>
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
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V4.ComputersInventory.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates mobile device summaries from <c>/api/v2/mobile-devices</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of mobile devices.</returns>
    public static IAsyncEnumerable<MobileDeviceV2> EnumerateMobileDevicesAsync(
        this JamfProClient client,
        int pageSize = 100,
        Action<RequestConfiguration<Generated.V2.MobileDevices.MobileDevicesRequestBuilder.MobileDevicesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V2.MobileDevices.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates detailed mobile device inventory from <c>/api/v2/mobile-devices/detail</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of mobile device inventory records.</returns>
    public static IAsyncEnumerable<MobileDeviceResponse> EnumerateMobileDevicesDetailAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V2.MobileDevices.Detail.DetailRequestBuilder.DetailRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V2.MobileDevices.Detail.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates scripts from <c>/api/v1/scripts</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of scripts.</returns>
    public static IAsyncEnumerable<Script> EnumerateScriptsAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.Scripts.ScriptsRequestBuilder.ScriptsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.Scripts.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates packages from <c>/api/v1/packages</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of packages.</returns>
    public static IAsyncEnumerable<Package> EnumeratePackagesAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.Packages.PackagesRequestBuilder.PackagesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.Packages.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates users from <c>/api/v1/users</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of users.</returns>
    public static IAsyncEnumerable<User> EnumerateUsersAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.Users.UsersRequestBuilder.UsersRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.Users.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates categories from <c>/api/v1/categories</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of categories.</returns>
    public static IAsyncEnumerable<Category> EnumerateCategoriesAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.Categories.CategoriesRequestBuilder.CategoriesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.Categories.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates patch policies from <c>/api/v2/patch-policies</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of patch policies.</returns>
    public static IAsyncEnumerable<PatchPolicyListView> EnumeratePatchPoliciesAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V2.PatchPolicies.PatchPoliciesRequestBuilder.PatchPoliciesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V2.PatchPolicies.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates computer groups from <c>/api/v1/computer-groups</c> (single-page API response).
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of computer groups.</returns>
    public static async IAsyncEnumerable<ComputerGroup> EnumerateComputerGroupsAsync(
        this JamfProClient client,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        var groups = await client.Api.V1.ComputerGroups.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (groups is null)
        {
            yield break;
        }

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return group;
        }
    }

    /// <summary>
    /// Enumerates mobile device groups from <c>/api/v1/mobile-device-groups</c> (single-page API response).
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of mobile device groups.</returns>
    public static async IAsyncEnumerable<MobileDeviceGroup> EnumerateMobileDeviceGroupsAsync(
        this JamfProClient client,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
#pragma warning disable CS0618 // Jamf still exposes the list endpoint; marked obsolete in OpenAPI.
        var groups = await client.Api.V1.MobileDeviceGroups.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
#pragma warning restore CS0618
        if (groups is null)
        {
            yield break;
        }

        foreach (var group in groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return group;
        }
    }

    /// <summary>
    /// Enumerates buildings from <c>/api/v1/buildings</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of buildings.</returns>
    public static IAsyncEnumerable<Building> EnumerateBuildingsAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.Buildings.BuildingsRequestBuilder.BuildingsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.Buildings.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates departments from <c>/api/v1/departments</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of departments.</returns>
    public static IAsyncEnumerable<Department> EnumerateDepartmentsAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.Departments.DepartmentsRequestBuilder.DepartmentsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.Departments.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates computer extension attributes from <c>/api/v1/computer-extension-attributes</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of computer extension attributes.</returns>
    public static IAsyncEnumerable<ComputerExtensionAttributes> EnumerateComputerExtensionAttributesAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.ComputerExtensionAttributes.ComputerExtensionAttributesRequestBuilder.ComputerExtensionAttributesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.ComputerExtensionAttributes.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates mobile device extension attributes from <c>/api/v1/mobile-device-extension-attributes</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of mobile device extension attributes.</returns>
    public static IAsyncEnumerable<MobileDeviceExtensionAttributes> EnumerateMobileDeviceExtensionAttributesAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V1.MobileDeviceExtensionAttributes.MobileDeviceExtensionAttributesRequestBuilder.MobileDeviceExtensionAttributesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V1.MobileDeviceExtensionAttributes.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates computer prestages from <c>/api/v3/computer-prestages</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of computer prestages.</returns>
    public static IAsyncEnumerable<GetComputerPrestageV3> EnumerateComputerPrestagesAsync(
        this JamfProClient client,
        int pageSize = 100,
        Action<RequestConfiguration<Generated.V3.ComputerPrestages.ComputerPrestagesRequestBuilder.ComputerPrestagesRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V3.ComputerPrestages.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates smart computer groups from <c>/api/v3/computer-groups/smart-groups</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of smart computer group summaries.</returns>
    public static IAsyncEnumerable<SmartComputerGroupSearch> EnumerateSmartComputerGroupsAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V3.ComputerGroups.SmartGroups.SmartGroupsRequestBuilder.SmartGroupsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V3.ComputerGroups.SmartGroups.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates static computer groups from <c>/api/v3/computer-groups/static-groups</c>.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="pageSize">Page size (default 100).</param>
    /// <param name="filter">Optional RSQL filter.</param>
    /// <param name="configure">Optional per-page query configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of static computer group summaries.</returns>
    public static IAsyncEnumerable<StaticComputerGroupSummary> EnumerateStaticComputerGroupsAsync(
        this JamfProClient client,
        int pageSize = 100,
        string? filter = null,
        Action<RequestConfiguration<Generated.V3.ComputerGroups.StaticGroups.StaticGroupsRequestBuilder.StaticGroupsRequestBuilderGetQueryParameters>>? configure = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return EnumeratePagesAsync(
            pageSize,
            async (page, size, ct) =>
            {
                var results = await client.Api.V3.ComputerGroups.StaticGroups.GetAsync(
                    PageConfigure(configure, page, size, static (q, p, s) => { q.Page = p; q.PageSize = s; }, filter, static (q, f) => q.Filter = f),
                    ct).ConfigureAwait(false);
                return Page(results?.Results, results?.TotalCount);
            },
            cancellationToken);
    }

    /// <summary>
    /// Enumerates sites from <c>/api/v1/sites</c> (single-page API response).
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of sites.</returns>
    public static async IAsyncEnumerable<V1Site> EnumerateSitesAsync(
        this JamfProClient client,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        var sites = await client.Api.V1.Sites.GetAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        if (sites is null)
        {
            yield break;
        }

        foreach (var site in sites)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return site;
        }
    }

    private static IAsyncEnumerable<T> EnumeratePagesAsync<T>(
        int pageSize,
        Func<int, int, CancellationToken, Task<(IReadOnlyList<T> Items, int? TotalCount)>> fetchPage,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(pageSize, 0);
        return JamfPagination.EnumerateByPageAsync(fetchPage, pageSize, cancellationToken);
    }

    private static Action<RequestConfiguration<TQuery>> PageConfigure<TQuery>(
        Action<RequestConfiguration<TQuery>>? configure,
        int page,
        int size,
        Action<TQuery, int, int> setPageAndSize,
        string? filter = null,
        Action<TQuery, string>? setFilter = null)
        where TQuery : class, new() =>
        config =>
        {
            configure?.Invoke(config);
            setPageAndSize(config.QueryParameters, page, size);
            if (!string.IsNullOrWhiteSpace(filter) && setFilter is not null)
            {
                setFilter(config.QueryParameters, filter);
            }
        };

    private static (IReadOnlyList<T> Items, int? TotalCount) Page<T>(IList<T>? results, int? totalCount) =>
        (results is { Count: > 0 } list ? (IReadOnlyList<T>)list : Array.Empty<T>(), totalCount);

    private static (IReadOnlyList<T> Items, int? TotalCount) Page<T>(IList<T>? results, long? totalCount) =>
        (results is { Count: > 0 } list ? (IReadOnlyList<T>)list : Array.Empty<T>(), ClampTotalCount(totalCount));

    private static int? ClampTotalCount(long? totalCount) =>
        totalCount switch
        {
            null => null,
            <= int.MaxValue => (int)totalCount.Value,
            _ => int.MaxValue,
        };
}
