using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using PrismCrosshair.Models;

namespace PrismCrosshair.Views;

public partial class CrosshairsPage : UserControl
{
    private readonly AppState _state = AppState.Instance;
    private ICollectionView? _view;
    private string _filter = "all";

    public CrosshairsPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_view is not null)
            return;
        _view = CollectionViewSource.GetDefaultView(_state.Presets);
        _view.Filter = FilterPreset;
        PresetItems.ItemsSource = _view;
        _state.Presets.CollectionChanged -= Presets_CollectionChanged;
        _state.Presets.CollectionChanged += Presets_CollectionChanged;
        UpdateCount();
    }

    private void Presets_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        => UpdateCount();

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_view is null)
            return;
        _state.Presets.CollectionChanged -= Presets_CollectionChanged;
        _view.Filter = null;
        _view = null;
        PresetItems.ItemsSource = null;
    }

    private bool FilterPreset(object item)
    {
        if (item is not CrosshairPreset preset)
            return false;
        return _filter == "all" ||
               preset.Category.Equals(_filter, StringComparison.CurrentCultureIgnoreCase);
    }

    private void Filter_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { Tag: string filter })
            return;
        _filter = filter;
        _view?.Refresh();
        UpdateCount();
    }

    private void UpdateCount()
    {
        if (_view is null || CountText is null)
            return;
        CountText.Text = _view.Cast<CrosshairPreset>().Count().ToString();
    }

    private void Preset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button)
            return;
        var preset = button.Tag as CrosshairPreset ?? button.DataContext as CrosshairPreset;
        if (preset is not null)
        {
            _state.ApplyPreset(preset);
            UpdateCount();
        }
    }

    private void DeletePreset_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { Tag: CrosshairPreset preset })
        {
            _state.DeletePreset(preset);
            e.Handled = true;
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        _state.NewCustom();
        Navigate("editor");
    }

    private void Edit_Click(object sender, RoutedEventArgs e) => Navigate("editor");

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        _state.SaveCurrentAsNewProfile();
        UpdateCount();
    }

    private void Navigate(string key)
    {
        if (Window.GetWindow(this) is MainWindow shell)
            shell.NavigateTo(key);
    }
}
