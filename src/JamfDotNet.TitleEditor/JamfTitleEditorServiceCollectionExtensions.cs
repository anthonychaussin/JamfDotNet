using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JamfDotNet.TitleEditor;

/// <summary>DI helpers for the Jamf Title Editor client.</summary>
public static class JamfTitleEditorServiceCollectionExtensions
{
    /// <summary>Registers <see cref="JamfTitleEditorClient"/> and Title Editor token services.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfTitleEditorClient(this IServiceCollection services, Action<JamfTitleEditorOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfTitleEditorOptions>()
            .Configure(configure)
            .PostConfigure(static o => o.Validate());

        services.AddHttpClient(JamfHttpClientNames.TitleEditorToken);
        services.AddHttpClient(JamfHttpClientNames.TitleEditorApi)
            .AddJamfResilience(static sp => sp.GetService<JamfTitleEditorTokenProvider>());
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

    /// <summary>
    /// Registers <see cref="JamfTitleEditorClient"/> by binding options from configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name (defaults to <see cref="JamfTitleEditorOptions.SectionName"/>).</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfTitleEditorClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.ConfigureJamfResilience(configuration);
        return services.AddJamfTitleEditorClient(configuration.GetSection(sectionName ?? JamfTitleEditorOptions.SectionName));
    }

    /// <summary>
    /// Registers <see cref="JamfTitleEditorClient"/> by binding options from a configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfTitleEditorClient(this IServiceCollection services, IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        return services.AddJamfTitleEditorClient(options => section.Bind(options));
    }
}
