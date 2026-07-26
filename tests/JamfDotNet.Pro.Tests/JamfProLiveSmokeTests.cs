using JamfDotNet.Core;
using JamfDotNet.Pro;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Pro.Tests;

/// <summary>
/// Optional live smoke tests. Enabled only when JAMF_URL, JAMF_CLIENT_ID, and JAMF_CLIENT_SECRET are set.
/// </summary>
public sealed class JamfProLiveSmokeTests
{
    private static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_URL"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_CLIENT_ID"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_CLIENT_SECRET"));

    [Fact]
    public async Task Can_Authenticate_And_Read_Jamf_Pro_Version()
    {
        if (!IsConfigured)
        {
            return;
        }

        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddJamfProClient(options =>
        {
            options.BaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_URL")!);
            options.AuthenticationMode = JamfAuthenticationMode.ClientCredentials;
            options.ClientId = Environment.GetEnvironmentVariable("JAMF_CLIENT_ID");
            options.ClientSecret = Environment.GetEnvironmentVariable("JAMF_CLIENT_SECRET");
        });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfProClient>();

        var version = await client.Api.V1.JamfProVersion.GetAsync(cancellationToken: ct);
        Assert.NotNull(version);
        Assert.False(string.IsNullOrWhiteSpace(version!.Version));
    }
}
