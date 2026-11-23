# Changelog

All notable changes to PRISM are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project uses [Semantic Versioning](https://semver.org/).

## [1.1.0] — 2026-11-23

### Added

- English interface as the default language.
- Russian localization with an in-app language selector.
- 48-color palette with HEX input and tooltips.
- Live screen preview while dragging settings sliders.
- Portable ZIP and Inno Setup installer builds.
- Project documentation and screenshots.
- GitHub-ready repository: MIT license, CI workflow, issue templates and contribution/security guides.

### Changed

- Reworked the project structure into `App`, `docs`, `Installer` and `Portable`.
- Improved the palette popup layout.
- Refined theme surfaces and default colors.
- Reticles are white by default and the second color is optional.
- Preset and profile changes are saved immediately.

### Fixed

- Preset selection is persisted correctly.
- Scale control now affects the rendered reticle.
- Autostart toggle writes the correct registry state.
- Live preview restores the studio after a slider drag.

## [1.0.0] — 2026-09-01

### Added

- Initial PRISM Crosshair Studio release.
- Native WPF overlay with click-through and always-on-top behavior.
- Reticle editor with 8 shapes, colors, glow, outline and animation.
- Built-in presets, profiles and global hotkeys.
