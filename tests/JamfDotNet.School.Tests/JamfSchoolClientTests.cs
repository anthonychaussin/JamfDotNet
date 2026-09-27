using System.Text;
using JamfDotNet.Core;
using JamfDotNet.School;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;

namespace JamfDotNet.School.Tests;

public sealed class JamfSchoolClientTests
{
    [Fact]
    public void AddJamfSchoolClient_Registers_Client()
    {
        var services = new ServiceCollection();
        services.AddJamfSchoolClient(o =>
        {
            o.BaseUrl = new Uri("https://contoso.jamfcloud.com");
            o.NetworkId = "network";
            o.ApiKey = "key";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<JamfSchoolClient>();
        Assert.NotNull(client.Devices);
        Assert.NotNull(client.Users);
        Assert.NotNull(client.Classes);
    }

    [Fact]
    public async Task Devices_ListAsync_Deserializes_Typed_Response()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, "https://contoso.jamfcloud.com/api/devices")
            .With(req =>
            {
                Assert.Equal("Basic", req.Headers.Authorization?.Scheme);
                var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("network:key"));
                Assert.Equal(expected, req.Headers.Authorization?.Parameter);
                Assert.True(req.Headers.TryGetValues("X-Server-Protocol-Version", out var values));
                Assert.Equal("3", values.Single());
                return true;
            })
            .Respond("application/json", """{ "code": 200, "devices": [ { "UDID": "abc", "name": "iPad", "serialNumber": "SN1" } ] }""");

        var options = new JamfSchoolOptions
        {
            BaseUrl = new Uri("https://contoso.jamfcloud.com"),
            NetworkId = "network",
            ApiKey = "key",
        };
        using var http = mock.ToHttpClient();
        using var client = JamfSchoolClient.Create(options, http);
        var response = await client.Devices.ListAsync(TestContext.Current.CancellationToken);
        Assert.Equal(200, response.Code);
        Assert.NotNull(response.Devices);
        Assert.Single(response.Devices!);
        Assert.Equal("abc", response.Devices![0].Udid);
        Assert.Equal("iPad", response.Devices[0].Name);
    }

    [Fact]
    public async Task Devices_RestartAsync_Posts_Command()
    {
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://contoso.jamfcloud.com/api/devices/abc/restart")
            .Respond(System.Net.HttpStatusCode.NoContent);

        var options = new JamfSchoolOptions
        {
            BaseUrl = new Uri("https://contoso.jamfcloud.com"),
            NetworkId = "network",
            ApiKey = "key",
        };
        using var http = mock.ToHttpClient();
        using var client = JamfSchoolClient.Create(options, http);
        await client.Devices.RestartAsync("abc", TestContext.Current.CancellationToken);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Devices_ShutdownAsync_Posts_Command()
    {
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://contoso.jamfcloud.com/api/devices/abc/shutdown")
            .Respond(System.Net.HttpStatusCode.NoContent);

        var options = new JamfSchoolOptions
        {
            BaseUrl = new Uri("https://contoso.jamfcloud.com"),
            NetworkId = "network",
            ApiKey = "key",
        };
        using var http = mock.ToHttpClient();
        using var client = JamfSchoolClient.Create(options, http);
        await client.Devices.ShutdownAsync("abc", TestContext.Current.CancellationToken);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Users_CreateAsync_Posts_Json()
    {
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://contoso.jamfcloud.com/api/users")
            .Respond("application/json", """{ "code": 200, "user": { "id": 9, "username": "student1" } }""");

        var options = new JamfSchoolOptions
        {
            BaseUrl = new Uri("https://contoso.jamfcloud.com"),
            NetworkId = "network",
            ApiKey = "key",
        };
        using var http = mock.ToHttpClient();
        using var client = JamfSchoolClient.Create(options, http);
        var created = await client.Users.CreateAsync(
            new Models.SchoolUserWriteRequest { Username = "student1" },
            TestContext.Current.CancellationToken);
        Assert.Equal(9, created.User!.Id);
        Assert.Equal("student1", created.User.Username);
        mock.VerifyNoOutstandingExpectation();
    }
}
