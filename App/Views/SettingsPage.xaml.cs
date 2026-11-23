using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;
using PrismCrosshair.Services;

namespace PrismCrosshair.Views;

public partial class SettingsPage : UserControl
{
    private readonly AppState _state = AppState.Instance;
    private readonly List<ThemeOption> _themes;

    public SettingsPage()
    {
        InitializeComponent();
        _themes = ThemeService.Themes.Select(t => new ThemeOption(t)).ToList();
        ThemeItems.ItemsSource = _themes;
        UpdateThemeSelection();
        DataPathText.Text = SettingsStore.DataDirectory;
        Loaded += (_, _) =>
        {
            UpdateThemeSelection();
            DataPathText.Text = SettingsStore.DataDirectory;
        };
    }

    private void UpdateThemeSelection()
    {
        foreach (var theme in _themes)
            theme.IsSelected = theme.Theme.Key.Equals(_state.Preferences.Accent, StringComparison.OrdinalIgnoreCase);
    }

    private void Language_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || sender is not ComboBox { SelectedValue: string language })
            return;
        if (!string.Equals(language, Loc.CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            App.Current.SwitchLanguage(language);
    }

    private void Theme_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ThemeOption option })
            return;

        ThemeService.Apply(option.Theme.Key);
        _state.Preferences.Accent = option.Theme.Key;
        UpdateThemeSelection();
        _state.Notify(Loc.T("Theme '{0}' applied", option.Theme.Label));
    }

    private void Export_Click(object sender, RoutedEventArgs e)
    {
        _state.SaveNow();
        var dialog = new SaveFileDialog
        {
            Title = Loc.T("Export PRISM settings"),
            FileName = "prism-settings.json",
            Filter = Loc.T("JSON (*.json)|*.json|All files (*.*)|*.*"),
            InitialDirectory = SettingsStore.DataDirectory
        };
        if (dialog.ShowDialog() != true)
            return;
        try
        {
            File.Copy(SettingsStore.SettingsPath, dialog.FileName, true);
            _state.Notify(Loc.T("Settings exported"));
        }
        catch (Exception ex)
        {
            SettingsStore.Log("Export failed: " + ex);
            _state.Notify(Loc.T("Failed to export settings"));
        }
    }

    private void OpenFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Directory.CreateDirectory(SettingsStore.DataDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = SettingsStore.DataDirectory,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            SettingsStore.Log("Open folder failed: " + ex);
            _state.Notify(Loc.T("Failed to open the data folder"));
        }
    }

    private void ResetAll_Click(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(
            Loc.T("Delete all saved crosshairs and restore defaults?"),
            Loc.T("PRISM — reset settings"),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes)
            return;

        _state.ResetAll();
        ThemeService.Apply(_state.Preferences.Accent);
        UpdateThemeSelection();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        if (Window.GetWindow(this) is MainWindow shell)
            shell.NavigateTo("about");
    }
}

public sealed class ThemeOption : ObservableObject
{
    private bool _isSelected;

    public ThemeOption(AppTheme theme)
    {
        Theme = theme;
    }

    public AppTheme Theme { get; }
    public string Key => Theme.Key;
    public string Label => Loc.T(Theme.Label);
    public string Hint => Loc.T(Theme.Hint);
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
}
