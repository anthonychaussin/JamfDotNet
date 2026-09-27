# Changelog

All notable changes to JamfDotNet are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **Core:** `JamfRsql` helper for building Jamf Pro RSQL `filter` strings.
- **Core:** `JamfClientOptions.OAuthScope` for optional OAuth2 role scoping.
- **Core:** `JamfClientOptions.RemapKiotaExceptions` with `JamfRemappingRequestAdapter` for Pro/Classic.
- **Core:** richer `JamfApiException.FromApiException` body extraction + explicit body overload.
- **Core:** Basic→bearer `InvalidateAsync` now best-effort calls `/api/v1/auth/invalidate-token`.
- **Pro:** pagination helpers for mobile devices, scripts, packages, users, and categories.
- **Protect:** typed plans/computers mutations, organization, users/roles CRUD, insights, telemetry v2, audit logs, alert status counts + matching `Enumerate*` helpers.
- **Platform:** `EnumerateDevicesAsync` / `EnumerateDeviceGroupsAsync` / `EnumerateBlueprintsAsync`; live smoke lists tenant devices.
- **School:** CRUD writes for groups/classes/profiles/apps/locations; additional MDM commands (`shutdown`, `blankpush`, `updateinventory`, lost mode, `SendCommandAsync`).
- **Tests:** dedicated `JamfDotNet.Core.Tests` project.
- Optional CI `live-smoke` job (manual / tag) gated on repository secrets.

### Changed

- Platform / School / Title Editor package READMEs document beta gateway, School pagination limits, and Title Editor bootstrap schema regeneration.

## [0.1.0] - 2026-09-27

### Added

- Initial multi-package SDK: Core, Pro, Classic, Platform, Protect, School, Title Editor.
- Kiota-generated Pro / Classic / Platform / Title Editor surfaces.
- Hand-written Protect GraphQL and School REST clients with escape hatches.
- Shared auth, resilience, pagination primitives, and DI registration helpers.
