using System.Net;
using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Core.Kiota;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using RichardSzalay.MockHttp;

namespace JamfDotNet.Core.Tests;

public sealed class JamfRsqlTests
{
    [Fact]
    public void Eq_And_Quotes_Strings()
    {
        Assert.Equal("name==\"Apps*\"", JamfRsql.Eq("name", "Apps*"));
        Assert.Equal("priority==5", JamfRsql.Eq("priority", 5));
        Assert.Equal("enabled==true", JamfRsql.Eq("enabled", true));
    }

    [Fact]
    public void And_Or_Combine_Clauses()
    {
        var filter = JamfRsql.And(
            JamfRsql.Eq("name", "Admin"),
            JamfRsql.Eq("siteId", -1));
        Assert.Equal("name==\"Admin\" and siteId==-1", filter);

        var or = JamfRsql.Or(JamfRsql.Eq("a", 1), JamfRsql.Eq("b", 2));
        Assert.Equal("a==1 or b==2", or);
    }

    [Fact]
    public void In_Formats_List()
    {
        Assert.Equal("id=in=(\"a\",\"b\")", JamfRsql.In("id", "a", "b"));
    }
}

public sealed class JamfApiExceptionTests
{
    [Fact]
    public void FromApiException_Maps_Status_And_Data_Body()
    {
        var api = new ApiException("boom") { ResponseStatusCode = 409 };
        api.Data["responseBody"] = "{\"error\":\"conflict\"}";

        var jamf = JamfApiException.FromApiException(api);
        Assert.Equal(HttpStatusCode.Conflict, jamf.StatusCode);
        Assert.Equal("{\"error\":\"conflict\"}", jamf.ResponseBody);
        Assert.Same(api, jamf.InnerException);
    }

    [Fact]
    public void FromApiException_Uses_Explicit_Body_Overload()
    {
        var api = new ApiException("boom") { ResponseStatusCode = 400 };
        var jamf = JamfApiException.FromApiException(api, "{\"message\":\"bad\"}");
        Assert.Equal("{\"message\":\"bad\"}", jamf.ResponseBody);
        Assert.Equal(["bad"], jamf.Errors);
    }

    [Fact]
    public void FromGraphQlErrors_Parses_Messages()
    {
        var jamf = JamfApiException.FromGraphQlErrors(
            """{"errors":[{"message":"not found"},{"message":"denied"}]}""",
            HttpStatusCode.OK);
        Assert.Equal("not found; denied", jamf.Message);
        Assert.Equal(["not found", "denied"], jamf.Errors);
    }

    [Fact]
    public void FromHttpResponse_Captures_RetryAfter()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
        response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromSeconds(12));
        response.Headers.TryAddWithoutValidation("X-RateLimit-Limit", "100");
        response.Headers.TryAddWithoutValidation("X-RateLimit-Remaining", "0");

        var jamf = JamfApiException.FromHttpResponse("rate limited", response, """{"message":"slow down"}""");
        Assert.Equal(TimeSpan.FromSeconds(12), jamf.RetryAfter);
        Assert.Equal("100", jamf.RateLimitLimit);
        Assert.Equal("0", jamf.RateLimitRemaining);
        Assert.Equal(["slow down"], jamf.Errors);
    }
}

public sealed class JamfRemappingRequestAdapterTests
{
    [Fact]
    public void MaybeWrap_Returns_Inner_When_Disabled()
    {
        var inner = new ThrowingRequestAdapter();
        Assert.Same(inner, JamfRemappingRequestAdapter.MaybeWrap(inner, remapKiotaExceptions: false));
    }

    [Fact]
    public async Task SendNoContentAsync_Remaps_ApiException_To_JamfApiException()
    {
        var adapter = new JamfRemappingRequestAdapter(new ThrowingRequestAdapter());
        var ex = await Assert.ThrowsAsync<JamfApiException>(() =>
            adapter.SendNoContentAsync(new RequestInformation(), cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
        Assert.IsType<ApiException>(ex.InnerException);
    }

    private sealed class ThrowingRequestAdapter : IRequestAdapter
    {
        public string? BaseUrl { get; set; }
        public ISerializationWriterFactory SerializationWriterFactory =>
            throw new NotImplementedException();

        public void EnableBackingStore(IBackingStoreFactory backingStoreFactory) =>
            throw new NotImplementedException();

        public Task<T?> ConvertToNativeRequestAsync<T>(RequestInformation requestInfo, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<ModelType?> SendAsync<ModelType>(
            RequestInformation requestInfo,
            ParsableFactory<ModelType> factory,
            Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
            CancellationToken cancellationToken = default)
            where ModelType : IParsable =>
            throw new NotImplementedException();

        public Task<IEnumerable<ModelType>?> SendCollectionAsync<ModelType>(
            RequestInformation requestInfo,
            ParsableFactory<ModelType> factory,
            Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
            CancellationToken cancellationToken = default)
            where ModelType : IParsable =>
            throw new NotImplementedException();

        public Task<ModelType?> SendPrimitiveAsync<ModelType>(
            RequestInformation requestInfo,
            Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task<IEnumerable<ModelType>?> SendPrimitiveCollectionAsync<ModelType>(
            RequestInformation requestInfo,
            Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task SendNoContentAsync(
            RequestInformation requestInfo,
            Dictionary<string, ParsableFactory<IParsable>>? errorMapping = default,
            CancellationToken cancellationToken = default) =>
            throw new ApiException("not found") { ResponseStatusCode = 404 };
    }
}

public sealed class JamfTokenProviderCoreTests
{
    [Fact]
    public async Task GetAccessTokenAsync_Sends_OAuth_Scope_When_Configured()
    {
        var ct = TestContext.Current.CancellationToken;
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/oauth/token")
            .WithFormData(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "client",
                ["client_secret"] = "secret",
                ["scope"] = "role:computers.read",
            })
            .Respond("application/json", """{"access_token":"scoped","expires_in":3600,"token_type":"Bearer"}""");

        var options = Options.Create(new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.ClientCredentials,
            ClientId = "client",
            ClientSecret = "secret",
            OAuthScope = "role:computers.read",
        });

        var provider = new JamfTokenProvider(mock.ToHttpClient(), options);
        var token = await provider.GetAccessTokenAsync(ct);
        Assert.Equal("scoped", token);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task InvalidateAsync_Clears_Cache_And_Revokes_Basic_Token()
    {
        var ct = TestContext.Current.CancellationToken;
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/v1/auth/token")
            .Respond("application/json", """{"token":"basic-token","expires":"2099-01-01T00:00:00Z"}""");
        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/v1/auth/invalidate-token")
            .With(req => req.Headers.Authorization?.Parameter == "basic-token")
            .Respond(HttpStatusCode.NoContent);

        var options = Options.Create(new JamfClientOptions
        {
            BaseUrl = new Uri("https://example.jamfcloud.com"),
            AuthenticationMode = JamfAuthenticationMode.BasicToken,
            Username = "admin",
            Password = "pass",
        });

        var provider = new JamfTokenProvider(mock.ToHttpClient(), options);
        Assert.Equal("basic-token", await provider.GetAccessTokenAsync(ct));
        await provider.InvalidateAsync(ct);

        mock.Expect(HttpMethod.Post, "https://example.jamfcloud.com/api/v1/auth/token")
            .Respond("application/json", """{"token":"next-token","expires":"2099-01-01T00:00:00Z"}""");
        Assert.Equal("next-token", await provider.GetAccessTokenAsync(ct));
        mock.VerifyNoOutstandingExpectation();
    }
}
