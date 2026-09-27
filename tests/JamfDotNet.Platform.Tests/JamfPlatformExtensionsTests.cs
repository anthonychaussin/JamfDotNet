using JamfDotNet.Core;
using JamfDotNet.Platform;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using RichardSzalay.MockHttp;

namespace JamfDotNet.Platform.Tests;

public sealed class JamfPlatformExtensionsTests
{
    private const string TenantId = "11111111-1111-1111-1111-111111111111";
    private static readonly Uri Gateway = new("https://us.apigw.jamf.com/");

    [Fact]
    public async Task EnumerateDevicesAsync_Follows_Pages()
    {
        var mock = new MockHttpMessageHandler();
        mock.When(HttpMethod.Get, $"https://us.apigw.jamf.com/api/devices/v1/tenant/{TenantId}/devices*")
            .Respond(async request =>
            {
                var uri = request.RequestUri!.Query;
                var page = uri.Contains("page=1", StringComparison.Ordinal) ? 1 : 0;
                if (page == 0)
                {
                    return JsonOk("""
                        {
                          "results": [ { "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "name": "Mac-1" } ],
                          "totalCount": 2,
                          "page": 0,
                          "pageSize": 1,
                          "totalPages": 2,
                          "hasNext": true,
                          "hasPrevious": false
                        }
                        """);
                }

                return JsonOk("""
                    {
                      "results": [ { "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "name": "Mac-2" } ],
                      "totalCount": 2,
                      "page": 1,
                      "pageSize": 1,
                      "totalPages": 2,
                      "hasNext": false,
                      "hasPrevious": true
                    }
                    """);
            });

        using var client = CreateClient(mock);
        var names = new List<string>();
        await foreach (var device in client.EnumerateDevicesAsync(pageSize: 1, cancellationToken: TestContext.Current.CancellationToken))
        {
            names.Add(device.Name!);
        }

        Assert.Equal(["Mac-1", "Mac-2"], names);
    }

    [Fact]
    public async Task RestartDeviceAsync_Posts_And_Deserializes()
    {
        var deviceId = "cccccccc-cccc-cccc-cccc-cccccccccccc";
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, $"https://us.apigw.jamf.com/api/device-actions/v1/tenant/{TenantId}/devices/{deviceId}/restart")
            .Respond("application/json", """
                [
                  {
                    "commandId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
                    "deviceId": "cccccccc-cccc-cccc-cccc-cccccccccccc"
                  }
                ]
                """);

        using var client = CreateClient(mock);
        var results = await client.RestartDeviceAsync(deviceId, TestContext.Current.CancellationToken);
        Assert.Single(results);
        Assert.Equal(Guid.Parse(deviceId), results[0].DeviceId);
        mock.VerifyNoOutstandingExpectation();
    }

    private static JamfPlatformClient CreateClient(MockHttpMessageHandler mock)
    {
        var options = new JamfPlatformOptions
        {
            GatewayBaseUrl = Gateway,
            ClientId = "id",
            ClientSecret = "secret",
            TenantId = TenantId,
        };
        options.Validate();
        var http = mock.ToHttpClient();
        var auth = new StaticBearerAuthProvider("test-token");
        return JamfPlatformClient.Create(options, auth, http);
    }

    private static HttpResponseMessage JsonOk(string json) =>
        new(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        };

    private sealed class StaticBearerAuthProvider : IAuthenticationProvider
    {
        private readonly string _token;

        public StaticBearerAuthProvider(string token) => _token = token;

        public Task AuthenticateRequestAsync(
            RequestInformation request,
            Dictionary<string, object>? additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
        {
            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", $"Bearer {_token}");
            return Task.CompletedTask;
        }
    }
}
