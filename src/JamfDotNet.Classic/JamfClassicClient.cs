using JamfDotNet.Classic.Generated;
using JamfDotNet.Core;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace JamfDotNet.Classic;

/// <summary>
/// Developer-friendly entry point for the Jamf Classic API (<c>/JSSResource</c>).
/// </summary>
/// <remarks>
/// Classic GET endpoints typically support JSON via <c>Accept: application/json</c>.
/// Create and update operations generally require XML request bodies.
/// Prefer the Jamf Pro API (<c>JamfDotNet.Pro.JamfProClient</c>) for new integrations when an equivalent endpoint exists.
/// </remarks>
public sealed class JamfClassicClient : IDisposable
{
    private readonly HttpClient? _ownedHttpClient;
    private bool _disposed;

    /// <summary>
    /// Creates a <see cref="JamfClassicClient"/> using the supplied Kiota request adapter.
    /// </summary>
    /// <param name="api">Generated Classic API client.</param>
    /// <param name="ownedHttpClient">Optional <see cref="HttpClient"/> owned by this instance.</param>
    public JamfClassicClient(JamfClassicApiClient api, HttpClient? ownedHttpClient = null)
    {
        Api = api ?? throw new ArgumentNullException(nameof(api));
        _ownedHttpClient = ownedHttpClient;
    }

    /// <summary>
    /// Generated Classic API surface (resource-oriented request builders under <c>/JSSResource</c>).
    /// </summary>
    public JamfClassicApiClient Api { get; }

    /// <summary>
    /// Creates a client from <see cref="JamfClientOptions"/> and an authentication provider.
    /// </summary>
    /// <param name="options">Jamf connection options.</param>
    /// <param name="authenticationProvider">Kiota authentication provider.</param>
    /// <param name="httpClient">Optional HTTP client. When null, a dedicated client is created and owned by the returned instance.</param>
    /// <returns>Configured <see cref="JamfClassicClient"/>.</returns>
    public static JamfClassicClient Create(
        JamfClientOptions options,
        IAuthenticationProvider authenticationProvider,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(authenticationProvider);
        options.Validate();

        var ownsClient = httpClient is null;
        httpClient ??= new HttpClient();
        var adapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient)
        {
            BaseUrl = options.ClassicApiBaseUrl.AbsoluteUri.TrimEnd('/'),
        };

        return new JamfClassicClient(new JamfClassicApiClient(adapter), ownsClient ? httpClient : null);
    }

    /// <summary>
    /// Creates a client from options monitor and DI-managed dependencies.
    /// </summary>
    /// <param name="options">Options monitor.</param>
    /// <param name="authenticationProvider">Authentication provider.</param>
    /// <param name="httpClient">HTTP client.</param>
    /// <returns>Configured <see cref="JamfClassicClient"/>.</returns>
    public static JamfClassicClient Create(
        IOptions<JamfClientOptions> options,
        IAuthenticationProvider authenticationProvider,
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        return Create(options.Value, authenticationProvider, httpClient);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _ownedHttpClient?.Dispose();
    }
}
