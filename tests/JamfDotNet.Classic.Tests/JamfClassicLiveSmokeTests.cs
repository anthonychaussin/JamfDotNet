using JamfDotNet.Classic;
using JamfDotNet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Classic.Tests;

/// <summary>
/// Optional live smoke tests. Enabled only when JAMF_URL, JAMF_CLIENT_ID, and JAMF_CLIENT_SECRET are set.
/// </summary>
public sealed class JamfClassicLiveSmokeTests
{
    private static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_URL"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_CLIENT_ID"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_CLIENT_SECRET"));

    [Fact]
    public async Task Can_List_Buildings()
    {
        if (!IsConfigured)
        {
            return;
        }

        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddJamfClassicClient(options =>
        {
            options.BaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_URL")!);
            options.AuthenticationMode = JamfAuthenticationMode.ClientCredentials;
            options.ClientId = Environment.GetEnvironmentVariable("JAMF_CLIENT_ID");
            options.ClientSecret = Environment.GetEnvironmentVariable("JAMF_CLIENT_SECRET");
        });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfClassicClient>();

        var buildings = await client.Api.Buildings.GetAsync(cancellationToken: ct);
        Assert.NotNull(buildings);
    }
}
