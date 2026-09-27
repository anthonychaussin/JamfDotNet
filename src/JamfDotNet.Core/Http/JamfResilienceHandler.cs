using System.Net;
using System.Net.Http.Headers;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Core.Http;

/// <summary>
/// Retries transient HTTP failures (429 / 503) and optionally refreshes bearer tokens after a 401.
/// </summary>
/// <remarks>
/// Place this handler outside auth handlers so a retry re-runs authentication
/// (for example outside <see cref="BearerAuthHandler"/>).
/// For Kiota pipelines without a bearer handler, this type refreshes the
/// <c>Authorization</c> header from <see cref="IJamfTokenProvider"/> after invalidation.
/// </remarks>
public sealed class JamfResilienceHandler : DelegatingHandler
{
    private readonly IJamfTokenProvider? _tokenProvider;
    private readonly int _maxTransientRetries;
    private readonly TimeSpan _baseDelay;
    private readonly bool _enableUnauthorizedRefresh;

    /// <summary>
    /// Creates a resilience handler.
    /// </summary>
    /// <param name="tokenProvider">
    /// Optional token provider used to invalidate and refresh credentials after HTTP 401.
    /// When <see langword="null"/>, 401 responses are not retried.
    /// </param>
    /// <param name="maxTransientRetries">Maximum retries for 429 / 503 (default 2).</param>
    /// <param name="baseDelay">Base delay for exponential backoff when <c>Retry-After</c> is absent.</param>
    /// <param name="enableUnauthorizedRefresh">When <see langword="false"/>, 401 responses are not retried.</param>
    public JamfResilienceHandler(
        IJamfTokenProvider? tokenProvider = null,
        int maxTransientRetries = 2,
        TimeSpan? baseDelay = null,
        bool enableUnauthorizedRefresh = true)
        : this(tokenProvider, new JamfResilienceOptions
        {
            MaxTransientRetries = maxTransientRetries,
            BaseDelay = baseDelay ?? TimeSpan.FromMilliseconds(250),
            EnableUnauthorizedRefresh = enableUnauthorizedRefresh,
        })
    {
    }

    /// <summary>
    /// Creates a resilience handler from options.
    /// </summary>
    /// <param name="tokenProvider">Optional token provider for 401 refresh.</param>
    /// <param name="options">Resilience options.</param>
    public JamfResilienceHandler(IJamfTokenProvider? tokenProvider, JamfResilienceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(options.MaxTransientRetries);
        _tokenProvider = tokenProvider;
        _maxTransientRetries = options.MaxTransientRetries;
        _baseDelay = options.BaseDelay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(250) : options.BaseDelay;
        _enableUnauthorizedRefresh = options.EnableUnauthorizedRefresh;
    }

    /// <summary>
    /// Creates a resilience handler from DI options.
    /// </summary>
    /// <param name="tokenProvider">Optional token provider for 401 refresh.</param>
    /// <param name="options">Options monitor.</param>
    public JamfResilienceHandler(IJamfTokenProvider? tokenProvider, IOptions<JamfResilienceOptions> options)
        : this(tokenProvider, options?.Value ?? new JamfResilienceOptions())
    {
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        HttpResponseMessage? response = null;
        var transientAttempt = 0;
        var refreshedUnauthorized = false;

        while (true)
        {
            response?.Dispose();
            response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized
                && _enableUnauthorizedRefresh
                && _tokenProvider is not null
                && !refreshedUnauthorized)
            {
                refreshedUnauthorized = true;
                response.Dispose();
                response = null;
                await _tokenProvider.InvalidateAsync(cancellationToken).ConfigureAwait(false);
                await ApplyBearerAsync(request, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if ((response.StatusCode == HttpStatusCode.TooManyRequests
                    || response.StatusCode == HttpStatusCode.ServiceUnavailable)
                && transientAttempt < _maxTransientRetries)
            {
                var delay = GetRetryDelay(response, transientAttempt);
                response.Dispose();
                response = null;
                transientAttempt++;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                }

                if (_tokenProvider is not null)
                {
                    await ApplyBearerAsync(request, cancellationToken).ConfigureAwait(false);
                }

                continue;
            }

            return response;
        }
    }

    private async Task ApplyBearerAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_tokenProvider is null)
        {
            return;
        }

        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt)
    {
        if (response.Headers.RetryAfter?.Delta is TimeSpan delta)
        {
            return delta;
        }

        if (response.Headers.RetryAfter?.Date is DateTimeOffset date)
        {
            var until = date - DateTimeOffset.UtcNow;
            return until > TimeSpan.Zero ? until : TimeSpan.Zero;
        }

        var factor = Math.Pow(2, attempt);
        return TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * factor);
    }
}
