namespace JamfDotNet.Core;

/// <summary>
/// Authentication mode used when obtaining Jamf Pro API bearer tokens.
/// </summary>
public enum JamfAuthenticationMode
{
    /// <summary>
    /// OAuth2 client credentials against <c>/api/oauth/token</c> (API roles &amp; clients).
    /// </summary>
    ClientCredentials = 0,

    /// <summary>
    /// Username/password exchanged for a bearer token via <c>/api/v1/auth/token</c>.
    /// </summary>
    BasicToken = 1,
}
