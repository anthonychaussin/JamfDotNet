namespace JamfDotNet.Core;

/// <summary>
/// Configuration for the Jamf Platform API Gateway.
/// </summary>
public sealed class JamfPlatformOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "JamfPlatform";

    /// <summary>
    /// Gateway base URL without a trailing slash (for example <c>https://us.apigw.jamf.com</c>).
    /// </summary>
    public Uri GatewayBaseUrl { get; set; } = null!;

    /// <summary>OAuth2 client ID for the Platform integration.</summary>
    public string ClientId { get; set; } = null!;

    /// <summary>OAuth2 client secret for the Platform integration.</summary>
    public string ClientSecret { get; set; } = null!;

    /// <summary>Tenant identifier injected into <c>/v1/tenant/{tenantId}/...</c> paths.</summary>
    public string TenantId { get; set; } = null!;

    /// <summary>Seconds before expiry to refresh the token.</summary>
    public int TokenRefreshSkewSeconds { get; set; } = 60;

    /// <summary>Absolute URI of the Platform token endpoint.</summary>
    public Uri TokenEndpoint => new(Combine(GatewayBaseUrl, "auth/token"));

    /// <summary>Builds a service base URL such as <c>https://us.apigw.jamf.com/api/blueprints</c>.</summary>
    public Uri GetServiceBaseUrl(string servicePath) => new(Combine(GatewayBaseUrl, servicePath.TrimStart('/')));

    /// <summary>Validates required options.</summary>
    public void Validate()
    {
        if (GatewayBaseUrl is null || !GatewayBaseUrl.IsAbsoluteUri)
        {
            throw new InvalidOperationException($"{nameof(GatewayBaseUrl)} must be an absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException($"{nameof(ClientId)} is required.");
        }

        if (string.IsNullOrWhiteSpace(ClientSecret))
        {
            throw new InvalidOperationException($"{nameof(ClientSecret)} is required.");
        }

        if (string.IsNullOrWhiteSpace(TenantId))
        {
            throw new InvalidOperationException($"{nameof(TenantId)} is required.");
        }
    }

    private static string Combine(Uri baseUrl, string relative)
    {
        var root = baseUrl.AbsoluteUri.TrimEnd('/') + "/";
        return root + relative.TrimStart('/');
    }
}
