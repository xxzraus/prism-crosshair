# Publishing PRISM on GitHub

PRISM is live at **[github.com/xxzraus/prism-crosshair](https://github.com/xxzraus/prism-crosshair)**. The source, documentation, CI workflow, issue templates, funding link and license are in the repository; build artifacts are intentionally ignored.

## 1. Push updates

```powershell
git clone https://github.com/xxzraus/prism-crosshair.git
cd prism-crosshair

git add .
git commit -m "Describe the change"
git push origin main
```

`Portable/*`, `Installer/*.exe`, `App/bin` and `App/obj` are ignored, so the repository contains source and docs only.

## 2. Repository settings

Suggested description:

> A lightweight native Windows crosshair overlay with a 48-color palette, profiles and English/Russian UI.

Suggested topics:

`windows`, `wpf`, `csharp`, `dotnet`, `crosshair`, `overlay`, `gaming`, `localization`

**Actions** is enabled and `.github/workflows/build.yml` runs on pushes and pull requests. Dependabot keeps the workflow actions up to date.

## 3. Create a release

Attach the built binaries manually to a GitHub Release:

- `Portable/PRISM-Portable-1.1.0.zip`
- `Installer/PRISM-Setup-1.1.0.exe`

Tag format: `v1.1.0`. Release notes can reuse the matching `CHANGELOG.md` section. The live release is at [releases/tag/v1.1.0](https://github.com/xxzraus/prism-crosshair/releases/tag/v1.1.0).

## 4. Before publishing

- [ ] `dotnet build PRISM.sln -c Release -warnaserror` passes.
- [ ] English and Russian UI checked.
- [ ] Portable build launches and is self-contained.
- [ ] Installer installs and uninstalls cleanly.
- [ ] No personal paths, settings, logs or binaries are committed.
- [ ] `LICENSE`, `SECURITY.md` and `CODE_OF_CONDUCT.md` are present.
- [ ] Release assets are uploaded.

## Notes

- The app is per-user and does not require elevation.
- The portable folder is self-contained; do not switch to single-file publishing without rechecking memory use.
- Screenshots live in `docs/screenshots` and are referenced by the README.
