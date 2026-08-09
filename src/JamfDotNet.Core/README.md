# JamfDotNet.Core

Shared authentication, options, HTTP helpers, and DI primitives used by all JamfDotNet clients.

## Features

- OAuth2 / basic→bearer token providers for Jamf Pro instances
- Platform, Protect, School, and Title Editor options + token providers
- `BearerAuthHandler` / `BasicAuthHandler` / `JamfResilienceHandler` (429/503 + 401 refresh)
- `JamfApiException`, Kiota remapping via `AsJamfApiAsync()`, and `JamfPagination`
- `JamfDeviceClassifier` — map Apple `modelIdentifier` + Jamf type/platform to iPhone / iPad / Mac / Apple TV / …
- Named `HttpClient` constants and `AddJamfCore(IConfiguration)`

## Install

```bash
dotnet add package JamfDotNet.Core
```

Usually referenced transitively via a product package (`JamfDotNet.Pro`, `JamfDotNet.Protect`, …).

XML documentation for public APIs is included in the package (IntelliSense).

See the [repository README](https://github.com/anthonychaussin/JamfDotNet) and [AGENTS.md](https://github.com/anthonychaussin/JamfDotNet/blob/main/AGENTS.md).
