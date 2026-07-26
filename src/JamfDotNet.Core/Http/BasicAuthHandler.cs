using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Core.Http;

/// <summary>
/// Attaches Basic authentication and the Jamf School protocol version header.
/// </summary>
public sealed class BasicAuthHandler : DelegatingHandler
{
    private readonly IOptions<JamfSchoolOptions> _options;

    /// <summary>
    /// Creates a new <see cref="BasicAuthHandler"/> for School credentials.
    /// </summary>
    /// <param name="options">School options containing Network ID, API key, and protocol version.</param>
    public BasicAuthHandler(IOptions<JamfSchoolOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var options = _options.Value;
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.NetworkId}:{options.ApiKey}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Headers.Remove("X-Server-Protocol-Version");
        request.Headers.TryAddWithoutValidation("X-Server-Protocol-Version", options.ProtocolVersion);
        return base.SendAsync(request, cancellationToken);
    }
}
