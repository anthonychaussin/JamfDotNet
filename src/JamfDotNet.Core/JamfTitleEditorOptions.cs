namespace JamfDotNet.Core;

/// <summary>
/// Configuration for the Jamf Title Editor API.
/// </summary>
public sealed class JamfTitleEditorOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "JamfTitleEditor";

    /// <summary>
    /// Title Editor base URL including the API root when known
    /// (for example <c>https://instance.appcatalog.jamfcloud.com</c>).
    /// The client appends <c>/v2</c> as the API base path.
    /// </summary>
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>Username for basic→bearer exchange.</summary>
    public string Username { get; set; } = null!;

    /// <summary>Password for basic→bearer exchange.</summary>
    public string Password { get; set; } = null!;

    /// <summary>Seconds before expiry to refresh the token.</summary>
    public int TokenRefreshSkewSeconds { get; set; } = 60;

    /// <summary>
    /// When <see langword="true"/>, Kiota <c>ApiException</c> failures are remapped to
    /// <see cref="JamfApiException"/> automatically for Title Editor clients created from these options.
    /// </summary>
    public bool RemapKiotaExceptions { get; set; }

    /// <summary>API base (<c>{BaseUrl}/v2/</c>).</summary>
    public Uri ApiBaseUrl => new(Combine(BaseUrl, "v2/"));

    /// <summary>Token endpoint.</summary>
    public Uri TokenEndpoint => new(Combine(ApiBaseUrl, "auth/token"));

    /// <summary>Keep-alive endpoint.</summary>
    public Uri KeepAliveEndpoint => new(Combine(ApiBaseUrl, "auth/keepalive"));

    /// <summary>Validates required options.</summary>
    public void Validate()
    {
        if (BaseUrl is null || !BaseUrl.IsAbsoluteUri)
        {
            throw new InvalidOperationException($"{nameof(BaseUrl)} must be an absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            throw new InvalidOperationException($"{nameof(Username)} is required.");
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException($"{nameof(Password)} is required.");
        }
    }

    private static string Combine(Uri baseUrl, string relative)
    {
        var root = baseUrl.AbsoluteUri.TrimEnd('/') + "/";
        return root + relative.TrimStart('/');
    }
}
