using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfTokenProviderTests
{
    [Fact]
    public async Task GetAccessTokenAsync_Uses_OAuth_ClientCredentials()
    {
        var ct = TestContext.Current.CancellationToken;
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/oauth/token")
            .Respond("application/json", """{"access_token":"abc","expires_in":3600,"token_type":"Bearer"}""");

        var options = Options.Create(new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.ClientCredentials,
            ClientId = "client",
            ClientSecret = "secret",
        });

        var provider = new JamfTokenProvider(mock.ToHttpClient(), options);
        var token = await provider.GetAccessTokenAsync(ct);

        Assert.Equal("abc", token);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Caches_Token()
    {
        var ct = TestContext.Current.CancellationToken;
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/oauth/token")
            .Respond("application/json", """{"access_token":"abc","expires_in":3600,"token_type":"Bearer"}""");

        var options = Options.Create(new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.ClientCredentials,
            ClientId = "client",
            ClientSecret = "secret",
        });

        var provider = new JamfTokenProvider(mock.ToHttpClient(), options);
        var first = await provider.GetAccessTokenAsync(ct);
        var second = await provider.GetAccessTokenAsync(ct);

        Assert.Equal(first, second);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Uses_Basic_Token_Endpoint()
    {
        var ct = TestContext.Current.CancellationToken;
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/v1/auth/token")
            .Respond(HttpStatusCode.OK, JsonContent.Create(new
            {
                token = "basic-token",
                expires = DateTimeOffset.UtcNow.AddHours(1),
            }, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

        var options = Options.Create(new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.BasicToken,
            Username = "admin",
            Password = "pass",
        });

        var provider = new JamfTokenProvider(mock.ToHttpClient(), options);
        var token = await provider.GetAccessTokenAsync(ct);

        Assert.Equal("basic-token", token);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetAccessTokenAsync_Throws_JamfApiException_On_Failure()
    {
        var ct = TestContext.Current.CancellationToken;
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/oauth/token")
            .Respond(HttpStatusCode.Unauthorized, "application/json", """{"error":"invalid_client"}""");

        var options = Options.Create(new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.ClientCredentials,
            ClientId = "client",
            ClientSecret = "bad",
        });

        var provider = new JamfTokenProvider(mock.ToHttpClient(), options);
        var ex = await Assert.ThrowsAsync<JamfApiException>(
            async () => await provider.GetAccessTokenAsync(ct));
        Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
    }
}
