using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Core.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
/// Emits <see cref="Activity"/> events via <see cref="JamfDiagnostics.ActivitySource"/> and optional <see cref="ILogger"/>.
/// </remarks>
public sealed class JamfResilienceHandler : DelegatingHandler
{
    private readonly IJamfTokenProvider? _tokenProvider;
    private readonly int _maxTransientRetries;
    private readonly TimeSpan _baseDelay;
    private readonly bool _enableUnauthorizedRefresh;
    private readonly ILogger _logger;

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
    /// <param name="logger">Optional logger (defaults to null logger).</param>
    public JamfResilienceHandler(
        IJamfTokenProvider? tokenProvider = null,
        int maxTransientRetries = 2,
        TimeSpan? baseDelay = null,
        bool enableUnauthorizedRefresh = true,
        ILogger<JamfResilienceHandler>? logger = null)
        : this(tokenProvider, new JamfResilienceOptions
        {
            MaxTransientRetries = maxTransientRetries,
            BaseDelay = baseDelay ?? TimeSpan.FromMilliseconds(250),
            EnableUnauthorizedRefresh = enableUnauthorizedRefresh,
        }, logger)
    {
    }

    /// <summary>
    /// Creates a resilience handler from options.
    /// </summary>
    /// <param name="tokenProvider">Optional token provider for 401 refresh.</param>
    /// <param name="options">Resilience options.</param>
    /// <param name="logger">Optional logger.</param>
    public JamfResilienceHandler(
        IJamfTokenProvider? tokenProvider,
        JamfResilienceOptions options,
        ILogger<JamfResilienceHandler>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentOutOfRangeException.ThrowIfNegative(options.MaxTransientRetries);
        _tokenProvider = tokenProvider;
        _maxTransientRetries = options.MaxTransientRetries;
        _baseDelay = options.BaseDelay <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(250) : options.BaseDelay;
        _enableUnauthorizedRefresh = options.EnableUnauthorizedRefresh;
        _logger = logger ?? NullLogger<JamfResilienceHandler>.Instance;
    }

    /// <summary>
    /// Creates a resilience handler from DI options.
    /// </summary>
    /// <param name="tokenProvider">Optional token provider for 401 refresh.</param>
    /// <param name="options">Options monitor.</param>
    /// <param name="logger">Optional logger.</param>
    public JamfResilienceHandler(
        IJamfTokenProvider? tokenProvider,
        IOptions<JamfResilienceOptions> options,
        ILogger<JamfResilienceHandler>? logger = null)
        : this(tokenProvider, options?.Value ?? new JamfResilienceOptions(), logger)
    {
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var activity = JamfDiagnostics.ActivitySource.StartActivity("jamf.http.send");
        activity?.SetTag("http.request.method", request.Method.Method);
        if (request.RequestUri is not null)
        {
            activity?.SetTag("url.scheme", request.RequestUri.Scheme);
            activity?.SetTag("server.address", request.RequestUri.Host);
            activity?.SetTag("url.path", request.RequestUri.AbsolutePath);
        }

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
                activity?.AddEvent(new ActivityEvent(
                    "jamf.token.refresh",
                    tags: new ActivityTagsCollection
                    {
                        { "jamf.retry.reason", "unauthorized" },
                    }));
                _logger.LogInformation("Jamf API returned 401; invalidating token and retrying once.");
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
                var statusCode = (int)response.StatusCode;
                activity?.AddEvent(new ActivityEvent(
                    "jamf.http.retry",
                    tags: new ActivityTagsCollection
                    {
                        { "http.response.status_code", statusCode },
                        { "jamf.retry.attempt", transientAttempt + 1 },
                        { "jamf.retry.delay_ms", delay.TotalMilliseconds },
                    }));
                _logger.LogDebug(
                    "Jamf API returned {StatusCode}; retry {Attempt} after {DelayMs} ms.",
                    statusCode,
                    transientAttempt + 1,
                    delay.TotalMilliseconds);
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

            activity?.SetTag("http.response.status_code", (int)response.StatusCode);
            if ((int)response.StatusCode >= 400)
            {
                activity?.SetStatus(ActivityStatusCode.Error);
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
