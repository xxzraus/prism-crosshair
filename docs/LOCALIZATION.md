# Localization

PRISM uses a small runtime string catalog. English is the source language and the default UI language. Russian is included as a complete translation.

## How it works

- Every user-facing string in XAML and C# is written in English.
- `App/Localization/ru.json` maps exact English strings to Russian strings.
- `Loc.T("English text")` translates a dynamic string and returns the current language.
- `Loc.ApplyTo(visual)` translates static XAML controls after a page or window loads and when the language changes.
- `Loc` remembers the original English text, so switching back to English restores it.
- The selected language is stored in `settings.json` as `preferences.language`.

`ru.json` is embedded into the assembly. English needs no file: the source string is the fallback.

## Add a language

1. Copy `App/Localization/ru.json` to `App/Localization/de.json` (for German).
2. Translate every value. Keep `{0}`, `{1}` placeholders and exact keys unchanged.
3. Add the file to `App/PrismCrosshair.csproj`:

```xml
<EmbeddedResource Include="Localization\de.json" />
```

4. Add the language to `Loc.SetLanguage` and the `ComboBox` in `SettingsPage.xaml`.
5. Add the display name to the language selector.

## Rules for translators

- Do not translate product names: **PRISM**, **Crosshair Studio**.
- Keep hotkey names and technical tokens (`F8`, `WPF`, `HEX`, `DPI`) unchanged.
- Preserve trailing spaces, punctuation and em dashes.
- Keep placeholders like `{0}` and `{1}` exactly as they appear.
- The key is the full English string, including punctuation. If the key changes, the translation stops matching.

## Testing

1. Set the language in **Settings → Language**.
2. Visit every page: Crosshairs, Editor, Overlay, Hotkeys, Settings, About.
3. Open the color palette and every dropdown.
4. Trigger a toast (save a preset, reset settings, toggle autostart).
5. Switch back to English and restart the app to verify persistence.

The build should stay warning-free:

```powershell
dotnet build PRISM.sln -c Release
```
