using JamfDotNet.Core;
using JamfDotNet.School;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.School.Tests;

/// <summary>
/// Optional live smoke tests. Enabled only when JAMF_SCHOOL_URL, JAMF_SCHOOL_NETWORK_ID,
/// and JAMF_SCHOOL_API_KEY are set.
/// </summary>
public sealed class JamfSchoolLiveSmokeTests
{
    private static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_SCHOOL_URL"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_SCHOOL_NETWORK_ID"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_SCHOOL_API_KEY"));

    [Fact]
    public async Task Can_List_Devices()
    {
        if (!IsConfigured)
        {
            return;
        }

        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddJamfSchoolClient(o =>
        {
            o.BaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_SCHOOL_URL")!);
            o.NetworkId = Environment.GetEnvironmentVariable("JAMF_SCHOOL_NETWORK_ID")!;
            o.ApiKey = Environment.GetEnvironmentVariable("JAMF_SCHOOL_API_KEY")!;
        });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfSchoolClient>();
        var devices = await client.Devices.ListAsync(ct);
        Assert.NotNull(devices);
    }
}
