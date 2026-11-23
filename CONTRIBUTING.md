# Contributing to PRISM

Thanks for taking the time to improve PRISM. This project is a native Windows/WPF application, so please keep changes small, readable and testable.

## Before opening a pull request

1. Search existing issues and pull requests.
2. Open an issue for a larger feature or a behavior change so the approach can be discussed first.
3. Keep the change focused. One logical change per pull request is easiest to review.
4. Do not include build output, local settings, screenshots with personal data or binaries in a pull request.

## Development setup

Requirements:

- Windows 10/11 x64
- .NET 9 SDK
- Visual Studio 2022/2026 or Rider (optional)
- Inno Setup 6 (only for installer changes)

```powershell
dotnet restore PRISM.sln
dotnet build PRISM.sln -c Release
dotnet run --project App/PrismCrosshair.csproj
```

The application writes its local settings to `%APPDATA%\PrismCrosshair`. Delete that folder to start with a clean profile.

## Code guidelines

- Keep the renderer and overlay free of unnecessary allocations; this app is intentionally light.
- Preserve click-through, always-on-top and no-injection behavior.
- Do not add telemetry, background services or network calls.
- Use English for code, comments, logs, commit messages and source-language UI strings.
- Do not hardcode colors or spacing in pages when a shared resource exists.
- Any new user-facing string must be translatable. See [docs/LOCALIZATION.md](docs/LOCALIZATION.md).

## Localization changes

- English is the source language.
- Add new strings in English, then update `App/Localization/ru.json`.
- Keep placeholders (`{0}`, `{1}`) and exact keys intact.
- Test all six pages in both languages before submitting.

## Testing

Before submitting:

1. `dotnet build PRISM.sln -c Release` finishes with zero warnings and errors.
2. The portable build launches in English by default.
3. Switching to Russian and back updates the whole UI.
4. Overlay toggle (`F8`), next preset (`F9`), editor (`F10`) and panic hide (`F12`) work.
5. The palette opens and selecting a swatch updates the reticle color.
6. Saving, deleting and resetting presets still persists after restart.

## Commit messages

Use a short imperative subject, for example:

```text
Add German localization
Fix palette popup clipping on small screens
```

## License

By contributing, you agree that your contribution is licensed under the project's MIT license.
