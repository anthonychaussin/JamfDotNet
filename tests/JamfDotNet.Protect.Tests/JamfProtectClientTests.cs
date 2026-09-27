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

    [Fact]
    public async Task EnumerateRolesAsync_Follows_Cursor()
    {
        var mock = new MockHttpMessageHandler();
        var calls = 0;
        mock.When(HttpMethod.Post, "https://contoso.protect.jamfcloud.com/app")
            .Respond(async _ =>
            {
                calls++;
                if (calls == 1)
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"data":{"listRoles":{"items":[{"id":"1","name":"A"}],"pageInfo":{"next":"c1","total":2}}}}""", System.Text.Encoding.UTF8, "application/json"),
                    };
                }

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"data":{"listRoles":{"items":[{"id":"2","name":"B"}],"pageInfo":{"next":null,"total":2}}}}""", System.Text.Encoding.UTF8, "application/json"),
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

        var names = new List<string>();
        await foreach (var role in client.EnumerateRolesAsync(cancellationToken: TestContext.Current.CancellationToken))
        {
            names.Add(role.Name!);
        }

        Assert.Equal(["A", "B"], names);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task GetAlertAsync_And_UpdateAlertsAsync_Work()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "https://contoso.protect.jamfcloud.com/app")
            .Respond(async request =>
            {
                var body = await request.Content!.ReadAsStringAsync();
                if (body.Contains("getAlert", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"data":{"getAlert":{"uuid":"a1","status":"New","severity":"High"}}}""", System.Text.Encoding.UTF8, "application/json"),
                    };
                }

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"data":{"updateAlerts":{"items":[{"uuid":"a1","status":"Resolved"}],"pageInfo":{"total":1}}}}""", System.Text.Encoding.UTF8, "application/json"),
                };
            });

        using var http = mock.ToHttpClient();
        using var client = new JamfProtectClient(http, new Uri("https://contoso.protect.jamfcloud.com/app"));

        var alert = await client.GetAlertAsync("a1", TestContext.Current.CancellationToken);
        Assert.Equal("High", alert!.Severity);

        var updated = await client.UpdateAlertsAsync(["a1"], "Resolved", TestContext.Current.CancellationToken);
        Assert.Equal("Resolved", updated.Items![0].Status);
    }

    [Fact]
    public async Task UpdateGroupAsync_And_Counts_Deserialize()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Post, "https://contoso.protect.jamfcloud.com/app")
            .Respond(async request =>
            {
                var body = await request.Content!.ReadAsStringAsync();
                if (body.Contains("updateGroup", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"data":{"updateGroup":{"id":"g1","name":"Fleet-2"}}}""", System.Text.Encoding.UTF8, "application/json"),
                    };
                }

                if (body.Contains("getComputerCount", StringComparison.Ordinal))
                {
                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("""{"data":{"getComputerCount":{"computers":42}}}""", System.Text.Encoding.UTF8, "application/json"),
                    };
                }

                return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("""{"data":{"getCount":{"computers":42,"alerts":7,"alertsComputers":5,"insightsComputers":3}}}""", System.Text.Encoding.UTF8, "application/json"),
                };
            });

        using var http = mock.ToHttpClient();
        using var client = new JamfProtectClient(http, new Uri("https://contoso.protect.jamfcloud.com/app"));

        var group = await client.UpdateGroupAsync(
            "g1",
            new Models.ProtectGroupUpdateRequest { Name = "Fleet-2" },
            TestContext.Current.CancellationToken);
        Assert.Equal("Fleet-2", group!.Name);

        var computers = await client.GetComputerCountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(42, computers!.Computers);

        var counts = await client.GetCountAsync(TestContext.Current.CancellationToken);
        Assert.Equal(42, counts!.Computers);
        Assert.Equal(7, counts.Alerts);
    }
}
