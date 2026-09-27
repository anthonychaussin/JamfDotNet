using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Core.Authentication;

/// <summary>
/// Shared client-credentials OAuth token acquisition with in-memory cache.
/// </summary>
internal static class JamfOAuthClientCredentials
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// POSTs form-urlencoded credentials and returns access token + absolute expiry.
    /// </summary>
    public static async Task<(string AccessToken, DateTimeOffset ExpiresAt)> AcquireAsync(
        HttpClient httpClient,
        Uri tokenEndpoint,
        IEnumerable<KeyValuePair<string, string>> formFields,
        string productLabel,
        CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent(formFields);
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint) { Content = content };
        using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Failed to acquire {productLabel} OAuth token from {tokenEndpoint}. Status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        var token = JsonSerializer.Deserialize<OAuthTokenResponse>(body, JsonOptions)
            ?? throw new JamfApiException($"{productLabel} OAuth token response was empty.", response.StatusCode, body);
        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new JamfApiException(
                $"{productLabel} OAuth token response did not include access_token.",
                response.StatusCode,
                body);
        }

        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn is > 0 ? token.ExpiresIn.Value : 1800);
        return (token.AccessToken, expiresAt);
    }

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }
    }
}

/// <summary>
/// Obtains OAuth2 access tokens from the Jamf Platform API Gateway.
/// </summary>
public sealed class JamfPlatformTokenProvider : IJamfTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly JamfPlatformOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    /// <summary>Creates a new <see cref="JamfPlatformTokenProvider"/>.</summary>
    /// <param name="httpClient">HTTP client used for token requests.</param>
    /// <param name="options">Platform options.</param>
    public JamfPlatformTokenProvider(HttpClient httpClient, IOptions<JamfPlatformOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _options.Validate();
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasValidCachedToken())
        {
            return _accessToken!;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (HasValidCachedToken())
            {
                return _accessToken!;
            }

            (_accessToken, _expiresAt) = await JamfOAuthClientCredentials.AcquireAsync(
                _httpClient,
                _options.TokenEndpoint,
                new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _options.ClientId,
                    ["client_secret"] = _options.ClientSecret,
                },
                "Platform",
                cancellationToken).ConfigureAwait(false);
            return _accessToken!;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _accessToken = null;
            _expiresAt = DateTimeOffset.MinValue;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool HasValidCachedToken() =>
        !string.IsNullOrEmpty(_accessToken)
        && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-_options.TokenRefreshSkewSeconds);
}

/// <summary>
/// Obtains OAuth2 access tokens for Jamf Protect.
/// </summary>
public sealed class JamfProtectTokenProvider : IJamfTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly JamfProtectOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    /// <summary>Creates a new <see cref="JamfProtectTokenProvider"/>.</summary>
    /// <param name="httpClient">HTTP client used for token requests.</param>
    /// <param name="options">Protect options.</param>
    public JamfProtectTokenProvider(HttpClient httpClient, IOptions<JamfProtectOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _options.Validate();
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasValidCachedToken())
        {
            return _accessToken!;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (HasValidCachedToken())
            {
                return _accessToken!;
            }

            // Protect uses client_id + password (not standard client_secret grant).
            (_accessToken, _expiresAt) = await JamfOAuthClientCredentials.AcquireAsync(
                _httpClient,
                _options.TokenEndpoint,
                new Dictionary<string, string>
                {
                    ["client_id"] = _options.ClientId,
                    ["password"] = _options.ClientSecret,
                },
                "Protect",
                cancellationToken).ConfigureAwait(false);
            return _accessToken!;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _accessToken = null;
            _expiresAt = DateTimeOffset.MinValue;
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool HasValidCachedToken() =>
        !string.IsNullOrEmpty(_accessToken)
        && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-_options.TokenRefreshSkewSeconds);
}

/// <summary>
/// Obtains bearer tokens for Jamf Title Editor (basic exchange + keepalive).
/// </summary>
public sealed class JamfTitleEditorTokenProvider : IJamfTokenProvider
{
    private readonly HttpClient _httpClient;
    private readonly JamfTitleEditorOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    /// <summary>Creates a new <see cref="JamfTitleEditorTokenProvider"/>.</summary>
    /// <param name="httpClient">HTTP client used for token requests.</param>
    /// <param name="options">Title Editor options.</param>
    public JamfTitleEditorTokenProvider(HttpClient httpClient, IOptions<JamfTitleEditorOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _options.Validate();
    }

    /// <inheritdoc />
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (HasValidCachedToken())
        {
            return await KeepAliveIfNeededAsync(cancellationToken).ConfigureAwait(false);
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (HasValidCachedToken())
            {
                return _accessToken!;
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.TokenEndpoint);
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                throw new JamfApiException(
                    $"Failed to acquire Title Editor token from {_options.TokenEndpoint}. Status {(int)response.StatusCode}.",
                    response.StatusCode,
                    body);
            }

            ApplyTokenResponse(body, response.StatusCode);
            return _accessToken!;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _accessToken = null;
            _expiresAt = DateTimeOffset.MinValue;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<string> KeepAliveIfNeededAsync(CancellationToken cancellationToken)
    {
        // Title Editor tokens expire quickly; refresh via keepalive near expiry.
        if (DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-Math.Max(_options.TokenRefreshSkewSeconds, 30)))
        {
            return _accessToken!;
        }

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _options.KeepAliveEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                if (!string.IsNullOrWhiteSpace(body))
                {
                    ApplyTokenResponse(body, response.StatusCode, requireToken: false);
                }
                else
                {
                    _expiresAt = DateTimeOffset.UtcNow.AddMinutes(14);
                }

                return _accessToken!;
            }

            _accessToken = null;
            _expiresAt = DateTimeOffset.MinValue;
        }
        finally
        {
            _gate.Release();
        }

        return await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
    }

    private void ApplyTokenResponse(string body, System.Net.HttpStatusCode statusCode, bool requireToken = true)
    {
        using var document = JsonDocument.Parse(body);
        var root = document.RootElement;
        if (root.TryGetProperty("token", out var tokenElement))
        {
            var token = tokenElement.GetString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                _accessToken = token;
            }
        }

        if (requireToken && string.IsNullOrWhiteSpace(_accessToken))
        {
            throw new JamfApiException("Title Editor token response did not include token.", statusCode, body);
        }

        if (root.TryGetProperty("expires", out var expiresElement))
        {
            _expiresAt = ParseExpires(expiresElement) ?? DateTimeOffset.UtcNow.AddMinutes(14);
        }
        else
        {
            _expiresAt = DateTimeOffset.UtcNow.AddMinutes(14);
        }
    }

    private static DateTimeOffset? ParseExpires(JsonElement expiresElement) =>
        expiresElement.ValueKind switch
        {
            JsonValueKind.Number when expiresElement.TryGetInt64(out var unix) =>
                DateTimeOffset.FromUnixTimeSeconds(unix),
            JsonValueKind.String when DateTimeOffset.TryParse(expiresElement.GetString(), out var dto) => dto.ToUniversalTime(),
            _ => null,
        };

    private bool HasValidCachedToken() =>
        !string.IsNullOrEmpty(_accessToken)
        && DateTimeOffset.UtcNow < _expiresAt;
}
