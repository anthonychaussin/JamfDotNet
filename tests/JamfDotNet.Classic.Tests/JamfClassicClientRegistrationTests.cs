using JamfDotNet.Classic;
using JamfDotNet.Core;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Classic.Tests;

public sealed class JamfClassicClientRegistrationTests
{
    [Fact]
    public void AddJamfClassicClient_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddJamfClassicClient(options =>
        {
            options.BaseUrl = new Uri("https://example.jamfcloud.com");
            options.ClientId = "id";
            options.ClientSecret = "secret";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfClassicClient>();
        Assert.NotNull(client.Api);
        Assert.NotNull(client.Api.Buildings);
        Assert.NotNull(client.Api.Computers);
        Assert.Equal(
            "https://example.jamfcloud.com/JSSResource/",
            provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<JamfClientOptions>>().Value.ClassicApiBaseUrl.AbsoluteUri);
    }
}
