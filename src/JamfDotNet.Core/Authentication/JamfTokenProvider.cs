using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Core.Authentication;

/// <summary>
/// Obtains and caches Jamf Pro bearer tokens.
/// </summary>
public sealed class JamfTokenProvider : IJamfTokenProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly JamfClientOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private string? _accessToken;
    private DateTimeOffset _expiresAt = DateTimeOffset.MinValue;

    /// <summary>
    /// Creates a new <see cref="JamfTokenProvider"/>.
    /// </summary>
    /// <param name="httpClient">HTTP client used for token requests. Base address is ignored; absolute URIs are used.</param>
    /// <param name="options">Jamf client options.</param>
    public JamfTokenProvider(HttpClient httpClient, IOptions<JamfClientOptions> options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = httpClient;
        _options = options.Value;
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

            return _options.AuthenticationMode switch
            {
                JamfAuthenticationMode.ClientCredentials => await AcquireClientCredentialsTokenAsync(cancellationToken).ConfigureAwait(false),
                JamfAuthenticationMode.BasicToken => await AcquireBasicTokenAsync(cancellationToken).ConfigureAwait(false),
                _ => throw new InvalidOperationException($"Unsupported authentication mode: {_options.AuthenticationMode}."),
            };
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

    private bool HasValidCachedToken()
    {
        return !string.IsNullOrEmpty(_accessToken)
            && DateTimeOffset.UtcNow < _expiresAt.AddSeconds(-_options.TokenRefreshSkewSeconds);
    }

    private async Task<string> AcquireClientCredentialsTokenAsync(CancellationToken cancellationToken)
    {
        var tokenUri = new Uri(_options.ApiBaseUrl, "oauth/token");
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId!,
            ["client_secret"] = _options.ClientSecret!,
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUri) { Content = content };
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Failed to acquire OAuth token from {tokenUri}. Status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        var token = JsonSerializer.Deserialize<OAuthTokenResponse>(body, JsonOptions)
            ?? throw new JamfApiException("OAuth token response was empty.", response.StatusCode, body);

        if (string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new JamfApiException("OAuth token response did not include access_token.", response.StatusCode, body);
        }

        _accessToken = token.AccessToken;
        var lifetime = token.ExpiresIn is > 0 ? token.ExpiresIn.Value : 1800;
        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(lifetime);
        return _accessToken;
    }

    private async Task<string> AcquireBasicTokenAsync(CancellationToken cancellationToken)
    {
        var tokenUri = new Uri(_options.ApiBaseUrl, "v1/auth/token");
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUri);
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_options.Username}:{_options.Password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new JamfApiException(
                $"Failed to acquire bearer token from {tokenUri}. Status {(int)response.StatusCode}.",
                response.StatusCode,
                body);
        }

        var token = JsonSerializer.Deserialize<BasicTokenResponse>(body, JsonOptions)
            ?? throw new JamfApiException("Bearer token response was empty.", response.StatusCode, body);

        if (string.IsNullOrWhiteSpace(token.Token))
        {
            throw new JamfApiException("Bearer token response did not include token.", response.StatusCode, body);
        }

        _accessToken = token.Token;
        if (token.Expires is { } expires)
        {
            _expiresAt = expires.ToUniversalTime();
        }
        else
        {
            _expiresAt = DateTimeOffset.UtcNow.AddMinutes(25);
        }

        return _accessToken;
    }

    private sealed class OAuthTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int? ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }

    private sealed class BasicTokenResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("expires")]
        public DateTimeOffset? Expires { get; set; }
    }
}
