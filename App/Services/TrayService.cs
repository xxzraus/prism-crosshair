using System.Drawing;
using System.Windows.Forms;
using PrismCrosshair.Localization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly ToolStripMenuItem _openItem;
    private readonly ToolStripMenuItem _toggleItem;
    private readonly ToolStripMenuItem _nextItem;
    private readonly ToolStripMenuItem _exitItem;

    public event EventHandler? ShowRequested;
    public event EventHandler? ToggleOverlayRequested;
    public event EventHandler? NextPresetRequested;
    public event EventHandler? ExitRequested;

    public TrayService(AppState state)
    {
        _toggleItem = new ToolStripMenuItem(Loc.T("Hide crosshair"));
        _toggleItem.Click += (_, _) => ToggleOverlayRequested?.Invoke(this, EventArgs.Empty);

        _openItem = new ToolStripMenuItem(Loc.T("Open PRISM"));
        _openItem.Font = new Font(_openItem.Font, FontStyle.Bold);
        _openItem.Click += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);

        _nextItem = new ToolStripMenuItem(Loc.T("Next crosshair"));
        _nextItem.Click += (_, _) => NextPresetRequested?.Invoke(this, EventArgs.Empty);

        _exitItem = new ToolStripMenuItem(Loc.T("Exit"));
        _exitItem.Click += (_, _) => ExitRequested?.Invoke(this, EventArgs.Empty);

        var menu = new ContextMenuStrip
        {
            BackColor = Color.FromArgb(20, 24, 34),
            ForeColor = Color.FromArgb(232, 238, 250),
            ShowImageMargin = false,
            Padding = new Padding(4)
        };
        menu.Items.Add(_openItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_toggleItem);
        menu.Items.Add(_nextItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(_exitItem);
        menu.Opening += (_, _) =>
        {
            _toggleItem.Text = Loc.T(state.Current.Visible ? "Hide crosshair" : "Show crosshair");
        };

        var icon = LoadAppIcon();
        _icon = new NotifyIcon
        {
            Icon = icon,
            Text = "PRISM — Crosshair Studio",
            Visible = true,
            ContextMenuStrip = menu
        };
        _icon.DoubleClick += (_, _) => ShowRequested?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshLanguage()
    {
        _openItem.Text = Loc.T("Open PRISM");
        _nextItem.Text = Loc.T("Next crosshair");
        _exitItem.Text = Loc.T("Exit");
        _toggleItem.Text = Loc.T("Hide crosshair");
    }

    public void ShowBalloon(string title, string message)
    {
        _icon.BalloonTipTitle = title;
        _icon.BalloonTipText = message;
        _icon.BalloonTipIcon = ToolTipIcon.Info;
        _icon.ShowBalloonTip(2800);
    }

    private static Icon LoadAppIcon()
    {
        try
        {
            var path = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var icon = Icon.ExtractAssociatedIcon(path);
                if (icon is not null)
                    return icon;
            }
        }
        catch
        {
            // fall through
        }
        return SystemIcons.Application;
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
