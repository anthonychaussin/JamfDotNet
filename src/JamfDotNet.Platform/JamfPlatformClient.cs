using JamfDotNet.Core;
using JamfDotNet.Core.Authentication;
using JamfDotNet.Platform.Generated.AppInstallerDeployments;
using JamfDotNet.Platform.Generated.AppInstallerSettings;
using JamfDotNet.Platform.Generated.AppInstallerTitles;
using JamfDotNet.Platform.Generated.Blueprints;
using JamfDotNet.Platform.Generated.Compliance;
using JamfDotNet.Platform.Generated.DeclarationReporting;
using JamfDotNet.Platform.Generated.DeviceActions;
using JamfDotNet.Platform.Generated.DeviceGroups;
using JamfDotNet.Platform.Generated.Devices;
using Microsoft.Extensions.Options;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;

namespace JamfDotNet.Platform;

/// <summary>
/// Entry point for Jamf Platform API Gateway services.
/// </summary>
public sealed class JamfPlatformClient : IDisposable
{
    private readonly HttpClient? _ownedHttpClient;
    private bool _disposed;

    /// <summary>Creates a platform client from pre-built service clients.</summary>
    public JamfPlatformClient(
        string tenantId,
        BlueprintsApiClient blueprints,
        DevicesApiClient devices,
        DeviceGroupsApiClient deviceGroups,
        DeviceActionsApiClient deviceActions,
        ComplianceApiClient compliance,
        DeclarationReportingApiClient declarationReporting,
        AppInstallerDeploymentsApiClient appInstallerDeployments,
        AppInstallerSettingsApiClient appInstallerSettings,
        AppInstallerTitlesApiClient appInstallerTitles,
        HttpClient? ownedHttpClient = null)
    {
        TenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        Blueprints = blueprints ?? throw new ArgumentNullException(nameof(blueprints));
        Devices = devices ?? throw new ArgumentNullException(nameof(devices));
        DeviceGroups = deviceGroups ?? throw new ArgumentNullException(nameof(deviceGroups));
        DeviceActions = deviceActions ?? throw new ArgumentNullException(nameof(deviceActions));
        Compliance = compliance ?? throw new ArgumentNullException(nameof(compliance));
        DeclarationReporting = declarationReporting ?? throw new ArgumentNullException(nameof(declarationReporting));
        AppInstallerDeployments = appInstallerDeployments ?? throw new ArgumentNullException(nameof(appInstallerDeployments));
        AppInstallerSettings = appInstallerSettings ?? throw new ArgumentNullException(nameof(appInstallerSettings));
        AppInstallerTitles = appInstallerTitles ?? throw new ArgumentNullException(nameof(appInstallerTitles));
        _ownedHttpClient = ownedHttpClient;
    }

    /// <summary>Tenant id used for <c>/v1/tenant/{tenantId}</c> paths.</summary>
    public string TenantId { get; }

    /// <summary>
    /// Returns tenant-scoped builders for services that require <c>/v1/tenant/{tenantId}</c>,
    /// using <see cref="TenantId"/>.
    /// </summary>
    /// <returns>Tenant scope for the configured tenant.</returns>
    /// <exception cref="FormatException">Thrown when <see cref="TenantId"/> is not a valid <see cref="Guid"/>.</exception>
    public JamfPlatformTenantScope ForTenant() => ForTenant(TenantId);

    /// <summary>
    /// Returns tenant-scoped builders for the given tenant id string.
    /// </summary>
    /// <param name="tenantId">Tenant id (GUID string).</param>
    /// <returns>Tenant scope.</returns>
    public JamfPlatformTenantScope ForTenant(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        return ForTenant(Guid.Parse(tenantId));
    }

    /// <summary>
    /// Returns tenant-scoped builders for the given tenant id.
    /// </summary>
    /// <param name="tenantId">Tenant id.</param>
    /// <returns>Tenant scope.</returns>
    public JamfPlatformTenantScope ForTenant(Guid tenantId) => new(this, tenantId);

    /// <summary>Blueprints service client.</summary>
    public BlueprintsApiClient Blueprints { get; }

    /// <summary>Devices inventory service client.</summary>
    public DevicesApiClient Devices { get; }

    /// <summary>Device groups service client.</summary>
    public DeviceGroupsApiClient DeviceGroups { get; }

    /// <summary>Device management actions service client.</summary>
    public DeviceActionsApiClient DeviceActions { get; }

    /// <summary>Compliance benchmarks service client.</summary>
    public ComplianceApiClient Compliance { get; }

    /// <summary>Declaration (DDM) reporting service client.</summary>
    public DeclarationReportingApiClient DeclarationReporting { get; }

    /// <summary>App Installer deployments service client.</summary>
    public AppInstallerDeploymentsApiClient AppInstallerDeployments { get; }

    /// <summary>App Installer global settings service client.</summary>
    public AppInstallerSettingsApiClient AppInstallerSettings { get; }

    /// <summary>App Installer titles service client.</summary>
    public AppInstallerTitlesApiClient AppInstallerTitles { get; }

    /// <summary>Creates a client from options and authentication.</summary>
    public static JamfPlatformClient Create(
        JamfPlatformOptions options,
        IAuthenticationProvider authenticationProvider,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(authenticationProvider);
        options.Validate();

        var ownsClient = httpClient is null;
        httpClient ??= new HttpClient();

        TClient CreateServiceClient<TClient>(string servicePath, Func<IRequestAdapter, TClient> factory)
        {
            var adapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient)
            {
                BaseUrl = options.GetServiceBaseUrl(servicePath).AbsoluteUri.TrimEnd('/'),
            };
            return factory(adapter);
        }

        return new JamfPlatformClient(
            options.TenantId,
            CreateServiceClient("api/blueprints", static a => new BlueprintsApiClient(a)),
            CreateServiceClient("api/devices", static a => new DevicesApiClient(a)),
            CreateServiceClient("api/device-groups", static a => new DeviceGroupsApiClient(a)),
            CreateServiceClient("api/device-actions", static a => new DeviceActionsApiClient(a)),
            CreateServiceClient("api/compliance-benchmarks", static a => new ComplianceApiClient(a)),
            CreateServiceClient("api/ddm/report", static a => new DeclarationReportingApiClient(a)),
            CreateServiceClient("api/app-installers", static a => new AppInstallerDeploymentsApiClient(a)),
            CreateServiceClient("api/app-installers", static a => new AppInstallerSettingsApiClient(a)),
            CreateServiceClient("api/app-installers", static a => new AppInstallerTitlesApiClient(a)),
            ownsClient ? httpClient : null);
    }

    /// <summary>Creates a client from DI options.</summary>
    public static JamfPlatformClient Create(
        IOptions<JamfPlatformOptions> options,
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
