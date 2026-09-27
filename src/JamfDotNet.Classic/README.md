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

// Prefer Jamf Pro for new work. Classic writes need XML:
await classic.CreateCategoryAsync("Apps", priority: 9);
await classic.CreateBuildingAsync("HQ", city: "Cupertino");
// or: await using var xml = JamfClassicXml.ToStream(JamfClassicXml.Category("Apps"));
```

## Notes

- Full Classic surface is available via `client.Api…`.
- Optional `RemapKiotaExceptions` remaps Kiota `ApiException` to `JamfApiException`.
- XML helpers: `JamfClassicXml` + `CreateCategoryAsync` / `CreateBuildingAsync` (and updates). Prefer Jamf Pro when an equivalent exists.
- Targets `net8.0` and `net10.0` in one NuGet package.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
