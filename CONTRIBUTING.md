# Contributing

Thanks for contributing to JamfDotNet.

## Basics

1. Read [AGENTS.md](AGENTS.md) for layout, regen scripts, and conventions.
2. Keep the public API and XML documentation in **English**.
3. Do not commit secrets or live credentials.

## Documentation

- **XML docs** are required on all public hand-written APIs (types and members). Release builds fail on missing docs for Core, Protect, and School (`CS1591`).
- Kiota `Generated/` trees are exempt; do not hand-edit them.
- Update the relevant `src/JamfDotNet.*/README.md` when changing package surface.
- Update the root [README.md](README.md) for cross-cutting behavior (auth, env vars, scripts).

## Build / test / pack

```powershell
dotnet restore JamfDotNet.slnx
dotnet build JamfDotNet.slnx -c Release
dotnet test JamfDotNet.slnx -c Release
dotnet pack JamfDotNet.slnx -c Release
```

Live smoke tests run only when the documented environment variables are set; they must no-op cleanly otherwise.

## Regenerating OpenAPI clients

Use the scripts under `scripts/` (see AGENTS.md). After generation, ensure the solution builds and tests pass.
