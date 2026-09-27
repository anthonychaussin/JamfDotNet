# JamfDotNet.Protect

Typed .NET client for the Jamf Protect GraphQL API (`POST {base}/app`).

Not the same as Jamf Pro `/v1/jamf-protect` registration endpoints.

## Quick start

```csharp
using JamfDotNet.Protect.Models;

services.AddJamfProtectClient(o =>
{
    o.BaseUrl = new Uri("https://your.protect.jamfcloud.com");
    o.ClientId = "...";
    o.ClientSecret = "...";
});

var protect = provider.GetRequiredService<JamfProtectClient>();
var roles = await protect.ListRolesAsync();
await foreach (var computer in protect.EnumerateComputersAsync()) { /* … */ }
var alert = await protect.GetAlertAsync(uuid);
var plan = await protect.CreatePlanAsync(new ProtectPlanWriteRequest
{
    Name = "Fleet",
    Description = "Default",
    ActionConfigsId = actionConfigId,
});
using var raw = await protect.ExecuteAsync(query, variables);
```

Typed coverage includes list/get helpers, plan/computer mutations, organization, users/roles, insights, telemetry, and audit logs. Use `ExecuteAsync` for anything not yet modeled.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet) and vendored schema `graphql/jamf-protect.schema.graphql`.
