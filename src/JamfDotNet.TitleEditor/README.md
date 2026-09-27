# JamfDotNet.TitleEditor

Strongly typed .NET client for the Jamf Title Editor API (`/v2`), generated with Kiota.

Authentication: Basic → bearer (`/auth/token`) with keepalive (~15 minute TTL).

## Quick start

```csharp
services.AddJamfTitleEditorClient(o =>
{
    o.BaseUrl = new Uri("https://your.appcatalog.jamfcloud.com");
    o.Username = "...";
    o.Password = "...";
});

var titles = provider.GetRequiredService<JamfTitleEditorClient>();
var list = await titles.Api.Softwaretitles.GetAsync();
```

## Schema regeneration

The committed snapshot under `openapi/jamf-title-editor.schema.json` is a **bootstrap** assembled from public reference pages (auth, softwaretitles, sources, users, patches, preferences). It is intentionally smaller than a full instance export.

Prefer regenerating from a live Title Editor instance before shipping production integrations:

```powershell
$env:JAMF_TITLE_EDITOR_URL = "https://your.appcatalog.jamfcloud.com"
./scripts/Fetch-JamfTitleEditorSchema.ps1
# or: ./scripts/Assemble-JamfTitleEditorSchema.ps1  # delegates to Fetch when the URL is set
./scripts/Generate-JamfTitleEditorClient.ps1
```

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
