using System.Windows;
using System.Windows.Controls;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Views;

public partial class HotkeysPage : UserControl
{
    private readonly AppState _state = AppState.Instance;
    private readonly List<KeyMapItem> _keys = Enumerable.Range(1, 12)
        .Select(i => new KeyMapItem($"F{i}"))
        .ToList();

    public HotkeysPage()
    {
        InitializeComponent();
        KeyMapItems.ItemsSource = _keys;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _state.Hotkeys.PropertyChanged -= Hotkeys_PropertyChanged;
        _state.Hotkeys.PropertyChanged += Hotkeys_PropertyChanged;
        UpdateKeyMap();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _state.Hotkeys.PropertyChanged -= Hotkeys_PropertyChanged;
    }

    private void Hotkeys_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        => UpdateKeyMap();

    private void UpdateKeyMap()
    {
        var assignments = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        AddAssignment(assignments, _state.Hotkeys.ToggleOverlay, Loc.T("Show/hide overlay"));
        AddAssignment(assignments, _state.Hotkeys.NextProfile, Loc.T("Next crosshair"));
        AddAssignment(assignments, _state.Hotkeys.OpenEditor, Loc.T("Open editor"));
        AddAssignment(assignments, _state.Hotkeys.PanicHide, Loc.T("Panic hide"));

        foreach (var key in _keys)
        {
            if (assignments.TryGetValue(key.Label, out var actions))
            {
                key.IsAssigned = true;
                key.Action = string.Join(" · ", actions);
            }
            else
            {
                key.IsAssigned = false;
                key.Action = "";
            }
        }

        KeyMapSummary.Text = Loc.T("{0} of 12 keys assigned", assignments.Count);
    }

    private static void AddAssignment(Dictionary<string, List<string>> map, string? key, string action)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Equals("None", StringComparison.OrdinalIgnoreCase))
            return;
        if (!map.TryGetValue(key, out var list))
            map[key] = list = new List<string>();
        list.Add(action);
    }

    private void ResetHotkeys_Click(object sender, RoutedEventArgs e)
    {
        _state.Hotkeys.ToggleOverlay = "F8";
        _state.Hotkeys.NextProfile = "F9";
        _state.Hotkeys.OpenEditor = "F10";
        _state.Hotkeys.PanicHide = "F12";
        _state.Notify(Loc.T("Hotkeys reset"));
    }
}

public sealed class KeyMapItem : ObservableObject
{
    private bool _isAssigned;
    private string _action = "";

    public KeyMapItem(string label) => Label = label;

    public string Label { get; }
    public bool IsAssigned { get => _isAssigned; set => Set(ref _isAssigned, value); }
    public string Action { get => _action; set => Set(ref _action, value); }
}
