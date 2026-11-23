# Publishing PRISM on GitHub

This repository is ready for a public GitHub release. The source, documentation, CI workflow, issue templates and license are included; build artifacts are intentionally ignored.

## 1. Create the repository

1. Create a new empty repository on GitHub, for example `prism-crosshair`.
2. Do **not** initialize it with a README, license or `.gitignore` — all three exist here.
3. Add the remote and push:

```powershell
cd PRISM
git init
git add .
git commit -m "Initial PRISM 1.1.0 release"
git branch -M main
git remote add origin https://github.com/<your-name>/prism-crosshair.git
git push -u origin main
```

`Portable/*`, `Installer/*.exe`, `App/bin` and `App/obj` are ignored, so the push contains source and docs only.

## 2. Repository settings

Suggested description:

> A lightweight native Windows crosshair overlay with a 48-color palette, profiles and English/Russian UI.

Suggested topics:

`windows`, `wpf`, `csharp`, `dotnet`, `crosshair`, `overlay`, `gaming`, `localization`

Enable **Actions** so `.github/workflows/build.yml` runs on pushes and pull requests.

## 3. Create a release

Attach the built binaries manually to a GitHub Release:

- `Portable/PRISM-Portable-1.1.0.zip`
- `Installer/PRISM-Setup-1.1.0.exe`

Tag format: `v1.1.0`. Release notes can reuse the matching `CHANGELOG.md` section.

## 4. Replace the clone placeholder

The root `README.md` contains `<repository-url>` in the build section. Replace it with your repository URL after creating the repository.

## 5. Before publishing

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
