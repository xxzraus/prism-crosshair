using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Services;

public enum HotkeyAction
{
    ToggleOverlay,
    NextProfile,
    OpenEditor,
    PanicHide
}

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;
    private readonly Window _window;
    private HwndSource? _source;
    private readonly List<int> _registered = new();
    private bool _initialized;

    public event EventHandler<HotkeyAction>? Pressed;

    public GlobalHotkeyService(Window window)
    {
        _window = window;
        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
            Initialize();
        else
            window.SourceInitialized += (_, _) => Initialize();
    }

    private void Initialize()
    {
        if (_initialized)
            return;

        var helper = new WindowInteropHelper(_window);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(WndProc);
        _initialized = true;
    }

    public IReadOnlyList<string> RegisterAll(HotkeyConfig config)
    {
        var failures = new List<string>();
        UnregisterAll();
        if (!config.Enabled)
            return failures;

        Register(1, HotkeyAction.ToggleOverlay, config.ToggleOverlay, "Show/hide", failures);
        Register(2, HotkeyAction.NextProfile, config.NextProfile, "Next crosshair", failures);
        Register(3, HotkeyAction.OpenEditor, config.OpenEditor, "Open editor", failures);
        Register(4, HotkeyAction.PanicHide, config.PanicHide, "Panic hide", failures);
        return failures;
    }

    private void Register(int id, HotkeyAction action, string keyName, string label, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(keyName) || keyName.Equals("None", StringComparison.OrdinalIgnoreCase))
            return;
        if (!Enum.TryParse<System.Windows.Input.Key>(keyName, out var key))
            return;

        var vk = System.Windows.Input.KeyInterop.VirtualKeyFromKey(key);
        if (vk == 0 || !RegisterHotKey(new WindowInteropHelper(_window).Handle, id, 0, vk))
        {
            failures.Add(Loc.T("{0}: {1} is already used by another app", label, keyName));
            return;
        }
        _registered.Add(id);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey)
        {
            var action = wParam.ToInt32() switch
            {
                1 => HotkeyAction.ToggleOverlay,
                2 => HotkeyAction.NextProfile,
                3 => HotkeyAction.OpenEditor,
                4 => HotkeyAction.PanicHide,
                _ => (HotkeyAction?)null
            };
            if (action is not null)
            {
                Pressed?.Invoke(this, action.Value);
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    private void UnregisterAll()
    {
        var handle = new WindowInteropHelper(_window).Handle;
        foreach (var id in _registered)
            UnregisterHotKey(handle, id);
        _registered.Clear();
    }

    public void Dispose()
    {
        UnregisterAll();
        _source?.RemoveHook(WndProc);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
