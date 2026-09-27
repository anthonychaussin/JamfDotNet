using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;

namespace JamfDotNet.Pro;

/// <summary>
/// DI registration helpers for the Jamf Pro client.
/// </summary>
public static class JamfProServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="JamfProClient"/> and shared Jamf core services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfProClient(this IServiceCollection services, Action<JamfClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddJamfCore(configure);
        services.AddHttpClient(JamfHttpClientNames.ProApi)
            .AddJamfResilience(static sp => sp.GetService<IJamfTokenProvider>());
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JamfClientOptions>>();
            var auth = sp.GetRequiredService<IAuthenticationProvider>();
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient(JamfHttpClientNames.ProApi);
            return JamfProClient.Create(options, auth, httpClient);
        });

        return services;
    }

    /// <summary>
    /// Registers <see cref="JamfProClient"/> by binding options from configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name (defaults to <see cref="JamfClientOptions.SectionName"/>).</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfProClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        services.ConfigureJamfResilience(configuration);
        return services.AddJamfProClient(configuration.GetSection(sectionName ?? JamfClientOptions.SectionName));
    }

    /// <summary>
    /// Registers <see cref="JamfProClient"/> by binding options from a configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfProClient(this IServiceCollection services, IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        return services.AddJamfProClient(options => section.Bind(options));
    }
}
