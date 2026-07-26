using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace JamfDotNet.Core.Authentication;

/// <summary>
/// Kiota <see cref="IAuthenticationProvider"/> that attaches Jamf bearer tokens.
/// </summary>
public sealed class JamfKiotaAuthenticationProvider : IAuthenticationProvider
{
    private readonly IJamfTokenProvider _tokenProvider;

    /// <summary>
    /// Creates a new <see cref="JamfKiotaAuthenticationProvider"/>.
    /// </summary>
    /// <param name="tokenProvider">Token provider.</param>
    public JamfKiotaAuthenticationProvider(IJamfTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
    }

    /// <inheritdoc />
    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var token = await _tokenProvider.GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        request.Headers.Remove("Authorization");
        request.Headers.Add("Authorization", $"Bearer {token}");
    }
}
