using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace JamfDotNet.Platform;

/// <summary>DI helpers for the Jamf Platform client.</summary>
public static class JamfPlatformServiceCollectionExtensions
{
    /// <summary>Registers <see cref="JamfPlatformClient"/> and Platform token services.</summary>
    public static IServiceCollection AddJamfPlatformClient(this IServiceCollection services, Action<JamfPlatformOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfPlatformOptions>()
            .Configure(configure)
            .PostConfigure(static o => o.Validate());

        services.AddHttpClient(JamfHttpClientNames.PlatformToken);
        services.AddHttpClient(JamfHttpClientNames.PlatformApi);
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
}
