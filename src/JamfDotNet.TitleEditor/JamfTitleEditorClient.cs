using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using JamfDotNet.TitleEditor.Generated;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace JamfDotNet.TitleEditor;

/// <summary>
/// Developer-friendly entry point for the Jamf Title Editor API.
/// </summary>
public sealed class JamfTitleEditorClient : IDisposable
{
    private readonly HttpClient? _ownedHttpClient;
    private bool _disposed;

    /// <summary>Creates a Title Editor client.</summary>
    public JamfTitleEditorClient(JamfTitleEditorApiClient api, HttpClient? ownedHttpClient = null)
    {
        Api = api ?? throw new ArgumentNullException(nameof(api));
        _ownedHttpClient = ownedHttpClient;
    }

    /// <summary>Generated Title Editor API surface.</summary>
    public JamfTitleEditorApiClient Api { get; }

    /// <summary>Creates a client from options and authentication.</summary>
    public static JamfTitleEditorClient Create(
        JamfTitleEditorOptions options,
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

        return new JamfTitleEditorClient(new JamfTitleEditorApiClient(adapter), ownsClient ? httpClient : null);
    }

    /// <summary>Creates a client from DI options and token provider.</summary>
    public static JamfTitleEditorClient Create(
        IOptions<JamfTitleEditorOptions> options,
        IJamfTokenProvider tokenProvider,
        HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(tokenProvider);
        var auth = new JamfKiotaAuthenticationProvider(tokenProvider);
        return Create(options.Value, auth, httpClient);
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
