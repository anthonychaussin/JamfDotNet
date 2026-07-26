namespace JamfDotNet.Core.Authentication;

/// <summary>
/// Provides cached Jamf Pro access tokens.
/// </summary>
public interface IJamfTokenProvider
{
    /// <summary>
    /// Returns a valid bearer access token, refreshing it when required.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Bearer access token.</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears any cached token so the next call acquires a new one.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}
