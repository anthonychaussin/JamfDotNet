using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Protect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;

namespace JamfDotNet.Protect.Tests;

public sealed class JamfProtectClientTests
{
    [Fact]
    public void AddJamfProtectClient_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddJamfProtectClient(o =>
        {
            o.BaseUrl = new Uri("https://contoso.protect.jamfcloud.com");
            o.ClientId = "id";
            o.ClientSecret = "secret";
        });

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetRequiredService<JamfProtectClient>());
        Assert.NotNull(provider.GetRequiredService<JamfProtectTokenProvider>());
    }

    [Fact]
    public async Task ListRolesAsync_Deserializes_GraphQl_Connection()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "https://contoso.protect.jamfcloud.com/app")
            .Respond("application/json", """
                {
                  "data": {
                    "listRoles": {
                      "items": [ { "id": "1", "name": "Admin", "created": "2024-01-01T00:00:00Z", "updated": "2024-01-02T00:00:00Z" } ],
                      "pageInfo": { "next": null, "total": 1 }
                    }
                  }
                }
                """);

        var options = Options.Create(new JamfProtectOptions
        {
            BaseUrl = new Uri("https://contoso.protect.jamfcloud.com"),
            ClientId = "id",
            ClientSecret = "secret",
        });
        using var http = mock.ToHttpClient();
        using var client = JamfProtectClient.Create(options, http);

        var roles = await client.ListRolesAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(roles.Items);
        Assert.Single(roles.Items!);
        Assert.Equal("Admin", roles.Items![0].Name);
        Assert.Equal(1, roles.PageInfo?.Total);
    }

    [Fact]
    public async Task ListAlertsAsync_And_ListGroupsAsync_Deserializes()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "https://contoso.protect.jamfcloud.com/app")
            .Respond(async request =>
            {
                var body = await request.Content!.ReadAsStringAsync();
                if (body.Contains("listAlerts", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"data":{"listAlerts":{"items":[{"uuid":"a1","severity":"High"}],"pageInfo":{"total":1}}}}""", System.Text.Encoding.UTF8, "application/json"),
                    };
                }

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"data":{"listGroups":{"items":[{"id":"g1","name":"Fleet"}],"pageInfo":{"total":1}}}}""", System.Text.Encoding.UTF8, "application/json"),
                };
            });

        var options = new JamfProtectOptions
        {
            BaseUrl = new Uri("https://contoso.protect.jamfcloud.com"),
            ClientId = "id",
            ClientSecret = "secret",
        };
        using var http = mock.ToHttpClient();
        using var client = new JamfProtectClient(http, options.GraphQlEndpoint);

        var alerts = await client.ListAlertsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("High", alerts.Items![0].Severity);

        var groups = await client.ListGroupsAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Fleet", groups.Items![0].Name);
    }

    [Fact]
    public async Task ExecuteAsync_Returns_Raw_Document()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "https://contoso.protect.jamfcloud.com/app")
            .Respond("application/json", """{ "data": { "ping": "ok" } }""");

        var options = new JamfProtectOptions
        {
            BaseUrl = new Uri("https://contoso.protect.jamfcloud.com"),
            ClientId = "id",
            ClientSecret = "secret",
        };
        using var http = mock.ToHttpClient();
        using var client = new JamfProtectClient(http, options.GraphQlEndpoint);
        using var doc = await client.ExecuteAsync("{ ping }", cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("ok", doc.RootElement.GetProperty("data").GetProperty("ping").GetString());
    }
}
