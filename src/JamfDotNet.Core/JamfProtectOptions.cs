namespace JamfDotNet.Core;

/// <summary>
/// Configuration for the Jamf Protect GraphQL API.
/// </summary>
public sealed class JamfProtectOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "JamfProtect";

    /// <summary>
    /// Protect tenant base URL (for example <c>https://contoso.protect.jamfcloud.com</c>).
    /// </summary>
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>OAuth2 client ID.</summary>
    public string ClientId { get; set; } = null!;

    /// <summary>OAuth2 client secret (sent as <c>password</c> in the token request).</summary>
    public string ClientSecret { get; set; } = null!;

    /// <summary>Seconds before expiry to refresh the token.</summary>
    public int TokenRefreshSkewSeconds { get; set; } = 60;

    /// <summary>Primary GraphQL endpoint used by the SDK (<c>{BaseUrl}/app</c>).</summary>
    public Uri GraphQlEndpoint => new(Combine(BaseUrl, "app"));

    /// <summary>OAuth token endpoint (<c>{BaseUrl}/token</c>).</summary>
    public Uri TokenEndpoint => new(Combine(BaseUrl, "token"));

    /// <summary>Validates required options.</summary>
    public void Validate()
    {
        if (BaseUrl is null || !BaseUrl.IsAbsoluteUri)
        {
            throw new InvalidOperationException($"{nameof(BaseUrl)} must be an absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException($"{nameof(ClientId)} is required.");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException($"{nameof(ClientSecret)} is required.");
        }
    }

    private static string Combine(Uri baseUrl, string relative)
    {
        var root = baseUrl.AbsoluteUri.TrimEnd('/') + "/";
        return root + relative.TrimStart('/');
    }
}
