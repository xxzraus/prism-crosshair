using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Views;

public partial class EditorPage : UserControl
{
    private readonly AppState _state = AppState.Instance;
    private readonly List<StyleOption> _styleOptions;
    private readonly List<AnimationOption> _animationOptions;
    private CrosshairSettings _current;
    private bool _subscribed;

    public EditorPage()
    {
        InitializeComponent();

        _styleOptions = new List<StyleOption>
        {
            new(ReticleStyle.Cross, Loc.T("Cross")),
            new(ReticleStyle.Dot, Loc.T("Dot")),
            new(ReticleStyle.Circle, Loc.T("Circle")),
            new(ReticleStyle.TShape, Loc.T("T-shape")),
            new(ReticleStyle.XShape, Loc.T("X")),
            new(ReticleStyle.Chevron, Loc.T("Chevron")),
            new(ReticleStyle.Brackets, Loc.T("Brackets")),
            new(ReticleStyle.Hybrid, Loc.T("Hybrid"))
        };
        _animationOptions = new List<AnimationOption>
        {
            new(ReticleAnimation.None, Loc.T("No animation")),
            new(ReticleAnimation.Pulse, Loc.T("Pulse")),
            new(ReticleAnimation.Breathe, Loc.T("Breathe")),
            new(ReticleAnimation.Rotate, Loc.T("Rotate"))
        };

        StyleItems.ItemsSource = _styleOptions;
        AnimationBox.ItemsSource = _animationOptions;

        _current = _state.Current;
        UpdateStyleSelection();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_subscribed)
        {
            _current = _state.Current;
            _current.PropertyChanged += Current_PropertyChanged;
            _state.PropertyChanged += State_PropertyChanged;
            _subscribed = true;
        }
        UpdateStyleSelection();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (!_subscribed)
            return;
        _current.PropertyChanged -= Current_PropertyChanged;
        _state.PropertyChanged -= State_PropertyChanged;
        _subscribed = false;
    }

    private void State_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AppState.Current))
            return;
        _current.PropertyChanged -= Current_PropertyChanged;
        _current = _state.Current;
        _current.PropertyChanged += Current_PropertyChanged;
        UpdateStyleSelection();
    }

    private void Current_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CrosshairSettings.Style))
            UpdateStyleSelection();
    }

    private void UpdateStyleSelection()
    {
        foreach (var option in _styleOptions)
            option.IsSelected = option.Style == _current.Style;
    }

    private void Animation_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || AnimationBox.SelectedItem is not AnimationOption option)
            return;
        if (option.Value != ReticleAnimation.None && _state.Preferences.EcoMode)
        {
            _state.Preferences.EcoMode = false;
            _state.Notify(Loc.T("Eco mode disabled so the animation also works in game"));
        }
    }

    private void Style_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: StyleOption option })
            _state.Current.Style = option.Style;
    }

    private void Backdrop_Checked(object sender, RoutedEventArgs e)
    {
        if (Backdrop is null || sender is not RadioButton { Tag: string mode })
            return;
        Backdrop.Mode = mode;
    }

    private void Grid_Changed(object sender, RoutedEventArgs e)
    {
        if (Backdrop is null)
            return;
        Backdrop.ShowGrid = GridToggle.IsChecked == true;
    }

    private void Zoom_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (Preview is null || ZoomBox.SelectedItem is not ComboBoxItem { Tag: string value })
            return;
        if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var scale))
            Preview.RenderScale = scale;
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _state.ResetCurrent();
        UpdateStyleSelection();
    }
}

public sealed class StyleOption : ObservableObject
{
    private bool _isSelected;

    public StyleOption(ReticleStyle style, string name)
    {
        Style = style;
        Name = name;
    }

    public ReticleStyle Style { get; }
    public string Name { get; }
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }
}

public sealed class AnimationOption
{
    public AnimationOption(ReticleAnimation value, string name)
    {
        Value = value;
        Name = name;
    }

    public ReticleAnimation Value { get; }
    public string Name { get; }
    public override string ToString() => Name;
}
