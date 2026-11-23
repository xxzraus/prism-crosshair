using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;
using PrismCrosshair.Services;

namespace PrismCrosshair.Views;

public partial class OverlayPage : UserControl
{
    private readonly AppState _state = AppState.Instance;

    public OverlayPage()
    {
        InitializeComponent();
    }

    private void Nudge_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag })
            return;
        var parts = tag.Split(':');
        if (parts.Length != 2 || !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var delta))
            return;
        if (parts[0] == "X")
            _state.Current.OffsetX += delta;
        else
            _state.Current.OffsetY += delta;
    }

    private void QuickPosition_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag })
            return;
        var parts = tag.Split(',');
        if (parts.Length != 2 ||
            !double.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var dx) ||
            !double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var dy))
            return;
        _state.Current.OffsetX += dx;
        _state.Current.OffsetY += dy;
    }

    private void ResetPosition_Click(object sender, RoutedEventArgs e)
    {
        _state.Current.OffsetX = 0;
        _state.Current.OffsetY = 0;
        _state.Notify(Loc.T("Crosshair centered"));
    }

    private void OpenEditor_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow shell)
            shell.NavigateTo("editor");
    }

    private void NumberBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox box)
            return;
        box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        Keyboard.ClearFocus();
        e.Handled = true;
    }

    private bool _syncingStartup;

    private void Startup_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || _syncingStartup)
            return;

        var enabled = StartupToggle.IsChecked == true;
        if (StartupService.Set(enabled))
        {
            _state.Preferences.RunAtStartup = enabled;
            _state.Notify(Loc.T(enabled ? "PRISM will start with Windows" : "Autostart disabled"));
            return;
        }

        _syncingStartup = true;
        StartupToggle.IsChecked = !enabled;
        _state.Preferences.RunAtStartup = !enabled;
        _syncingStartup = false;
        _state.Notify(Loc.T("Failed to change autostart"));
    }
}
