using JamfDotNet.Core;
using JamfDotNet.Pro.Generated;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace JamfDotNet.Pro;

/// <summary>
/// Developer-friendly entry point for the Jamf Pro API.
/// </summary>
public sealed class JamfProClient : IDisposable
{
    private readonly HttpClient? _ownedHttpClient;
    private bool _disposed;

    /// <summary>
    /// Creates a <see cref="JamfProClient"/> using the supplied Kiota request adapter.
    /// </summary>
    /// <param name="api">Generated Jamf Pro API client.</param>
    /// <param name="ownedHttpClient">Optional <see cref="HttpClient"/> owned by this instance.</param>
    public JamfProClient(JamfProApiClient api, HttpClient? ownedHttpClient = null)
    {
        Api = api ?? throw new ArgumentNullException(nameof(api));
        _ownedHttpClient = ownedHttpClient;
    }

    /// <summary>
    /// Generated Jamf Pro API surface (resource-oriented request builders).
    /// </summary>
    public JamfProApiClient Api { get; }

    /// <summary>
    /// Creates a client from <see cref="JamfClientOptions"/> and an authentication provider.
    /// </summary>
    /// <param name="options">Jamf connection options.</param>
    /// <param name="authenticationProvider">Kiota authentication provider.</param>
    /// <param name="httpClient">Optional HTTP client. When null, a dedicated client is created and owned by the returned instance.</param>
    /// <returns>Configured <see cref="JamfProClient"/>.</returns>
    public static JamfProClient Create(
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
            BaseUrl = options.ApiBaseUrl.AbsoluteUri.TrimEnd('/'),
        };

        return new JamfProClient(new JamfProApiClient(adapter), ownsClient ? httpClient : null);
    }

    /// <summary>
    /// Creates a client from options monitor and DI-managed dependencies.
    /// </summary>
    /// <param name="options">Options monitor.</param>
    /// <param name="authenticationProvider">Authentication provider.</param>
    /// <param name="httpClient">HTTP client.</param>
    /// <returns>Configured <see cref="JamfProClient"/>.</returns>
    public static JamfProClient Create(
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
