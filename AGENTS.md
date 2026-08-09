# Agent guide — JamfDotNet

English-only public API and documentation. Multi-package .NET SDK for Jamf APIs.

## Layout

| Path | Purpose |
|------|---------|
| `src/JamfDotNet.Core` | Shared options, auth handlers, exceptions, DI helpers |
| `src/JamfDotNet.Pro` | Jamf Pro API (Kiota + façade) |
| `src/JamfDotNet.Classic` | Classic API (Kiota + façade) |
| `src/JamfDotNet.Platform` | Platform API Gateway (multi-spec Kiota) |
| `src/JamfDotNet.Protect` | Protect GraphQL typed client + `ExecuteAsync` |
| `src/JamfDotNet.School` | School REST typed client |
| `src/JamfDotNet.TitleEditor` | Title Editor API (Kiota + bearer keepalive) |
| `tests/JamfDotNet.*.Tests` | Unit + optional live smoke tests |
| `openapi/` | OpenAPI snapshots |
| `graphql/jamf-protect.schema.graphql` | Protect GraphQL schema |
| `scripts/` | Fetch / assemble schema + Kiota regen |

## XML documentation (required)

- Every **public hand-written** type and member must have English XML docs (`<summary>`, plus `<param>` / `<returns>` / `<exception>` when useful).
- `GenerateDocumentationFile` is enabled globally; XML files ship in NuGet packages.
- **Core / Protect / School**: do not suppress `CS1591` — Release treats missing docs as errors.
- **Pro / Classic / Platform / TitleEditor**: `CS1591` may be suppressed for `Generated/` only; façades (`Jamf*Client`, `AddJamf*Client`) remain documented.
- Never hand-edit `**/Generated/**`.

## Regenerating clients

```powershell
./scripts/Fetch-JamfProSchema.ps1
./scripts/Generate-JamfProClient.ps1

./scripts/Fetch-JamfClassicSchema.ps1
./scripts/Generate-JamfClassicClient.ps1

./scripts/Fetch-JamfPlatformSpecs.ps1
./scripts/Generate-JamfPlatformClient.ps1

./scripts/Assemble-JamfTitleEditorSchema.ps1   # or Fetch when JAMF_TITLE_EDITOR_URL is set
./scripts/Generate-JamfTitleEditorClient.ps1

./scripts/Fetch-JamfSchoolSchema.ps1           # optional; School stays hand-written
```

Do **not** regenerate Pro/Classic from Platform monolith OpenAPI specs.

## Protect maintenance

Protect is hand-written against `graphql/jamf-protect.schema.graphql`. Add typed operations next to existing `List*Async` / `Get*Async` methods, keep `Enumerate*Async` pagination helpers in sync, and keep `ExecuteAsync` as the escape hatch. Update models under `src/JamfDotNet.Protect/Models/`.

## DX helpers (Core)

- `JamfPagination` — cursor and page/page-size `IAsyncEnumerable` helpers
- `JamfApiException.FromApiException` / `AsJamfApiAsync()` — unify Kiota errors
- `JamfResilienceHandler` — 429/503 retries + one 401 token refresh
- `AddJamf*Client(IConfiguration)` — appsettings binding via each options `SectionName`

## Packaging

- One `.nupkg` per product with `lib/net8.0` and `lib/net10.0`.
- Package README lives under each `src/JamfDotNet.*/README.md`.
- Native AOT / trimming is **not** claimed yet (reflection-based `System.Text.Json` + Kiota Generated). Document any future source-gen work in PRs.

## Build & test

```powershell
dotnet restore JamfDotNet.slnx
dotnet build JamfDotNet.slnx -c Release
dotnet test JamfDotNet.slnx -c Release
dotnet pack JamfDotNet.slnx -c Release
```

Live smoke env vars: see root README.

## Conventions

- Prefer file-scoped namespaces, nullable reference types, XML docs on public members.
- Shared auth/HTTP in Core; product façades in product packages.
- Protect GraphQL ≠ Pro `/v1/jamf-protect`.
- Do not commit secrets.
