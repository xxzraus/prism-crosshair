using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using PrismCrosshair.Models;
using PrismCrosshair.Localization;
using PrismCrosshair.Services;

namespace PrismCrosshair.Models;

public sealed class AppState : ObservableObject
{
    private static readonly Lazy<AppState> Lazy = new(() => new AppState());
    public static AppState Instance => Lazy.Value;

    private readonly DispatcherTimer _saveTimer;
    private CrosshairSettings _current = new();
    private string _activePresetId = "builtin-aurora";
    private bool _isDirty;
    private bool _applyingPreset;
    private string _memoryText = "—";
    private string _cpuText = "0.0%";
    private bool _isStudioActive;

    public ObservableCollection<CrosshairPreset> Presets { get; } = new();
    public AppPreferences Preferences { get; }
    public HotkeyConfig Hotkeys { get; }

    public event EventHandler? OverlayChanged;
    public event EventHandler? HotkeysChanged;
    public event Action<string>? ToastRequested;

    public CrosshairSettings Current
    {
        get => _current;
        private set
        {
            if (ReferenceEquals(_current, value))
                return;
            _current.PropertyChanged -= Current_PropertyChanged;
            _current = value;
            _current.PropertyChanged += Current_PropertyChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ActivePreset));
            OnPropertyChanged(nameof(CurrentName));
            OverlayChanged?.Invoke(this, EventArgs.Empty);
            RequestSave();
        }
    }

    public string ActivePresetId
    {
        get => _activePresetId;
        private set
        {
            if (Set(ref _activePresetId, value))
                OnPropertyChanged(nameof(ActivePreset));
        }
    }

    public CrosshairPreset? ActivePreset => Presets.FirstOrDefault(p => p.Id == ActivePresetId);

    public bool IsDirty
    {
        get => _isDirty;
        private set => Set(ref _isDirty, value);
    }

    public string MemoryText { get => _memoryText; set => Set(ref _memoryText, value); }
    public string CpuText { get => _cpuText; set => Set(ref _cpuText, value); }
    public bool IsStudioActive { get => _isStudioActive; set => Set(ref _isStudioActive, value); }
    public string CurrentName => string.IsNullOrWhiteSpace(Current.Name) ? Loc.T("Untitled") : Current.Name;

    private AppState()
    {
        var persisted = SettingsStore.Load();
        Preferences = persisted.Preferences;
        if (ThemeService.Themes.All(t => !t.Key.Equals(Preferences.Accent, StringComparison.OrdinalIgnoreCase)))
            Preferences.Accent = "Graphite";
        Hotkeys = persisted.Hotkeys;
        ActivePresetId = persisted.ActivePresetId;

        foreach (var preset in PresetLibrary.CreateBuiltIns())
            Presets.Add(preset);

        foreach (var profile in persisted.Profiles)
        {
            if (string.IsNullOrWhiteSpace(profile.Id))
                profile.Id = Guid.NewGuid().ToString("N");
            profile.IsUser = true;
            Presets.Add(profile);
        }

        _current = persisted.Current ?? (Presets.FirstOrDefault(p => p.Id == ActivePresetId)?.Settings.Clone() ?? new CrosshairSettings());
        _current.PropertyChanged += Current_PropertyChanged;

        if (_current.Name == "Custom crosshair" && string.IsNullOrWhiteSpace(persisted.Current?.Name))
            _current.Name = "My crosshair";

        Preferences.PropertyChanged += (_, _) => RequestSave();
        Hotkeys.PropertyChanged += (_, _) =>
        {
            HotkeysChanged?.Invoke(this, EventArgs.Empty);
            RequestSave();
        };

        _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(650) };
        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            SaveNow();
        };

        UpdateActiveFlags();
    }

    private void Current_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsDirty = true;
        OverlayChanged?.Invoke(this, EventArgs.Empty);
        if (e.PropertyName is nameof(CrosshairSettings.Name))
            OnPropertyChanged(nameof(CurrentName));
        RequestSave();
    }

    public void ApplyPreset(CrosshairPreset preset)
    {
        _applyingPreset = true;
        try
        {
            ActivePresetId = preset.Id;
            var clone = preset.Settings.Clone();
            clone.Name = preset.Name;
            clone.Visible = Current.Visible;
            clone.ClickThrough = Current.ClickThrough;
            clone.AlwaysOnTop = Current.AlwaysOnTop;
            clone.OffsetX = Current.OffsetX;
            clone.OffsetY = Current.OffsetY;
            clone.Scale = Current.Scale;
            clone.Opacity = Current.Opacity;
            Current = clone;
            IsDirty = false;
        }
        finally
        {
            _applyingPreset = false;
        }
        UpdateActiveFlags();
        SaveNow();
        Notify(Loc.T("Crosshair '{0}' applied", preset.Name));
    }

    public void NextPreset()
    {
        if (Presets.Count == 0)
            return;
        var index = Presets.IndexOf(ActivePreset ?? Presets[0]);
        ApplyPreset(Presets[(index + 1) % Presets.Count]);
    }

    public void NewCustom()
    {
        var settings = new CrosshairSettings
        {
            Name = "Custom crosshair",
            Visible = Current.Visible,
            ClickThrough = Current.ClickThrough,
            AlwaysOnTop = Current.AlwaysOnTop,
            OffsetX = Current.OffsetX,
            OffsetY = Current.OffsetY
        };
        _applyingPreset = true;
        try
        {
            ActivePresetId = "";
            Current = settings;
            IsDirty = true;
        }
        finally
        {
            _applyingPreset = false;
        }
        UpdateActiveFlags();
        SaveNow();
        Notify(Loc.T("New crosshair created"));
    }

    public void ResetCurrent()
    {
        var preset = ActivePreset;
        if (preset is not null)
        {
            ApplyPreset(preset);
            Notify(Loc.T("Changes reset"));
            return;
        }
        var reset = new CrosshairSettings
        {
            Visible = Current.Visible,
            OffsetX = Current.OffsetX,
            OffsetY = Current.OffsetY
        };
        Current = reset;
        SaveNow();
        Notify(Loc.T("Changes reset"));
    }

    public CrosshairPreset SaveCurrentAsProfile()
    {
        var name = string.IsNullOrWhiteSpace(Current.Name) ? "My crosshair" : Current.Name.Trim();
        Current.Name = name;

        var existing = Presets.FirstOrDefault(p => p.IsUser && p.Id == ActivePresetId);
        if (existing is null)
            existing = Presets.FirstOrDefault(p =>
                p.IsUser && string.Equals(p.Name, name, StringComparison.CurrentCultureIgnoreCase));

        CrosshairPreset result;
        if (existing is not null && ActivePresetId == existing.Id)
        {
            existing.Settings = Current.Clone();
            existing.Name = name;
            result = existing;
        }
        else
        {
            var uniqueName = name;
            var suffix = 2;
            while (Presets.Any(p => p.IsUser && p.Name.Equals(uniqueName, StringComparison.CurrentCultureIgnoreCase)))
                uniqueName = $"{name} {suffix++}";

            result = new CrosshairPreset
            {
                Id = "user-" + Guid.NewGuid().ToString("N"),
                Name = uniqueName,
                Category = "Mine",
                IsUser = true,
                IsActive = true,
                Settings = Current.Clone()
            };
            result.Settings.Name = uniqueName;
            Current.Name = uniqueName;
            Presets.Add(result);
        }

        ActivePresetId = result.Id;
        UpdateActiveFlags();
        IsDirty = false;
        SaveNow();
        Notify(Loc.T("'{0}' saved to the library", result.Name));
        return result;
    }

    public CrosshairPreset SaveCurrentAsNewProfile()
    {
        var baseName = string.IsNullOrWhiteSpace(Current.Name) ? "My crosshair" : Current.Name.Trim();
        var uniqueName = baseName;
        var suffix = 2;
        while (Presets.Any(p => p.IsUser && p.Name.Equals(uniqueName, StringComparison.CurrentCultureIgnoreCase)))
            uniqueName = $"{baseName} {suffix++}";

        Current.Name = uniqueName;
        var preset = new CrosshairPreset
        {
            Id = "user-" + Guid.NewGuid().ToString("N"),
            Name = uniqueName,
            Category = "Mine",
            IsUser = true,
            Settings = Current.Clone()
        };
        preset.Settings.Name = uniqueName;
        Presets.Add(preset);
        ActivePresetId = preset.Id;
        UpdateActiveFlags();
        IsDirty = false;
        SaveNow();
        Notify(Loc.T("'{0}' saved as a new profile", uniqueName));
        return preset;
    }

    public void DeletePreset(CrosshairPreset preset)
    {
        if (!preset.IsUser)
            return;
        if (preset.IsActive)
        {
            var fallback = Presets.FirstOrDefault(p => !p.IsUser) ?? Presets.FirstOrDefault(p => p != preset);
            if (fallback is not null)
                ApplyPreset(fallback);
        }
        Presets.Remove(preset);
        SaveNow();
        Notify(Loc.T("'{0}' deleted", preset.Name));
    }

    public void ToggleOverlay()
    {
        Current.Visible = !Current.Visible;
        Notify(Loc.T(Current.Visible ? "Overlay on" : "Overlay hidden"));
    }

    public void Notify(string message) => ToastRequested?.Invoke(message);

    public void ResetAll()
    {
        _applyingPreset = true;
        try
        {
            foreach (var profile in Presets.Where(p => p.IsUser).ToList())
                Presets.Remove(profile);

            var fallback = Presets.FirstOrDefault(p => p.Id == "builtin-aurora") ?? Presets.FirstOrDefault();
            if (fallback is null)
                return;

            ActivePresetId = fallback.Id;
            Current = fallback.Settings.Clone();
            Current.Name = fallback.Name;
            IsDirty = false;

            Preferences.Accent = "Graphite";
            Preferences.Language = "en";
            Preferences.MinimizeToTray = true;
            Preferences.StartMinimized = false;
            Preferences.EcoMode = true;
            Preferences.ShowPerformance = true;
            Preferences.ShowHotkeyHints = true;
            Preferences.LivePreview = true;
            Preferences.LivePreviewOpacity = 0.12;

            Hotkeys.Enabled = true;
            Hotkeys.ToggleOverlay = "F8";
            Hotkeys.NextProfile = "F9";
            Hotkeys.OpenEditor = "F10";
            Hotkeys.PanicHide = "F12";
        }
        finally
        {
            _applyingPreset = false;
        }
        UpdateActiveFlags();
        SaveNow();
        Loc.SetLanguage("en");
        Notify(Loc.T("All settings reset"));
    }

    public void RequestSave()
    {
        if (_applyingPreset)
            return;
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    public void SaveNow()
    {
        _saveTimer.Stop();
        var activeProfile = Presets.FirstOrDefault(p => p.IsUser && p.Id == ActivePresetId);
        if (activeProfile is not null)
        {
            activeProfile.Settings = Current.Clone();
            activeProfile.Name = string.IsNullOrWhiteSpace(Current.Name) ? activeProfile.Name : Current.Name.Trim();
        }

        var persisted = new PersistedState
        {
            Current = Current.Clone(),
            Preferences = Preferences,
            Hotkeys = Hotkeys,
            ActivePresetId = ActivePresetId,
            Profiles = Presets.Where(p => p.IsUser).Select(p => p.Clone()).ToList()
        };
        SettingsStore.Save(persisted);
    }

    private void UpdateActiveFlags()
    {
        foreach (var preset in Presets)
            preset.IsActive = preset.Id == ActivePresetId;
    }
}
