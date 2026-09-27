using System.Runtime.CompilerServices;
using JamfDotNet.School.Models;

namespace JamfDotNet.School;

/// <summary>
/// Convenience enumeration helpers over School list endpoints (single-page API responses).
/// </summary>
public static class JamfSchoolClientPaginationExtensions
{
    /// <summary>Enumerates devices from a single list response.</summary>
    public static IAsyncEnumerable<SchoolDevice> EnumerateDevicesAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Devices.ListAsync(ct),
            static r => r.Devices,
            cancellationToken);

    /// <summary>Enumerates device groups from a single list response.</summary>
    public static IAsyncEnumerable<SchoolDeviceGroup> EnumerateDeviceGroupsAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.DeviceGroups.ListAsync(ct),
            static r => r.DeviceGroups ?? r.Groups,
            cancellationToken);

    /// <summary>Enumerates users from a single list response.</summary>
    public static IAsyncEnumerable<SchoolUser> EnumerateUsersAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Users.ListAsync(ct),
            static r => r.Users,
            cancellationToken);

    /// <summary>Enumerates user groups from a single list response.</summary>
    public static IAsyncEnumerable<SchoolUserGroup> EnumerateUserGroupsAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Groups.ListAsync(ct),
            static r => r.Groups,
            cancellationToken);

    /// <summary>Enumerates classes from a single list response.</summary>
    public static IAsyncEnumerable<SchoolClass> EnumerateClassesAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Classes.ListAsync(ct),
            static r => r.Classes,
            cancellationToken);

    /// <summary>Enumerates profiles from a single list response.</summary>
    public static IAsyncEnumerable<SchoolProfile> EnumerateProfilesAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Profiles.ListAsync(ct),
            static r => r.Profiles,
            cancellationToken);

    /// <summary>Enumerates apps from a single list response.</summary>
    public static IAsyncEnumerable<SchoolApp> EnumerateAppsAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Apps.ListAsync(ct),
            static r => r.Apps,
            cancellationToken);

    /// <summary>Enumerates locations from a single list response.</summary>
    public static IAsyncEnumerable<SchoolLocation> EnumerateLocationsAsync(
        this JamfSchoolClient client,
        CancellationToken cancellationToken = default) =>
        EnumerateAsync(
            client,
            static (c, ct) => c.Locations.ListAsync(ct),
            static r => r.Locations,
            cancellationToken);

    private static async IAsyncEnumerable<TItem> EnumerateAsync<TResponse, TItem>(
        JamfSchoolClient client,
        Func<JamfSchoolClient, CancellationToken, Task<TResponse>> listAsync,
        Func<TResponse, IReadOnlyList<TItem>?> selectItems,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        var response = await listAsync(client, cancellationToken).ConfigureAwait(false);
        var items = selectItems(response);
        if (items is null)
        {
            yield break;
        }

        foreach (var item in items)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }
}
