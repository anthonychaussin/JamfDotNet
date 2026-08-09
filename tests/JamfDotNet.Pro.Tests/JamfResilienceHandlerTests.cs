using System.Net;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Core.Http;

namespace JamfDotNet.Pro.Tests;

public sealed class JamfResilienceHandlerTests
{
    [Fact]
    public async Task Retries_Unauthorized_After_Token_Invalidation()
    {
        var provider = new FakeTokenProvider();
        var inner = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.Unauthorized),
            new HttpResponseMessage(HttpStatusCode.OK));

        using var handler = new JamfResilienceHandler(provider) { InnerHandler = inner };
        using var client = new HttpClient(handler);

        using var response = await client.GetAsync("https://example.test/api", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, provider.InvalidateCount);
        Assert.Equal(2, inner.CallCount);
    }

    [Fact]
    public async Task Retries_TooManyRequests()
    {
        var inner = new SequenceHandler(
            new HttpResponseMessage(HttpStatusCode.TooManyRequests),
            new HttpResponseMessage(HttpStatusCode.OK));

        using var handler = new JamfResilienceHandler(tokenProvider: null, maxTransientRetries: 2, baseDelay: TimeSpan.Zero)
        {
            InnerHandler = inner,
        };
        using var client = new HttpClient(handler);

        using var response = await client.GetAsync("https://example.test/api", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, inner.CallCount);
    }

    private sealed class FakeTokenProvider : IJamfTokenProvider
    {
        public int InvalidateCount { get; private set; }

        public Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult("token");

        public Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            InvalidateCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class SequenceHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;

        public SequenceHandler(params HttpResponseMessage[] responses) =>
            _responses = new Queue<HttpResponseMessage>(responses);

        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
