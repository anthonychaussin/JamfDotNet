namespace JamfDotNet.Core;

/// <summary>
/// Configuration for connecting to a Jamf Pro server.
/// </summary>
public sealed class JamfClientOptions
{
    /// <summary>
    /// Configuration section name used with <c>IConfiguration</c> binding.
    /// </summary>
    public const string SectionName = "Jamf";

    /// <summary>
    /// Base URL of the Jamf Pro server (for example <c>https://contoso.jamfcloud.com</c>).
    /// Must not include the <c>/api</c> suffix.
    /// </summary>
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>
    /// Authentication mode. Defaults to <see cref="JamfAuthenticationMode.ClientCredentials"/>.
    /// </summary>
    public JamfAuthenticationMode AuthenticationMode { get; set; } = JamfAuthenticationMode.ClientCredentials;

    /// <summary>
    /// OAuth2 client ID (API client) when using <see cref="JamfAuthenticationMode.ClientCredentials"/>.
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    /// OAuth2 client secret when using <see cref="JamfAuthenticationMode.ClientCredentials"/>.
    /// </summary>
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Username when using <see cref="JamfAuthenticationMode.BasicToken"/>.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password when using <see cref="JamfAuthenticationMode.BasicToken"/>.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Refresh the bearer token this many seconds before the reported expiry.
    /// </summary>
    public int TokenRefreshSkewSeconds { get; set; } = 60;

    /// <summary>
    /// Absolute URI of the Jamf Pro API root (<c>{BaseUrl}/api</c>).
    /// </summary>
    public Uri ApiBaseUrl => new(Combine(BaseUrl, "api/"));

    /// <summary>
    /// Absolute URI of the Classic API root (<c>{BaseUrl}/JSSResource</c>).
    /// </summary>
    public Uri ClassicApiBaseUrl => new(Combine(BaseUrl, "JSSResource/"));

    /// <summary>
    /// Validates required options for the selected authentication mode.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when required values are missing or invalid.</exception>
    public void Validate()
    {
        if (BaseUrl is null)
        {
            throw new InvalidOperationException($"{nameof(BaseUrl)} is required.");
        }

        if (!BaseUrl.IsAbsoluteUri || (BaseUrl.Scheme != Uri.UriSchemeHttps && BaseUrl.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException($"{nameof(BaseUrl)} must be an absolute http(s) URI.");
        }

        if (TokenRefreshSkewSeconds < 0)
        {
            throw new InvalidOperationException($"{nameof(TokenRefreshSkewSeconds)} must be >= 0.");
        }

        switch (AuthenticationMode)
        {
            case JamfAuthenticationMode.ClientCredentials:
                if (string.IsNullOrWhiteSpace(ClientId))
                {
                    throw new InvalidOperationException($"{nameof(ClientId)} is required for client credentials authentication.");
                }

                if (string.IsNullOrWhiteSpace(ClientSecret))
                {
                    throw new InvalidOperationException($"{nameof(ClientSecret)} is required for client credentials authentication.");
                }

                break;

            case JamfAuthenticationMode.BasicToken:
                if (string.IsNullOrWhiteSpace(Username))
                {
                    throw new InvalidOperationException($"{nameof(Username)} is required for basic token authentication.");
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    throw new InvalidOperationException($"{nameof(Password)} is required for basic token authentication.");
                }

                break;

            default:
                throw new InvalidOperationException($"Unsupported authentication mode: {AuthenticationMode}.");
        }
    }

    private static string Combine(Uri baseUrl, string relative)
    {
        var root = baseUrl.AbsoluteUri.TrimEnd('/') + "/";
        return root + relative.TrimStart('/');
    }
}
