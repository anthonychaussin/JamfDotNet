using JamfDotNet.Platform.Generated.Blueprints.V1.Tenant.Item;

namespace JamfDotNet.Platform;

/// <summary>
/// Tenant-scoped request builders for Platform services that use <c>/v1/tenant/{tenantId}</c>.
/// </summary>
/// <remarks>
/// App Installer services do not use a tenant path segment; access them via
/// <see cref="JamfPlatformClient.AppInstallerDeployments"/>, <see cref="JamfPlatformClient.AppInstallerSettings"/>,
/// and <see cref="JamfPlatformClient.AppInstallerTitles"/>.
/// </remarks>
public sealed class JamfPlatformTenantScope
{
    private readonly JamfPlatformClient _client;
    private readonly Guid _tenantId;

    internal JamfPlatformTenantScope(JamfPlatformClient client, Guid tenantId)
    {
        _client = client;
        _tenantId = tenantId;
    }

    /// <summary>Tenant id used for path segments.</summary>
    public Guid TenantId => _tenantId;

    /// <summary>Blueprints tenant root.</summary>
    public WithTenantItemRequestBuilder Blueprints => _client.Blueprints.V1.Tenant[_tenantId];

    /// <summary>Devices inventory tenant root.</summary>
    public Generated.Devices.V1.Tenant.Item.WithTenantItemRequestBuilder Devices =>
        _client.Devices.V1.Tenant[_tenantId];

    /// <summary>Device groups tenant root.</summary>
    public Generated.DeviceGroups.V1.Tenant.Item.WithTenantItemRequestBuilder DeviceGroups =>
        _client.DeviceGroups.V1.Tenant[_tenantId];

    /// <summary>Device management actions tenant root.</summary>
    public Generated.DeviceActions.V1.Tenant.Item.WithTenantItemRequestBuilder DeviceActions =>
        _client.DeviceActions.V1.Tenant[_tenantId];

    /// <summary>Compliance benchmarks tenant root.</summary>
    public Generated.Compliance.V1.Tenant.Item.WithTenantItemRequestBuilder Compliance =>
        _client.Compliance.V1.Tenant[_tenantId];

    /// <summary>Declaration (DDM) reporting tenant root.</summary>
    public Generated.DeclarationReporting.V1.Tenant.Item.WithTenantItemRequestBuilder DeclarationReporting =>
        _client.DeclarationReporting.V1.Tenant[_tenantId];
}
