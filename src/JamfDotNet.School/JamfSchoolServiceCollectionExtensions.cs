using JamfDotNet.Core;
using JamfDotNet.Core.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JamfDotNet.School;

/// <summary>DI helpers for the Jamf School client.</summary>
public static class JamfSchoolServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="JamfSchoolClient"/> with Basic auth and protocol version middleware.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfSchoolClient(this IServiceCollection services, Action<JamfSchoolOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfSchoolOptions>()
            .Configure(configure)
            .PostConfigure(static o => o.Validate());

        services.TryAddTransient<BasicAuthHandler>();
        services.AddHttpClient(JamfHttpClientNames.SchoolApi)
            .AddHttpMessageHandler<BasicAuthHandler>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<JamfSchoolOptions>>();
            var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient(JamfHttpClientNames.SchoolApi);
            return JamfSchoolClient.Create(options, http);
        });

        return services;
    }
}
