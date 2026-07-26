using JamfDotNet.Core;
using JamfDotNet.TitleEditor;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.TitleEditor.Tests;

/// <summary>
/// Optional live smoke tests. Enabled only when JAMF_TITLE_EDITOR_URL,
/// JAMF_TITLE_EDITOR_USERNAME, and JAMF_TITLE_EDITOR_PASSWORD are set.
/// </summary>
public sealed class JamfTitleEditorLiveSmokeTests
{
    private static bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_TITLE_EDITOR_URL"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_TITLE_EDITOR_USERNAME"))
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JAMF_TITLE_EDITOR_PASSWORD"));

    [Fact]
    public async Task Can_List_Software_Titles()
    {
        if (!IsConfigured)
        {
            return;
        }

        var ct = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddJamfTitleEditorClient(o =>
        {
            o.BaseUrl = new Uri(Environment.GetEnvironmentVariable("JAMF_TITLE_EDITOR_URL")!);
            o.Username = Environment.GetEnvironmentVariable("JAMF_TITLE_EDITOR_USERNAME")!;
            o.Password = Environment.GetEnvironmentVariable("JAMF_TITLE_EDITOR_PASSWORD")!;
        });

        await using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfTitleEditorClient>();
        var titles = await client.Api.Softwaretitles.GetAsync(cancellationToken: ct);
        Assert.NotNull(titles);
    }
}
