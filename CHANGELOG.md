# Changelog

All notable changes to JamfDotNet are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.2.0] - 2026-09-27

Developer-experience and production hardening across all packages: pagination façades, richer errors/resilience, Protect CRUD, Platform device actions, diagnostics, and maintainability polish.

### Added

#### Core
- `JamfRsql` helper for building Jamf Pro RSQL `filter` strings.
- `JamfClientOptions.OAuthScope` for optional OAuth2 role scoping.
- `RemapKiotaExceptions` with `JamfRemappingRequestAdapter` (Pro, Classic, Platform, Title Editor).
- `JamfResilienceOptions` + `ConfigureJamfResilience` (auto-bound from `Jamf:Resilience` by `AddJamf*Client(IConfiguration)` / `AddJamfCore(IConfiguration)`).
- Richer `JamfApiException`: GraphQL `errors[]`, `RetryAfter`, rate-limit headers, `FromHttpResponse` / `FromGraphQlErrors`.
- `JamfDiagnostics.ActivitySource` (`JamfDotNet`) + optional `ILogger` on `JamfResilienceHandler` (retry / token-refresh events).
- Basic→bearer `InvalidateAsync` best-effort call to `/api/v1/auth/invalidate-token`.
- Shared `JamfOAuthClientCredentials` helper for Platform/Protect token acquisition.

#### Pro
- `Enumerate*` pagination helpers: computers inventory, mobile devices (summary + detail), scripts, packages, users, categories, patch policies, computer/mobile groups, buildings, departments, sites, extension attributes, computer prestages, smart/static computer groups.
- MDM wrappers: clear passcode, lost mode, update inventory (`DEVICE_INFORMATION`), and related command helpers.

#### Protect
- Typed plans / computers / groups / users / roles surfaces.
- Config CRUD: PreventList, ActionConfig, AnalyticSet, ExceptionSet.
- List filters for alerts/computers; counts; audit logs.
- Cursor `Enumerate*` helpers for roles, plans, analytic sets, computers, USB control sets, prevent lists, alerts, groups, API clients, action configs, exception sets, users, telemetries v2, audit logs by date.
- Single-parse GraphQL pipeline (`SendGraphQlAsync` returns one `JsonDocument`).

#### Platform
- `Enumerate*` for devices, device groups, blueprints, compliance benchmarks, app-installer deployments, declaration devices.
- Device action helpers: `RestartDeviceAsync`, `ShutDownDeviceAsync`, `EraseDeviceAsync`, `CheckInDeviceAsync`.
- Live smoke uses `EnumerateDevicesAsync` when secrets are configured.

#### Classic
- `JamfClassicXml` + create/update helpers for categories and buildings (Pro-first for new work).

#### School
- CRUD writes; additional MDM commands.
- Convenience `Enumerate*Async` over single-page list endpoints (documented as single-page).

#### Tooling / tests
- Optional CI `live-smoke` job (manual / tag) gated on repository secrets.
- Unit tests: Core remapping/exceptions/resilience; Protect CRUD + GraphQL errors; Platform enumerate/device actions; School/Classic XML; Pro MDM argument checks.

### Changed
- Package READMEs document remap, pagination, MDM, Classic XML, Platform device actions, Protect `Enumerate*` table, and resilience config binding.
- Pro `Enumerate*` helpers share internal page/filter plumbing.
- Platform/Protect OAuth token providers share form-post logic.

## [1.0.1] - 2026-08-09

### Fixed
- Patch release over `1.0.0` (see GitHub release notes).

## [1.0.0] - 2026-07-26

### Added
- Initial multi-package SDK published to NuGet: Core, Pro, Classic, Platform, Protect, School, Title Editor.
- Kiota-generated Pro / Classic / Platform / Title Editor surfaces.
- Hand-written Protect GraphQL and School REST clients with escape hatches.
- Shared auth, resilience, pagination primitives, and DI registration helpers.
