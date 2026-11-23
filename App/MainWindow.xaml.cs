using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;
using PrismCrosshair.Views;

namespace PrismCrosshair;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, RadioButton> _navButtons = new();
    private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(2.4) };
    private bool _ready;
    private bool _livePreview;
    private string _currentPageKey = "";
    private string _lastPageKey = "crosshairs";

    public MainWindow()
    {
        InitializeComponent();
        _navButtons["crosshairs"] = NavCrosshairs;
        _navButtons["editor"] = NavEditor;
        _navButtons["overlay"] = NavOverlay;
        _navButtons["hotkeys"] = NavHotkeys;
        _navButtons["settings"] = NavSettings;
        _navButtons["about"] = NavAbout;

        Loaded += OnLoaded;
        StateChanged += (_, _) => UpdateWindowStateVisuals();
        _toastTimer.Tick += (_, _) => HideToast();
        State.Preferences.PropertyChanged += Preferences_PropertyChanged;
    }

    private AppState State => App.Current.State;

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        if (FindAncestor<Slider>(e.OriginalSource as DependencyObject) is not null)
            BeginLivePreview();
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        EndLivePreview();
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        EndLivePreview();
    }

    private void Preferences_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppPreferences.LivePreviewOpacity) && _livePreview)
            Opacity = Math.Clamp(State.Preferences.LivePreviewOpacity, 0.05, 0.6);
        else if (e.PropertyName == nameof(AppPreferences.LivePreview) && !State.Preferences.LivePreview)
            EndLivePreview();
    }

    private void BeginLivePreview()
    {
        if (_livePreview || !State.Preferences.LivePreview || !State.Current.Visible)
            return;
        _livePreview = true;
        Opacity = Math.Clamp(State.Preferences.LivePreviewOpacity, 0.05, 0.6);
        App.Current.Overlay.SetTemporaryPeek(true);
    }

    private void EndLivePreview()
    {
        if (!_livePreview)
            return;
        _livePreview = false;
        Opacity = 1;
        App.Current.Overlay.SetTemporaryPeek(false);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_ready)
            return;
        _ready = true;
        FitToWorkArea();
        State.ToastRequested -= State_ToastRequested;
        State.ToastRequested += State_ToastRequested;
        NavigateTo("crosshairs");
        Loc.ApplyTo(this);
    }

    private void FitToWorkArea()
    {
        var area = SystemParameters.WorkArea;
        var maxWidth = Math.Max(MinWidth, area.Width - 24);
        var maxHeight = Math.Max(MinHeight, area.Height - 24);
        if (Width <= maxWidth && Height <= maxHeight)
            return;
        Width = Math.Min(Width, maxWidth);
        Height = Math.Min(Height, maxHeight);
        Left = area.Left + (area.Width - Width) / 2;
        Top = area.Top + (area.Height - Height) / 2;
    }

    private void State_ToastRequested(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => ShowToast(message));
            return;
        }
        ShowToast(message);
    }

    public void ShowToast(string message)
    {
        if (!_ready)
            return;
        ToastText.Text = message;
        ToastPanel.Visibility = Visibility.Visible;
        ToastPanel.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        });
        var transform = new TranslateTransform(0, 8);
        ToastPanel.RenderTransform = transform;
        transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(8, 0, TimeSpan.FromMilliseconds(180))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        });
        _toastTimer.Stop();
        _toastTimer.Start();
    }

    private void HideToast()
    {
        _toastTimer.Stop();
        var animation = new DoubleAnimation(ToastPanel.Opacity, 0, TimeSpan.FromMilliseconds(180));
        animation.Completed += (_, _) => ToastPanel.Visibility = Visibility.Collapsed;
        ToastPanel.BeginAnimation(OpacityProperty, animation);
    }

    public void NavigateTo(string key, bool force = false)
    {
        if (!_navButtons.TryGetValue(key, out var nav))
            key = "crosshairs";
        if (!force && _currentPageKey == key && PageHost.Content is not null)
            return;
        _currentPageKey = key;
        _lastPageKey = key;
        if (nav is not null && nav.IsChecked != true)
            nav.IsChecked = true;
        ShowPage(key);
    }

    public void ApplyLanguage()
    {
        Title = Loc.T("PRISM — Crosshair Studio");
        State.Current.NotifyLocalizationChanged();
        Loc.ApplyTo(this);
        if (!string.IsNullOrWhiteSpace(_currentPageKey))
            NavigateTo(_currentPageKey, force: true);
    }

    public void ReleaseContent()
    {
        if (!string.IsNullOrWhiteSpace(_currentPageKey))
            _lastPageKey = _currentPageKey;
        PageHost.Content = null;
        _currentPageKey = "";
        GC.Collect(2, GCCollectionMode.Optimized, false, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Optimized, false, true);
    }

    public void RestoreContent()
    {
        if (PageHost.Content is null)
            NavigateTo(_lastPageKey);
    }

    private void Nav_Checked(object sender, RoutedEventArgs e)
    {
        if (!_ready || sender is not RadioButton { Tag: string key })
            return;
        Nav_CheckedCore(key);
    }

    private void Nav_CheckedCore(string key)
    {
        if (_currentPageKey == key && PageHost.Content is not null)
            return;
        _currentPageKey = key;
        ShowPage(key);
    }

    private void ShowPage(string key)
    {
        UserControl page = key switch
        {
            "editor" => new EditorPage(),
            "overlay" => new OverlayPage(),
            "hotkeys" => new HotkeysPage(),
            "settings" => new SettingsPage(),
            "about" => new AboutPage(),
            _ => new CrosshairsPage()
        };
        page.DataContext = State;
        PageHost.Content = page;
        PageHost.Opacity = 0;
        PageHost.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(190))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        });

        (PageTitleText.Text, PageSubtitleText.Text) = key switch
        {
            "editor" => (Loc.T("Crosshair editor"), Loc.T("Shape, color and effects in real time")),
            "overlay" => (Loc.T("Overlay"), Loc.T("Position, behavior and performance")),
            "hotkeys" => (Loc.T("Hotkeys"), Loc.T("Control without leaving the game")),
            "settings" => (Loc.T("Settings"), Loc.T("Theme, startup, data and performance")),
            "about" => (Loc.T("About"), Loc.T("PRISM — a lightweight crosshair studio for Windows")),
            _ => (Loc.T("Crosshair library"), Loc.T("Ready-made presets and your own setups"))
        };

        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
        {
            if (ReferenceEquals(PageHost.Content, page))
                Loc.ApplyTo(page);
        }));
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers != ModifierKeys.Control)
            return;

        var key = e.Key switch
        {
            Key.D1 or Key.NumPad1 => "crosshairs",
            Key.D2 or Key.NumPad2 => "editor",
            Key.D3 or Key.NumPad3 => "overlay",
            Key.D4 or Key.NumPad4 => "hotkeys",
            Key.D5 or Key.NumPad5 => "settings",
            _ => null
        };
        if (key is not null)
        {
            NavigateTo(key);
            e.Handled = true;
        }
    }

    private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
        e.Handled = true;
    }

    private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        State.SaveCurrentAsProfile();
        e.Handled = true;
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindAncestor<Button>(e.OriginalSource as DependencyObject) is not null)
            return;
        if (e.ClickCount == 2)
        {
            ToggleMaximize();
            return;
        }
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            try { DragMove(); }
            catch { /* ignored */ }
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void Maximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

    private void ToggleMaximize()
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Save_Click(object sender, RoutedEventArgs e) => State.SaveCurrentAsProfile();

    private void Status_Click(object sender, RoutedEventArgs e) => State.ToggleOverlay();

    private void Tray_Click(object sender, RoutedEventArgs e) => App.Current.HideShellToTray();

    private void Help_Click(object sender, RoutedEventArgs e)
    {
        NavigateTo("about");
        NavAbout.IsChecked = true;
    }

    private void UpdateWindowStateVisuals()
    {
        var maximized = WindowState == WindowState.Maximized;
        MaxIcon.Data = (Geometry)FindResource(maximized ? "IcoRestore" : "IcoMaximize");
        WindowFrame.CornerRadius = maximized ? new CornerRadius(0) : new CornerRadius(16);
        WindowFrame.BorderThickness = maximized ? new Thickness(0) : new Thickness(1);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        EndLivePreview();
        base.OnDeactivated(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!App.Current.IsExiting)
        {
            if (State.Preferences.MinimizeToTray)
            {
                e.Cancel = true;
                App.Current.HideShellToTray();
                return;
            }
            e.Cancel = true;
            App.Current.ExitApp();
            return;
        }
        State.ToastRequested -= State_ToastRequested;
        base.OnClosing(e);
    }

    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current is not null)
        {
            if (current is T match)
                return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }
}
