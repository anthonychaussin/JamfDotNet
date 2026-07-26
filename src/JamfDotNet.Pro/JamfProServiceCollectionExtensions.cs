using JamfDotNet.Core;
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
        services.AddHttpClient(JamfHttpClientNames.ProApi);
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
}
