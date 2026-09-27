# JamfDotNet.Platform

Strongly typed .NET client for the Jamf Platform API Gateway (`*.apigw.jamf.com`): blueprints, devices, groups, compliance, DDM reporting, device actions, and app installers.

> **Beta surface:** the Platform API Gateway is documented by Jamf as a beta/unified entry point.
> Permissions are scoped per product (for example `read:pro:blueprints`). This package does **not** replace `JamfDotNet.Pro` / `JamfDotNet.Classic`.

## Quick start

```csharp
services.AddJamfPlatformClient(o =>
{
    o.GatewayBaseUrl = new Uri("https://us.apigw.jamf.com");
    o.ClientId = "...";
    o.ClientSecret = "...";
    o.TenantId = "...";
});

var platform = provider.GetRequiredService<JamfPlatformClient>();
var tenant = platform.ForTenant();
// tenant.Blueprints, tenant.Devices, tenant.Compliance, ...

await foreach (var device in platform.EnumerateDevicesAsync(pageSize: 100))
{
    // …
}
```

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
