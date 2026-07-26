# JamfDotNet.Platform

Strongly typed .NET client for the Jamf Platform API Gateway (`*.apigw.jamf.com`): blueprints, devices, groups, compliance, DDM reporting, device actions, and app installers.

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
// platform.Blueprints, platform.Devices, platform.Compliance, ...
```

This package does **not** replace `JamfDotNet.Pro` / `JamfDotNet.Classic`.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
