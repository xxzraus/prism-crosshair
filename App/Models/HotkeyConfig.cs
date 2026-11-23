namespace PrismCrosshair.Models;

public sealed class HotkeyConfig : ObservableObject
{
    private bool _enabled = true;
    private string _toggleOverlay = "F8";
    private string _nextProfile = "F9";
    private string _openEditor = "F10";
    private string _panicHide = "F12";

    public bool Enabled { get => _enabled; set => Set(ref _enabled, value); }
    public string ToggleOverlay { get => _toggleOverlay; set => Set(ref _toggleOverlay, string.IsNullOrWhiteSpace(value) ? "None" : value); }
    public string NextProfile { get => _nextProfile; set => Set(ref _nextProfile, string.IsNullOrWhiteSpace(value) ? "None" : value); }
    public string OpenEditor { get => _openEditor; set => Set(ref _openEditor, string.IsNullOrWhiteSpace(value) ? "None" : value); }
    public string PanicHide { get => _panicHide; set => Set(ref _panicHide, string.IsNullOrWhiteSpace(value) ? "None" : value); }
}

public static class HotkeyOptions
{
    public static IReadOnlyList<string> All { get; } = new[]
    {
        "None", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"
    };
}
