using System.Runtime.CompilerServices;
using JamfDotNet.Core;
using JamfDotNet.Protect.Models;

namespace JamfDotNet.Protect;

/// <summary>
/// Cursor pagination helpers for <see cref="JamfProtectClient"/> list operations.
/// </summary>
public static class JamfProtectClientPaginationExtensions
{
    /// <summary>Enumerates all roles across pages.</summary>
    public static IAsyncEnumerable<ProtectRole> EnumerateRolesAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListRolesAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all plans across pages.</summary>
    public static IAsyncEnumerable<ProtectPlan> EnumeratePlansAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListPlansAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all analytic sets across pages.</summary>
    public static IAsyncEnumerable<ProtectAnalyticSet> EnumerateAnalyticSetsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListAnalyticSetsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all computers across pages.</summary>
    public static IAsyncEnumerable<ProtectComputer> EnumerateComputersAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListComputersAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all USB control sets across pages.</summary>
    public static IAsyncEnumerable<ProtectUsbControlSet> EnumerateUsbControlSetsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListUsbControlSetsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all prevent lists across pages.</summary>
    public static IAsyncEnumerable<ProtectPreventList> EnumeratePreventListsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListPreventListsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all alerts across pages.</summary>
    public static IAsyncEnumerable<ProtectAlert> EnumerateAlertsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListAlertsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all groups across pages.</summary>
    public static IAsyncEnumerable<ProtectGroup> EnumerateGroupsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListGroupsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all API clients across pages.</summary>
    public static IAsyncEnumerable<ProtectApiClient> EnumerateApiClientsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListApiClientsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all action configs across pages.</summary>
    public static IAsyncEnumerable<ProtectActionConfig> EnumerateActionConfigsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListActionConfigsAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all exception sets across pages.</summary>
    public static IAsyncEnumerable<ProtectExceptionSet> EnumerateExceptionSetsAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListExceptionSetsAsync(pageSize, next, ct), cancellationToken);

    private static IAsyncEnumerable<T> EnumerateAsync<T>(
        JamfProtectClient client,
        Func<JamfProtectClient, string?, CancellationToken, Task<ProtectConnection<T>>> listAsync,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        return JamfPagination.EnumerateByCursorAsync(
            async (next, ct) =>
            {
                var page = await listAsync(client, next, ct).ConfigureAwait(false);
                IReadOnlyList<T> items = page.Items ?? Array.Empty<T>();
                return (items, page.PageInfo?.Next);
            },
            cancellationToken);
    }
}
