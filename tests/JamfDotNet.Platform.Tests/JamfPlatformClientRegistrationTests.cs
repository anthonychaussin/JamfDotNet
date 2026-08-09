using JamfDotNet.Core;
using JamfDotNet.Platform;
using Microsoft.Extensions.DependencyInjection;

namespace JamfDotNet.Platform.Tests;

public sealed class JamfPlatformClientRegistrationTests
{
    [Fact]
    public void AddJamfPlatformClient_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddJamfPlatformClient(o =>
        {
            o.GatewayBaseUrl = new Uri("https://us.apigw.jamf.com");
            o.ClientId = "id";
            o.ClientSecret = "secret";
            o.TenantId = "11111111-1111-1111-1111-111111111111";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfPlatformClient>();
        Assert.Equal("11111111-1111-1111-1111-111111111111", client.TenantId);
        Assert.NotNull(client.Blueprints);
        Assert.NotNull(client.Devices);

        var tenant = client.ForTenant();
        Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), tenant.TenantId);
        Assert.NotNull(tenant.Devices);
        Assert.NotNull(tenant.Blueprints);
    }

    [Fact]
    public void PlatformOptions_Build_Service_Urls()
    {
        var options = new JamfPlatformOptions
        {
            GatewayBaseUrl = new Uri("https://eu.apigw.jamf.com"),
            ClientId = "id",
            ClientSecret = "secret",
            TenantId = "t",
        };
        options.Validate();
        Assert.Equal("https://eu.apigw.jamf.com/auth/token", options.TokenEndpoint.AbsoluteUri);
        Assert.Equal("https://eu.apigw.jamf.com/api/blueprints", options.GetServiceBaseUrl("api/blueprints").AbsoluteUri.TrimEnd('/'));
    }
}
