using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Core.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Protect;

/// <summary>DI helpers for the Jamf Protect GraphQL client.</summary>
public static class JamfProtectServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="JamfProtectClient"/> and Protect token services with bearer auth middleware.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfProtectClient(this IServiceCollection services, Action<JamfProtectOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfProtectOptions>()
            .Configure(configure)
            .PostConfigure(static o => o.Validate());

        services.AddHttpClient(JamfHttpClientNames.ProtectToken);
        services.TryAddSingleton<JamfProtectTokenProvider>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            return new JamfProtectTokenProvider(
                factory.CreateClient(JamfHttpClientNames.ProtectToken),
                sp.GetRequiredService<IOptions<JamfProtectOptions>>());
        });

        services.AddHttpClient(JamfHttpClientNames.ProtectApi)
            .AddJamfResilience(static sp => sp.GetService<JamfProtectTokenProvider>())
            .AddHttpMessageHandler(sp => new BearerAuthHandler(sp.GetRequiredService<JamfProtectTokenProvider>()));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JamfProtectOptions>>();
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(JamfHttpClientNames.ProtectApi);
            return JamfProtectClient.Create(options, http);
        });

        return services;
    }

    /// <summary>
    /// Registers <see cref="JamfProtectClient"/> by binding options from configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name (defaults to <see cref="JamfProtectOptions.SectionName"/>).</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfProtectClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddJamfProtectClient(configuration.GetSection(sectionName ?? JamfProtectOptions.SectionName));
    }

    /// <summary>
    /// Registers <see cref="JamfProtectClient"/> by binding options from a configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfProtectClient(this IServiceCollection services, IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(section);
        return services.AddJamfProtectClient(options => section.Bind(options));
    }
}
