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
await school.Devices.RestartAsync(udid);
await school.Users.CreateAsync(new SchoolUserWriteRequest { Username = "alice" });
```

## Notes

- Typed reads cover devices, groups, users, classes, profiles, apps, and locations.
- Writes are available on those resources; use `SendDocumentAsync` for endpoints not yet modeled.
- School list endpoints generally return a single page (no cursor/page helpers). Prefer server-side filters when the instance supports them, or `GetDocumentAsync` for raw payloads.
- Optional OpenAPI fetch: `./scripts/Fetch-JamfSchoolSchema.ps1` (client remains hand-written).

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
