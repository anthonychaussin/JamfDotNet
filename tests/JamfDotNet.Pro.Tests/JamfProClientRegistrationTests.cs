using JamfDotNet.Core;
using JamfDotNet.Pro;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfProClientRegistrationTests
{
    [Fact]
    public void AddJamfProClient_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddJamfProClient(options =>
        {
            options.BaseUrl = new Uri("https://example.jamfcloud.com");
            options.ClientId = "id";
            options.ClientSecret = "secret";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfProClient>();
        Assert.NotNull(client.Api);
        Assert.NotNull(client.Api.V1);
    }
}
