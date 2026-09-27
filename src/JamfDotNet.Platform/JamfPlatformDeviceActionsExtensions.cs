using JamfDotNet.Platform.Generated.DeviceActions.Models;

namespace JamfDotNet.Platform;

/// <summary>
/// Tenant-scoped device action helpers for the Platform API Gateway.
/// </summary>
public static class JamfPlatformDeviceActionsExtensions
{
    /// <summary>
    /// Requests a device restart via Platform device-actions.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="deviceId">Platform device id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Command responses when returned by the API.</returns>
    public static async Task<IReadOnlyList<DeviceCommandResponse>> RestartDeviceAsync(
        this JamfPlatformClient client,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var results = await client.ForTenant().DeviceActions.Devices[deviceId].Restart
            .PostAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return results is { Count: > 0 } list ? list : Array.Empty<DeviceCommandResponse>();
    }

    /// <summary>
    /// Requests a device shutdown via Platform device-actions.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="deviceId">Platform device id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Command responses when returned by the API.</returns>
    public static async Task<IReadOnlyList<DeviceCommandResponse>> ShutDownDeviceAsync(
        this JamfPlatformClient client,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var results = await client.ForTenant().DeviceActions.Devices[deviceId].Shutdown
            .PostAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return results is { Count: > 0 } list ? list : Array.Empty<DeviceCommandResponse>();
    }

    /// <summary>
    /// Requests a device erase via Platform device-actions.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="deviceId">Platform device id.</param>
    /// <param name="request">Optional erase options (PIN, data plan, …).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Command responses when returned by the API.</returns>
    public static async Task<IReadOnlyList<DeviceCommandResponse>> EraseDeviceAsync(
        this JamfPlatformClient client,
        string deviceId,
        EraseDeviceRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        var results = await client.ForTenant().DeviceActions.Devices[deviceId].Erase
            .PostAsync(request ?? new EraseDeviceRequest(), cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return results is { Count: > 0 } list ? list : Array.Empty<DeviceCommandResponse>();
    }

    /// <summary>
    /// Requests a device check-in (blank push) via Platform device-actions.
    /// </summary>
    /// <param name="client">Platform client.</param>
    /// <param name="deviceId">Platform device id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static Task CheckInDeviceAsync(
        this JamfPlatformClient client,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        return client.ForTenant().DeviceActions.Devices[deviceId].CheckIn
            .PostAsync(cancellationToken: cancellationToken);
    }
}
