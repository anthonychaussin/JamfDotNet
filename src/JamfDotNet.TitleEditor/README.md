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

Prefer regenerating from an instance schema when `JAMF_TITLE_EDITOR_URL` is available.

See the [repository README](https://github.com/anthonychaussin/JamfDotNet).
