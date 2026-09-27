using JamfDotNet.Core;
using JamfDotNet.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Platform.Tests;

/// <summary>
/// Optional live smoke tests. Enabled only when JAMF_PLATFORM_URL, JAMF_PLATFORM_CLIENT_ID,
/// JAMF_PLATFORM_CLIENT_SECRET, and JAMF_PLATFORM_TENANT_ID are set.
/// </summary>
public sealed class JamfPlatformLiveSmokeTests
{
    private static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PLATFORM_URL"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PLATFORM_CLIENT_ID"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PLATFORM_CLIENT_SECRET"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PLATFORM_TENANT_ID"));

    [Fact]
    public void Can_Resolve_Platform_Client()
    {
        if (!IsConfigured)
        {
            return;
        }

        var services = new ServiceCollection();
        services.AddJamfPlatformClient(o =>
        {
            o.GatewayBaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_PLATFORM_URL")!);
            o.ClientId = Environment.GetEnvironmentVariable("JAMF_PLATFORM_CLIENT_ID")!;
            o.ClientSecret = Environment.GetEnvironmentVariable("JAMF_PLATFORM_CLIENT_SECRET")!;
            o.TenantId = Environment.GetEnvironmentVariable("JAMF_PLATFORM_TENANT_ID")!;
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfPlatformClient>();
        Assert.Equal(Environment.GetEnvironmentVariable("JAMF_PLATFORM_TENANT_ID"), client.TenantId);
        Assert.NotNull(client.Blueprints);
        Assert.NotNull(client.Devices);
    }

    [Fact]
    public async Task Can_Enumerate_Tenant_Devices()
    {
        if (!IsConfigured)
        {
            return;
        }

        var services = new ServiceCollection();
        services.AddJamfPlatformClient(o =>
        {
            o.GatewayBaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_PLATFORM_URL")!);
            o.ClientId = Environment.GetEnvironmentVariable("JAMF_PLATFORM_CLIENT_ID")!;
            o.ClientSecret = Environment.GetEnvironmentVariable("JAMF_PLATFORM_CLIENT_SECRET")!;
            o.TenantId = Environment.GetEnvironmentVariable("JAMF_PLATFORM_TENANT_ID")!;
        });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfPlatformClient>();
        var count = 0;
        await foreach (var _ in client.EnumerateDevicesAsync(pageSize: 1, cancellationToken: TestContext.Current.CancellationToken))
        {
            count++;
            break;
        }

        Assert.True(count is 0 or 1);
    }
}
