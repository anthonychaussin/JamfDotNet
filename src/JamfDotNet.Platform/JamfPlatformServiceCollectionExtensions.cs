using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Platform;

/// <summary>DI helpers for the Jamf Platform client.</summary>
public static class JamfPlatformServiceCollectionExtensions
{
    /// <summary>Registers <see cref="JamfPlatformClient"/> and Platform token services.</summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfPlatformClient(this IServiceCollection services, Action<JamfPlatformOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfPlatformOptions>()
            .Configure(configure)
            .PostConfigure(static o => o.Validate());

        services.AddHttpClient(JamfHttpClientNames.PlatformToken);
        services.AddHttpClient(JamfHttpClientNames.PlatformApi)
            .AddJamfResilience(static sp => sp.GetService<JamfPlatformTokenProvider>());
        services.TryAddSingleton<JamfPlatformTokenProvider>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JamfPlatformTokenProvider(
                factory.CreateClient(JamfHttpClientNames.PlatformToken),
                sp.GetRequiredService<IOptions<JamfPlatformOptions>>());
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JamfPlatformOptions>>();
            var tokens = sp.GetRequiredService<JamfPlatformTokenProvider>();
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(JamfHttpClientNames.PlatformApi);
            return JamfPlatformClient.Create(options, tokens, http);
        });

        return services;
    }

    /// <summary>
    /// Registers <see cref="JamfPlatformClient"/> by binding options from configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name (defaults to <see cref="JamfPlatformOptions.SectionName"/>).</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfPlatformClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddJamfPlatformClient(configuration.GetSection(sectionName ?? JamfPlatformOptions.SectionName));
    }

    /// <summary>
    /// Registers <see cref="JamfPlatformClient"/> by binding options from a configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfPlatformClient(this IServiceCollection services, IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        return services.AddJamfPlatformClient(options => section.Bind(options));
    }
}
