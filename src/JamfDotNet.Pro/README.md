# JamfDotNet.Pro

Strongly typed .NET client for the Jamf Pro API (`/api`), generated with Kiota.

## Quick start

```csharp
services.AddJamfProClient(options =>
{
    options.BaseUrl = new Uri("https://your.jamfcloud.com");
    options.ClientId = "...";
    options.ClientSecret = "...";
});

var jamf = provider.GetRequiredService<JamfProClient>();
var inventory = await jamf.Api.V1.ComputersInventory.GetAsync();
```

Targets `net8.0` and `net10.0` in one NuGet package.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet) for schema fetch/regen scripts.
