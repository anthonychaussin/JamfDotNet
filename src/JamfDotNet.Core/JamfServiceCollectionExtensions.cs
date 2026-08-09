using JamfDotNet.Core.Authentication;
using JamfDotNet.Core.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions.Authentication;

namespace JamfDotNet.Core;

/// <summary>
/// Dependency injection helpers for JamfDotNet core services.
/// </summary>
public static class JamfServiceCollectionExtensions
{
    /// <summary>
    /// Registers Jamf options, token provider, and Kiota authentication provider.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configure">Options configuration callback.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfCore(this IServiceCollection services, Action<JamfClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<JamfClientOptions>()
            .Configure(configure)
            .PostConfigure(static options => options.Validate());

        services.AddHttpClient(JamfHttpClientNames.Token);
        services.TryAddSingleton<IJamfTokenProvider>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var options = sp.GetRequiredService<IOptions<JamfClientOptions>>();
            return new JamfTokenProvider(factory.CreateClient(JamfHttpClientNames.Token), options);
        });
        services.TryAddSingleton<IAuthenticationProvider, JamfKiotaAuthenticationProvider>();

        return services;
    }

    /// <summary>
    /// Registers Jamf core services by binding <see cref="JamfClientOptions"/> from configuration.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="sectionName">Section name (defaults to <see cref="JamfClientOptions.SectionName"/>).</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfCore(
        this IServiceCollection services,
        IConfiguration configuration,
        string? sectionName = null)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return services.AddJamfCore(configuration.GetSection(sectionName ?? JamfClientOptions.SectionName));
    }

    /// <summary>
    /// Registers Jamf core services by binding <see cref="JamfClientOptions"/> from a configuration section.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="section">Configuration section.</param>
    /// <returns>The same service collection.</returns>
    public static IServiceCollection AddJamfCore(this IServiceCollection services, IConfigurationSection section)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(section);
        return services.AddJamfCore(options => section.Bind(options));
    }

    /// <summary>
    /// Adds <see cref="JamfResilienceHandler"/> to a named HTTP client builder.
    /// </summary>
    /// <param name="builder">HTTP client builder.</param>
    /// <param name="tokenProviderFactory">
    /// Optional factory for the token provider used on 401 refresh.
    /// When <see langword="null"/>, only 429 / 503 retries are enabled.
    /// </param>
    /// <returns>The same builder.</returns>
    public static IHttpClientBuilder AddJamfResilience(
        this IHttpClientBuilder builder,
        Func<IServiceProvider, IJamfTokenProvider?>? tokenProviderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddHttpMessageHandler(sp =>
            new JamfResilienceHandler(tokenProviderFactory?.Invoke(sp)));
    }
}

/// <summary>
/// Named <see cref="HttpClient"/> registrations used by JamfDotNet.
/// </summary>
public static class JamfHttpClientNames
{
    /// <summary>
    /// HTTP client used for token acquisition.
    /// </summary>
    public const string Token = "JamfDotNet.Token";

    /// <summary>
    /// HTTP client used for Jamf Pro API calls.
    /// </summary>
    public const string ProApi = "JamfDotNet.ProApi";

    /// <summary>
    /// HTTP client used for Jamf Classic API calls.
    /// </summary>
    public const string ClassicApi = "JamfDotNet.ClassicApi";

    /// <summary>HTTP client used for Platform Gateway API calls.</summary>
    public const string PlatformApi = "JamfDotNet.PlatformApi";

    /// <summary>HTTP client used for Platform token acquisition.</summary>
    public const string PlatformToken = "JamfDotNet.PlatformToken";

    /// <summary>HTTP client used for Protect GraphQL calls.</summary>
    public const string ProtectApi = "JamfDotNet.ProtectApi";

    /// <summary>HTTP client used for Protect token acquisition.</summary>
    public const string ProtectToken = "JamfDotNet.ProtectToken";

    /// <summary>HTTP client used for School API calls.</summary>
    public const string SchoolApi = "JamfDotNet.SchoolApi";

    /// <summary>HTTP client used for Title Editor API calls.</summary>
    public const string TitleEditorApi = "JamfDotNet.TitleEditorApi";

    /// <summary>HTTP client used for Title Editor token acquisition.</summary>
    public const string TitleEditorToken = "JamfDotNet.TitleEditorToken";
}
