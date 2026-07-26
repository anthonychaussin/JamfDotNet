using System.Net.Http.Headers;
using JamfDotNet.Core.Authentication;

namespace JamfDotNet.Core.Http;

/// <summary>
/// Attaches a bearer access token from <see cref="IJamfTokenProvider"/> to outgoing requests.
/// </summary>
public sealed class BearerAuthHandler : DelegatingHandler
{
    private readonly IJamfTokenProvider _tokenProvider;

    /// <summary>
    /// Creates a new <see cref="BearerAuthHandler"/>.
    /// </summary>
    /// <param name="tokenProvider">Token provider used to obtain bearer tokens.</param>
    public BearerAuthHandler(IJamfTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
