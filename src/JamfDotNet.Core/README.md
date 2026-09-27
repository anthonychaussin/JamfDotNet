# JamfDotNet.Core

Shared authentication, options, HTTP helpers, and DI primitives used by all JamfDotNet clients.

## Features

- OAuth2 / basic→bearer token providers for Jamf Pro instances (optional `OAuthScope`, invalidate/revoke)
- Platform, Protect, School, and Title Editor options + token providers
- `BearerAuthHandler` / `BasicAuthHandler` / `JamfResilienceHandler` (429/503 + 401 refresh; tunable via `JamfResilienceOptions`; `ActivitySource` `JamfDotNet` + optional `ILogger`)
- `AddJamf*Client(IConfiguration)` and `AddJamfCore(IConfiguration)` automatically bind `Jamf:Resilience` via `ConfigureJamfResilience` (or call it yourself when using the `Action<>` overloads)
- `JamfApiException` (GraphQL errors, `RetryAfter`, rate-limit headers), Kiota remapping via `AsJamfApiAsync()` or `RemapKiotaExceptions`
- `JamfPagination` and `JamfRsql`
- `JamfDeviceClassifier` — map Apple `modelIdentifier` + Jamf type/platform to iPhone / iPad / Mac / Apple TV / …
- Named `HttpClient` constants and `AddJamfCore(IConfiguration)`
- Native AOT / trimming is **not** claimed yet

### Tracing

Register the Jamf activity source with your OpenTelemetry pipeline (no OpenTelemetry package is required by this SDK):

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddSource(JamfDotNet.Core.Diagnostics.JamfDiagnostics.ActivitySourceName));
```
## Install

```bash
dotnet add package JamfDotNet.Core
```

Usually referenced transitively via a product package (`JamfDotNet.Pro`, `JamfDotNet.Protect`, …).

XML documentation for public APIs is included in the package (IntelliSense).

See the [repository README](https://github.com/anthonychaussin/JamfDotNet) and [AGENTS.md](https://github.com/anthonychaussin/JamfDotNet/blob/main/AGENTS.md).
