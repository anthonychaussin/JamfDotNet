# JamfDotNet.Pro

Strongly typed .NET client for the Jamf Pro API (`/api`), generated with Kiota.

## Quick start

```csharp
services.AddJamfProClient(options =>
{
    options.BaseUrl = new Uri("https://your.jamfcloud.com");
    options.ClientId = "...";
    options.ClientSecret = "...";
    // options.OAuthScope = "role:computers.read";
    // options.RemapKiotaExceptions = true;
});

var jamf = provider.GetRequiredService<JamfProClient>();
var inventory = await jamf.Api.V1.ComputersInventory.GetAsync();

var filter = JamfRsql.And(JamfRsql.Eq("general.name", "Mac*"));
await foreach (var computer in jamf.EnumerateComputersInventoryAsync(filter: filter)) { /* … */ }

await jamf.RestartDevicesAsync(["management-id"]);
await foreach (var policy in jamf.EnumeratePatchPoliciesAsync()) { /* … */ }
```

## Notes

- Full OpenAPI surface is available via `client.Api…`.
- DX helpers: pagination (`Enumerate*Async`), `JamfRsql`, MDM wrappers (`RestartDevicesAsync`, `LockDevicesAsync`, …), optional `RemapKiotaExceptions`.
- Targets `net8.0` and `net10.0` in one NuGet package.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet) for schema fetch/regen scripts.
