using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;
using PrismCrosshair.Services;
using PrismCrosshair.Windows;

namespace PrismCrosshair;

public partial class App : Application
{
    public static new App Current => (App)Application.Current;

    public AppState State { get; private set; } = null!;
    public MainWindow Shell { get; private set; } = null!;
    public OverlayWindow Overlay { get; private set; } = null!;
    public TrayService? Tray { get; private set; }
    public bool IsExiting { get; private set; }

    private GlobalHotkeyService? _hotkeys;
    private PerformanceMonitor? _performance;
    private Mutex? _instanceMutex;
    private bool _startHidden;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _instanceMutex = new Mutex(true, @"Local\PrismCrosshairStudio", out var isFirstInstance);
        if (!isFirstInstance)
        {
            MessageBox.Show(
                "PRISM is already running. Open it from the system tray — the icon next to the clock.",
                "PRISM — Crosshair Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return;
        }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) => SettingsStore.Log("Unhandled: " + args.ExceptionObject);

        Loc.Initialize();
        State = AppState.Instance;
        Loc.SetLanguage(State.Preferences.Language);
        ThemeService.Apply(State.Preferences.Accent);

        if (State.Preferences.RunAtStartup != StartupService.IsEnabled())
            State.Preferences.RunAtStartup = StartupService.IsEnabled();

        _startHidden = e.Args.Any(a => a.Equals("--minimized", StringComparison.OrdinalIgnoreCase))
                       || State.Preferences.StartMinimized;

        Overlay = new OverlayWindow(State);
        Shell = new MainWindow { DataContext = State };
        Shell.Activated += (_, _) =>
        {
            State.IsStudioActive = true;
            Overlay.SetStudioVisible(false);
        };
        Shell.Deactivated += (_, _) =>
        {
            State.IsStudioActive = false;
            Overlay.SetStudioVisible(true);
        };
        Tray = new TrayService(State);
        Loc.LanguageChanged += (_, _) =>
        {
            Shell?.ApplyLanguage();
            Tray?.RefreshLanguage();
        };

        Tray.ShowRequested += (_, _) => ShowShell();
        Tray.ToggleOverlayRequested += (_, _) => State.ToggleOverlay();
        Tray.NextPresetRequested += (_, _) => State.NextPreset();
        Tray.ExitRequested += (_, _) => ExitApp();

        _performance = new PerformanceMonitor();
        _performance.Start();

        new WindowInteropHelper(Shell).EnsureHandle();
        _hotkeys = new GlobalHotkeyService(Shell);
        _hotkeys.Pressed += Hotkeys_Pressed;
        RegisterHotkeys();
        State.HotkeysChanged += (_, _) => RegisterHotkeys();

        if (!_startHidden)
            Shell.Show();
        else if (State.Preferences.FirstRun)
            Tray.ShowBalloon(Loc.T("PRISM is already running"), Loc.T("The crosshair is pinned on screen. Click the tray icon to open the studio."));

        State.Preferences.FirstRun = false;

        if (State.Current.Visible)
            Overlay.Apply();

        if (_startHidden)
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => MemoryTrimmer.Trim());
    }

    public void ShowShell()
    {
        if (!Shell.IsVisible)
        {
            Shell.RestoreContent();
            Shell.Show();
        }
        if (Shell.WindowState == WindowState.Minimized)
            Shell.WindowState = WindowState.Normal;
        Shell.ShowInTaskbar = true;
        Shell.Activate();
        Shell.Focus();
        Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => Loc.ApplyTo(Shell)));
    }

    public void HideShellToTray(bool balloon = true)
    {
        Shell.Hide();
        State.SaveNow();
        State.IsStudioActive = false;
        Shell.ReleaseContent();
        Overlay.SetStudioVisible(true);
        Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, () => MemoryTrimmer.Trim());
        if (balloon)
            Tray?.ShowBalloon(Loc.T("PRISM minimized"), Loc.T("The crosshair stays on screen. F8 quickly hides or shows it."));
    }

    public void SwitchLanguage(string language)
    {
        var normalized = string.Equals(language, "ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "en";
        State.Preferences.Language = normalized;
        State.SaveNow();
        Loc.SetLanguage(normalized);
    }

    public void ExitApp()
    {
        if (IsExiting)
            return;
        IsExiting = true;
        State.SaveNow();
        _hotkeys?.Dispose();
        _performance?.Dispose();
        Tray?.Dispose();
        Overlay?.HideOverlay();
        Overlay?.Close();
        Shutdown();
    }

    private void RegisterHotkeys()
    {
        if (_hotkeys is null)
            return;
        var failures = _hotkeys.RegisterAll(State.Hotkeys);
        if (failures.Count > 0)
            State.Notify(failures[0]);
    }

    private void Hotkeys_Pressed(object? sender, HotkeyAction action)
    {
        switch (action)
        {
            case HotkeyAction.ToggleOverlay:
                State.ToggleOverlay();
                break;
            case HotkeyAction.NextProfile:
                State.NextPreset();
                break;
            case HotkeyAction.OpenEditor:
                ShowShell();
                Shell.NavigateTo("editor");
                break;
            case HotkeyAction.PanicHide:
                State.Current.Visible = false;
                HideShellToTray(false);
                break;
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        SettingsStore.Log("Dispatcher exception: " + e.Exception);
        e.Handled = true;
        State.Notify(Loc.T("Something went wrong — see prism.log for details"));
    }

    protected override void OnExit(ExitEventArgs e)
    {
        State?.SaveNow();
        _hotkeys?.Dispose();
        _performance?.Dispose();
        Tray?.Dispose();
        try
        {
            _instanceMutex?.ReleaseMutex();
        }
        catch
        {
            // ignore
        }
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }
}
