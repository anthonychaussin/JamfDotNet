# JamfDotNet.Classic

Strongly typed .NET client for the Jamf Classic API (`/JSSResource`), generated with Kiota.

## Quick start

```csharp
services.AddJamfClassicClient(options =>
{
    options.BaseUrl = new Uri("https://your.jamfcloud.com");
    options.ClientId = "...";
    options.ClientSecret = "...";
});

var classic = provider.GetRequiredService<JamfClassicClient>();
var buildings = await classic.Api.Buildings.GetAsync();
```

Classic writes often require XML bodies. Prefer Jamf Pro API for new work when an equivalent exists.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
