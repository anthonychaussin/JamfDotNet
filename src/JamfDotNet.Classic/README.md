# JamfDotNet.Classic

Strongly typed .NET client for the Jamf Classic API (`/JSSResource`), generated with Kiota.

## Quick start

```csharp
services.AddJamfClassicClient(options =>
{
    options.BaseUrl = new Uri("https://your.jamfcloud.com");
    options.ClientId = "...";
    options.ClientSecret = "...";
    // options.RemapKiotaExceptions = true;
});

var classic = provider.GetRequiredService<JamfClassicClient>();
var buildings = await classic.Api.Buildings.GetAsync();
```

## Notes

- Full Classic surface is available via `client.Api…`.
- Optional `RemapKiotaExceptions` remaps Kiota `ApiException` to `JamfApiException`.
- Classic writes often require XML bodies. Prefer Jamf Pro API for new work when an equivalent exists.
- Targets `net8.0` and `net10.0` in one NuGet package.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
