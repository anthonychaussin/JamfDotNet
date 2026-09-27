# Changelog

All notable changes to JamfDotNet are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Core:** `JamfRsql` helper for building Jamf Pro RSQL `filter` strings.
- **Core:** `JamfClientOptions.OAuthScope` for optional OAuth2 role scoping.
- **Core:** `RemapKiotaExceptions` with `JamfRemappingRequestAdapter` for Pro, Classic, Platform, and Title Editor.
- **Core:** `JamfResilienceOptions` + `ConfigureJamfResilience`; richer `JamfApiException` (GraphQL `errors[]`, `RetryAfter`, rate-limit headers, `FromHttpResponse` / `FromGraphQlErrors`).
- **Core:** `JamfDiagnostics.ActivitySource` (`JamfDotNet`) + optional `ILogger` on `JamfResilienceHandler` (retry / token-refresh events).
- **Core:** Basic→bearer `InvalidateAsync` now best-effort calls `/api/v1/auth/invalidate-token`.
- **Pro:** pagination helpers for inventory, mobile devices, scripts, packages, users, categories, patch policies, computer/mobile groups, buildings, departments, sites, extension attributes, computer prestages, smart/static computer groups; MDM wrappers including clear passcode, lost mode, update inventory (`DEVICE_INFORMATION`).
- **Protect:** typed plans/computers/groups/users/roles, config CRUD (PreventList, ActionConfig, AnalyticSet, ExceptionSet), list filters for alerts/computers, counts, audit logs + `Enumerate*`.
- **Platform:** device/group/blueprint/benchmark/app-installer/declaration `Enumerate*`; device action helpers (`RestartDeviceAsync`, …); live smoke uses `EnumerateDevicesAsync`.
- **Classic:** `JamfClassicXml` + create/update helpers for categories and buildings (Pro-first for new work).
- **School:** CRUD writes; additional MDM commands; convenience `Enumerate*Async` over single-page list endpoints.
- **Tests:** Core remapping/exception/resilience coverage; Protect/School/Classic XML; Pro MDM argument checks.
- Optional CI `live-smoke` job (manual / tag) gated on repository secrets.

### Changed

- Package READMEs document remap, pagination, MDM, Classic XML, and Platform device actions.

## [0.1.0] - 2026-09-27

### Added

- Initial multi-package SDK: Core, Pro, Classic, Platform, Protect, School, Title Editor.
- Kiota-generated Pro / Classic / Platform / Title Editor surfaces.
- Hand-written Protect GraphQL and School REST clients with escape hatches.
- Shared auth, resilience, pagination primitives, and DI registration helpers.
