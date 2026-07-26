using JamfDotNet.Core;
using JamfDotNet.Protect;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Protect.Tests;

/// <summary>
/// Optional live smoke tests. Enabled only when JAMF_PROTECT_URL, JAMF_PROTECT_CLIENT_ID,
/// and JAMF_PROTECT_CLIENT_SECRET are set.
/// </summary>
public sealed class JamfProtectLiveSmokeTests
{
    private static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PROTECT_URL"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PROTECT_CLIENT_ID"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_PROTECT_CLIENT_SECRET"));

    [Fact]
    public async Task Can_List_Roles()
    {
        if (!IsConfigured)
        {
            return;
        }

        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddJamfProtectClient(o =>
        {
            o.BaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_PROTECT_URL")!);
            o.ClientId = Environment.GetEnvironmentVariable("JAMF_PROTECT_CLIENT_ID")!;
            o.ClientSecret = Environment.GetEnvironmentVariable("JAMF_PROTECT_CLIENT_SECRET")!;
        });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfProtectClient>();
        var roles = await client.ListRolesAsync(pageSize: 1, cancellationToken: ct);
        Assert.NotNull(roles);
    }
}
