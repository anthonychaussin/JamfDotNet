using JamfDotNet.Pro.Generated.Models;

namespace JamfDotNet.Pro;

/// <summary>
/// High-level MDM command helpers on top of Jamf Pro <c>/api/v2/mdm</c>.
/// </summary>
public static class JamfProMdmExtensions
{
    /// <summary>
    /// Sends an MDM restart-device command to the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> RestartDevicesAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                RestartDeviceCommand = new RestartDeviceCommand
                {
                    CommandType = MdmCommandType.RESTART_DEVICE,
                },
            },
            cancellationToken);

    /// <summary>
    /// Sends an MDM device-lock command to the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="pin">Optional lock PIN.</param>
    /// <param name="message">Optional lock screen message.</param>
    /// <param name="phoneNumber">Optional contact phone number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> LockDevicesAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        string? pin = null,
        string? message = null,
        string? phoneNumber = null,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                DeviceLockCommand = new DeviceLockCommand
                {
                    CommandType = MdmCommandType.DEVICE_LOCK,
                    Pin = pin,
                    Message = message,
                    PhoneNumber = phoneNumber,
                },
            },
            cancellationToken);

    /// <summary>
    /// Sends an MDM erase-device command to the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="pin">Optional erase PIN (macOS Find My).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> EraseDevicesAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        string? pin = null,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                EraseDeviceCommand = new EraseDeviceCommand
                {
                    CommandType = MdmCommandType.ERASE_DEVICE,
                    Pin = pin,
                },
            },
            cancellationToken);

    /// <summary>
    /// Sends an MDM shut-down-device command to the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> ShutDownDevicesAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                ShutDownDeviceCommand = new ShutDownDeviceCommand
                {
                    CommandType = MdmCommandType.SHUT_DOWN_DEVICE,
                },
            },
            cancellationToken);

    /// <summary>
    /// Sends an MDM clear-passcode command to the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="unlockToken">Optional unlock token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> ClearPasscodeDevicesAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        string? unlockToken = null,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                ClearPasscodeCommand = new ClearPasscodeCommand
                {
                    CommandType = MdmCommandType.CLEAR_PASSCODE,
                    UnlockToken = unlockToken,
                },
            },
            cancellationToken);

    /// <summary>
    /// Enables lost mode on supervised devices.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="message">Optional lock-screen message (at least message or phone required).</param>
    /// <param name="phone">Optional phone number displayed on the lock screen.</param>
    /// <param name="footnote">Optional footnote text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> EnableLostModeAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        string? message = null,
        string? phone = null,
        string? footnote = null,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                EnableLostModeCommand = new EnableLostModeCommand
                {
                    CommandType = MdmCommandType.ENABLE_LOST_MODE,
                    LostModeMessage = message,
                    LostModePhone = phone,
                    LostModeFootnote = footnote,
                },
            },
            cancellationToken);

    /// <summary>
    /// Disables lost mode on the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> DisableLostModeAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                DisableLostModeCommand = new DisableLostModeCommand
                {
                    CommandType = MdmCommandType.DISABLE_LOST_MODE,
                },
            },
            cancellationToken);

    /// <summary>
    /// Requests an inventory refresh via MDM <c>DEVICE_INFORMATION</c> (closest Pro equivalent to School update-inventory).
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="queries">Optional DEVICE_INFORMATION query keys; when null, the server default set is used.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static Task<IReadOnlyList<HrefResponse>> UpdateInventoryAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        IEnumerable<string>? queries = null,
        CancellationToken cancellationToken = default) =>
        SendCommandAsync(
            client,
            managementIds,
            new MdmCommandRequest.MdmCommandRequest_commandData
            {
                DeviceInformationCommand = new DeviceInformationCommand
                {
                    CommandType = MdmCommandType.DEVICE_INFORMATION,
                    Queries = queries?.ToList(),
                },
            },
            cancellationToken);

    /// <summary>
    /// Sends a blank push (check-in nudge) to the given management IDs.
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="managementIds">Client management IDs.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Blank push response.</returns>
    public static async Task<BlankPushResponse?> BlankPushAsync(
        this JamfProClient client,
        IEnumerable<string> managementIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        var ids = NormalizeManagementIds(managementIds);
        return await client.Api.V2.Mdm.BlankPush.PostAsync(
            new BlankPushRequest { ClientManagementIds = ids.ToList() },
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a raw MDM command request (escape hatch for advanced command payloads).
    /// </summary>
    /// <param name="client">Jamf Pro client.</param>
    /// <param name="request">Fully constructed MDM command request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Href responses for queued commands.</returns>
    public static async Task<IReadOnlyList<HrefResponse>> SendMdmCommandAsync(
        this JamfProClient client,
        MdmCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(request);
        var results = await client.Api.V2.Mdm.Commands.PostAsync(request, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return results is { Count: > 0 } list ? list : Array.Empty<HrefResponse>();
    }

    private static async Task<IReadOnlyList<HrefResponse>> SendCommandAsync(
        JamfProClient client,
        IEnumerable<string> managementIds,
        MdmCommandRequest.MdmCommandRequest_commandData commandData,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        var ids = NormalizeManagementIds(managementIds);
        var request = new MdmCommandRequest
        {
            ClientData = ids.Select(id => new MdmCommandClientRequest { ManagementId = id }).ToList(),
            CommandData = commandData,
        };
        return await client.SendMdmCommandAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static List<string> NormalizeManagementIds(IEnumerable<string> managementIds)
    {
        ArgumentNullException.ThrowIfNull(managementIds);
        var ids = managementIds
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ids.Count == 0)
        {
            throw new ArgumentException("At least one management id is required.", nameof(managementIds));
        }

        return ids;
    }
}
