namespace JamfDotNet.Core;

/// <summary>
/// Configuration for the Jamf School API.
/// </summary>
public sealed class JamfSchoolOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "JamfSchool";

    /// <summary>
    /// School instance base URL (for example <c>https://contoso.jamfcloud.com</c>).
    /// Must not include the <c>/api</c> suffix.
    /// </summary>
    public Uri BaseUrl { get; set; } = null!;

    /// <summary>Network ID used as the Basic auth username.</summary>
    public string NetworkId { get; set; } = null!;

    /// <summary>API key used as the Basic auth password.</summary>
    public string ApiKey { get; set; } = null!;

    /// <summary>
    /// Value for the <c>X-Server-Protocol-Version</c> header. Defaults to <c>3</c>.
    /// </summary>
    public string ProtocolVersion { get; set; } = "3";

    /// <summary>API root (<c>{BaseUrl}/api/</c>).</summary>
    public Uri ApiBaseUrl => new(Combine(BaseUrl, "api/"));

    /// <summary>Validates required options.</summary>
    public void Validate()
    {
        if (BaseUrl is null || !BaseUrl.IsAbsoluteUri)
        {
            throw new InvalidOperationException($"{nameof(BaseUrl)} must be an absolute URI.");
        }

        if (string.IsNullOrWhiteSpace(NetworkId))
        {
            throw new InvalidOperationException($"{nameof(NetworkId)} is required.");
        }

        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new InvalidOperationException($"{nameof(ApiKey)} is required.");
        }

        if (string.IsNullOrWhiteSpace(ProtocolVersion))
        {
            throw new InvalidOperationException($"{nameof(ProtocolVersion)} is required.");
        }
    }

    private static string Combine(Uri baseUrl, string relative)
    {
        var root = baseUrl.AbsoluteUri.TrimEnd('/') + "/";
        return root + relative.TrimStart('/');
    }
}
