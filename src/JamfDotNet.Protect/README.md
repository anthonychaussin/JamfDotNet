# JamfDotNet.Protect

Typed .NET client for the Jamf Protect GraphQL API (`POST {base}/app`).

Not the same as Jamf Pro `/v1/jamf-protect` registration endpoints.

## Quick start

```csharp
services.AddJamfProtectClient(o =>
{
    o.BaseUrl = new Uri("https://your.protect.jamfcloud.com");
    o.ClientId = "...";
    o.ClientSecret = "...";
});

var protect = provider.GetRequiredService<JamfProtectClient>();
var roles = await protect.ListRolesAsync();
using var raw = await protect.ExecuteAsync(query, variables);
```

See the [repository README](https://github.com/loky974587/JamfDotNet) and vendored schema `graphql/jamf-protect.schema.graphql`.
