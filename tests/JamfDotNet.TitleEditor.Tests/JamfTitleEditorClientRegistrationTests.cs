using JamfDotNet.Core;
using JamfDotNet.TitleEditor;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.TitleEditor.Tests;

public sealed class JamfTitleEditorClientRegistrationTests
{
    [Fact]
    public void AddJamfTitleEditorClient_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddJamfTitleEditorClient(o =>
        {
            o.BaseUrl = new Uri("https://example.appcatalog.jamfcloud.com");
            o.Username = "user";
            o.Password = "pass";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfTitleEditorClient>();
        Assert.NotNull(client.Api);
        Assert.NotNull(client.Api.Softwaretitles);
        Assert.NotNull(client.Api.Sources);
        Assert.NotNull(client.Api.Users);
    }

    [Fact]
    public void TitleEditorOptions_Build_Endpoints()
    {
        var options = new JamfTitleEditorOptions
        {
            BaseUrl = new Uri("https://example.appcatalog.jamfcloud.com"),
            Username = "user",
            Password = "pass",
        };
        options.Validate();
        Assert.Equal("https://example.appcatalog.jamfcloud.com/v2/auth/token", options.TokenEndpoint.AbsoluteUri);
        Assert.Equal("https://example.appcatalog.jamfcloud.com/v2/auth/keepalive", options.KeepAliveEndpoint.AbsoluteUri);
    }
}
