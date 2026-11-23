using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Windows;

public partial class OverlayWindow : Window
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExToolWindow = 0x00000080;
    private const int WsExLayered = 0x00080000;
    private const int WsExNoActivate = 0x08000000;
    private const uint SwpNoSize = 0x0001;
    private const uint SwpNoMove = 0x0002;
    private const uint SwpNoZOrder = 0x0004;
    private const uint SwpNoActivate = 0x0010;
    private static readonly IntPtr HwndTopmost = new(-1);
    private static readonly IntPtr HwndNotTopmost = new(-2);

    private readonly AppState _state;
    private bool _initialized;
    private bool _studioVisible = true;
    private bool _temporaryPeek;
    private bool _dragging;
    private POINT _dragStartCursor;
    private int _dragStartLeft;
    private int _dragStartTop;
    private double _dragStartOffsetX;
    private double _dragStartOffsetY;

    public OverlayWindow(AppState state)
    {
        _state = state;
        InitializeComponent();
        DataContext = state;
        SourceInitialized += OnSourceInitialized;
        Loaded += (_, _) => Apply();
        _state.OverlayChanged += State_OverlayChanged;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = GetWindowLong(hwnd, GwlExStyle);
        exStyle |= WsExLayered | WsExToolWindow | WsExNoActivate;
        if (_state.Current.ClickThrough)
            exStyle |= WsExTransparent;
        SetWindowLong(hwnd, GwlExStyle, exStyle);
        _initialized = true;
        ApplyWindowFlags();
        Reposition();
    }

    private void State_OverlayChanged(object? sender, EventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(Apply);
            return;
        }
        Apply();
    }

    public void Apply()
    {
        Reticle.Settings = _state.Current;
        Reticle.Animate = !_state.Preferences.EcoMode && (_studioVisible || _temporaryPeek) && _state.Current.Visible;
        ApplyWindowFlags();
        Reposition();

        if (_state.Current.Visible && (_studioVisible || _temporaryPeek))
        {
            if (!IsVisible)
                Show();
        }
        else if (IsVisible)
        {
            Hide();
        }
    }

    public void SetTemporaryPeek(bool visible)
    {
        if (_temporaryPeek == visible)
            return;
        _temporaryPeek = visible;
        Apply();
    }

    public void SetStudioVisible(bool visible)
    {
        if (_studioVisible == visible)
            return;
        _studioVisible = visible;
        if (!visible)
            HideOverlay();
        else
            Apply();
    }

    public void HideOverlay()
    {
        Reticle.Animate = false;
        if (IsVisible)
            Hide();
    }

    private void ApplyWindowFlags()
    {
        if (!_initialized)
            return;
        var hwnd = new WindowInteropHelper(this).Handle;
        var exStyle = GetWindowLong(hwnd, GwlExStyle);

        if (_state.Current.ClickThrough)
            exStyle |= WsExTransparent;
        else
            exStyle &= ~WsExTransparent;

        SetWindowLong(hwnd, GwlExStyle, exStyle);
        Topmost = _state.Current.AlwaysOnTop;
        SetWindowPos(hwnd, _state.Current.AlwaysOnTop ? HwndTopmost : HwndNotTopmost,
            0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoActivate);

        var interactive = !_state.Current.ClickThrough;
        OverlayRoot.IsHitTestVisible = interactive;
        OverlayRoot.Cursor = interactive ? System.Windows.Input.Cursors.Arrow : null;
        if (!interactive && _dragging)
            EndDrag(false);
    }

    private void OverlayRoot_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_state.Current.ClickThrough || !_initialized)
            return;
        if (!GetCursorPos(out _dragStartCursor))
            return;
        var hwnd = new WindowInteropHelper(this).Handle;
        if (!GetWindowRect(hwnd, out var rect))
            return;
        _dragStartLeft = rect.Left;
        _dragStartTop = rect.Top;
        _dragStartOffsetX = _state.Current.OffsetX;
        _dragStartOffsetY = _state.Current.OffsetY;
        _dragging = true;
        CaptureMouse();
        e.Handled = true;
    }

    private void OverlayRoot_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_dragging || _state.Current.ClickThrough)
            return;
        if (!GetCursorPos(out var cursor))
            return;
        var hwnd = new WindowInteropHelper(this).Handle;
        SetWindowPos(hwnd, IntPtr.Zero,
            _dragStartLeft + cursor.X - _dragStartCursor.X,
            _dragStartTop + cursor.Y - _dragStartCursor.Y,
            0, 0, SwpNoSize | SwpNoZOrder | SwpNoActivate);
    }

    private void OverlayRoot_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (!_dragging)
            return;
        EndDrag(true);
        e.Handled = true;
    }

    private void OverlayRoot_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_dragging)
            EndDrag(false);
    }

    private void EndDrag(bool commit)
    {
        _dragging = false;
        if (IsMouseCaptured)
            ReleaseMouseCapture();

        if (!commit || !GetCursorPos(out var cursor))
            return;

        _state.Current.OffsetX = _dragStartOffsetX + cursor.X - _dragStartCursor.X;
        _state.Current.OffsetY = _dragStartOffsetY + cursor.Y - _dragStartCursor.Y;
        _state.Notify(Loc.T("Crosshair position saved"));
    }

    private void Reposition()
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        var scaleX = dpi.DpiScaleX <= 0 ? 1 : dpi.DpiScaleX;
        var scaleY = dpi.DpiScaleY <= 0 ? 1 : dpi.DpiScaleY;

        var area = System.Windows.Forms.Screen.PrimaryScreen?.Bounds
                   ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);

        Left = area.Left / scaleX + area.Width / scaleX / 2 - Width / 2 + _state.Current.OffsetX / scaleX;
        Top = area.Top / scaleY + area.Height / scaleY / 2 - Height / 2 + _state.Current.OffsetY / scaleY;
    }

    protected override void OnClosed(EventArgs e)
    {
        _state.OverlayChanged -= State_OverlayChanged;
        base.OnClosed(e);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }
}
