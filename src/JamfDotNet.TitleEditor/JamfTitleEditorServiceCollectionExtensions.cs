using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JamfDotNet.TitleEditor;

/// <summary>DI helpers for the Jamf Title Editor client.</summary>
public static class JamfTitleEditorServiceCollectionExtensions
{
    /// <summary>Registers <see cref="JamfTitleEditorClient"/> and Title Editor token services.</summary>
    public static IServiceCollection AddJamfTitleEditorClient(this IServiceCollection services, Action<JamfTitleEditorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfTitleEditorOptions>()
            .Configure(configure)
            .PostConfigure(static o => o.Validate());

        services.AddHttpClient(JamfHttpClientNames.TitleEditorToken);
        services.AddHttpClient(JamfHttpClientNames.TitleEditorApi);
        services.TryAddSingleton<JamfTitleEditorTokenProvider>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JamfTitleEditorTokenProvider(
                factory.CreateClient(JamfHttpClientNames.TitleEditorToken),
                sp.GetRequiredService<IOptions<JamfTitleEditorOptions>>());
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JamfTitleEditorOptions>>();
            var tokens = sp.GetRequiredService<JamfTitleEditorTokenProvider>();
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(JamfHttpClientNames.TitleEditorApi);
            return JamfTitleEditorClient.Create(options, tokens, http);
        });

        return services;
    }
}
