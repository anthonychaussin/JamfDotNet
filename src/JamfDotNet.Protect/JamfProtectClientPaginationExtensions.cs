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
        ProtectComputerFilters? filter = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListComputersAsync(pageSize, next, filter, ct), cancellationToken);

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
        ProtectAlertFilters? filter = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListAlertsAsync(pageSize, next, filter, ct), cancellationToken);

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

    /// <summary>Enumerates all users across pages.</summary>
    public static IAsyncEnumerable<ProtectUser> EnumerateUsersAsync(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListUsersAsync(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates all telemetry v2 configs across pages.</summary>
    public static IAsyncEnumerable<ProtectTelemetryV2> EnumerateTelemetriesV2Async(
        this JamfProtectClient client,
        int? pageSize = null,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(client, (c, next, ct) => c.ListTelemetriesV2Async(pageSize, next, ct), cancellationToken);

    /// <summary>Enumerates audit logs for a date range across pages.</summary>
    /// <param name="client">Protect client.</param>
    /// <param name="startDate">Inclusive start (UTC).</param>
    /// <param name="endDate">Inclusive end (UTC).</param>
    /// <param name="pageSize">Optional page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async stream of audit log entries.</returns>
    public static IAsyncEnumerable<ProtectAuditLog> EnumerateAuditLogsByDateAsync(
        this JamfProtectClient client,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        int? pageSize = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        return JamfPagination.EnumerateByCursorAsync(
            async (next, ct) =>
            {
                var page = await client.ListAuditLogsByDateAsync(startDate, endDate, pageSize, next, ct).ConfigureAwait(false);
                IReadOnlyList<ProtectAuditLog> items = page.Items ?? Array.Empty<ProtectAuditLog>();
                return (items, page.PageInfo?.Next);
            },
            cancellationToken);
    }

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
