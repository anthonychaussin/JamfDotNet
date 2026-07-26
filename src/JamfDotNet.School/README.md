# JamfDotNet.School

Typed .NET client for the Jamf School REST API.

Authentication: Basic (`NetworkId`:`ApiKey`) plus `X-Server-Protocol-Version` (default `3`).

## Quick start

```csharp
services.AddJamfSchoolClient(o =>
{
    o.BaseUrl = new Uri("https://your.jamfcloud.com");
    o.NetworkId = "...";
    o.ApiKey = "...";
});

var school = provider.GetRequiredService<JamfSchoolClient>();
var devices = await school.Devices.ListAsync();
```

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
