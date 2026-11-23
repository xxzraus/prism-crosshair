using System.Text.Json.Serialization;
using PrismCrosshair.Localization;

namespace PrismCrosshair.Models;

public sealed class AppPreferences : ObservableObject
{
    private string _accent = "Graphite";
    private string _language = "en";
    private bool _minimizeToTray = true;
    private bool _startMinimized;
    private bool _runAtStartup;
    private bool _ecoMode = true;
    private bool _showPerformance = true;
    private bool _showHotkeyHints = true;
    private bool _livePreview = true;
    private double _livePreviewOpacity = 0.12;
    private bool _firstRun = true;

    public string Accent { get => _accent; set => Set(ref _accent, value); }
    public string Language { get => _language; set => Set(ref _language, string.Equals(value, "ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "en"); }
    public bool MinimizeToTray { get => _minimizeToTray; set => Set(ref _minimizeToTray, value); }
    public bool StartMinimized { get => _startMinimized; set => Set(ref _startMinimized, value); }
    public bool RunAtStartup { get => _runAtStartup; set => Set(ref _runAtStartup, value); }
    public bool EcoMode { get => _ecoMode; set => Set(ref _ecoMode, value); }
    public bool ShowPerformance { get => _showPerformance; set => Set(ref _showPerformance, value); }
    public bool ShowHotkeyHints { get => _showHotkeyHints; set => Set(ref _showHotkeyHints, value); }
    public bool LivePreview { get => _livePreview; set => Set(ref _livePreview, value); }
    public double LivePreviewOpacity { get => _livePreviewOpacity; set => Set(ref _livePreviewOpacity, Math.Round(Math.Clamp(value, 0.05, 0.6), 2)); }
    public bool FirstRun { get => _firstRun; set => Set(ref _firstRun, value); }

    [JsonIgnore] public string AccentLabel => Loc.T(Accent switch
    {
        "Aurora" => "Aurora",
        "Ice" => "Ice",
        "Ember" => "Sunset",
        "Violet" => "Ultraviolet",
        "Forest" => "Forest",
        _ => "Graphite"
    });
}
