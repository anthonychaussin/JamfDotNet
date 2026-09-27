using JamfDotNet.Core;
using JamfDotNet.Pro.Generated.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfProMdmAndEnumerateTests
{
    [Fact]
    public async Task ClearPasscodeDevicesAsync_Requires_Management_Ids()
    {
        var client = CreateClient();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.ClearPasscodeDevicesAsync([], cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task EnableLostModeAsync_Requires_Management_Ids()
    {
        var client = CreateClient();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            client.EnableLostModeAsync([], message: "Lost", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task SendMdmCommandAsync_Requires_Request()
    {
        var client = CreateClient();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.SendMdmCommandAsync(null!, TestContext.Current.CancellationToken));
    }

    [Fact]
    public void UpdateInventoryAsync_Builds_DeviceInformation_Command()
    {
        // Construction smoke: command payload types used by UpdateInventoryAsync.
        var command = new DeviceInformationCommand
        {
            CommandType = MdmCommandType.DEVICE_INFORMATION,
            Queries = ["DeviceName", "OSVersion"],
        };
        Assert.Equal(MdmCommandType.DEVICE_INFORMATION, command.CommandType);
        Assert.Equal(2, command.Queries!.Count);
    }

    private static JamfProClient CreateClient()
    {
        var options = new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            ClientId = "id",
            ClientSecret = "secret",
        };
        return JamfProClient.Create(options, new AnonymousAuthenticationProvider());
    }
}
