using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace PrismCrosshair.Controls;

public sealed class ArenaBackdrop : FrameworkElement
{
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
        nameof(Mode), typeof(string), typeof(ArenaBackdrop),
        new FrameworkPropertyMetadata("Training", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(
        nameof(ShowGrid), typeof(bool), typeof(ArenaBackdrop),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(double), typeof(ArenaBackdrop),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Accent1Property = DependencyProperty.Register(
        nameof(Accent1), typeof(string), typeof(ArenaBackdrop),
        new FrameworkPropertyMetadata("#8B7CFF", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Accent2Property = DependencyProperty.Register(
        nameof(Accent2), typeof(string), typeof(ArenaBackdrop),
        new FrameworkPropertyMetadata("#3EE8C4", FrameworkPropertyMetadataOptions.AffectsRender));

    public string Mode
    {
        get => (string)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public string Accent1
    {
        get => (string)GetValue(Accent1Property);
        set => SetValue(Accent1Property, value);
    }

    public string Accent2
    {
        get => (string)GetValue(Accent2Property);
        set => SetValue(Accent2Property, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0)
            return;

        var light = string.Equals(Mode, "Light", StringComparison.OrdinalIgnoreCase);
        var dark = string.Equals(Mode, "Dark", StringComparison.OrdinalIgnoreCase);
        var a1 = Parse(Accent1);
        var a2 = Parse(Accent2);

        var radius = Math.Max(0, CornerRadius);
        if (radius > 0)
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, w, h), radius, radius));
        try
        {
            var background = light
                ? new LinearGradientBrush(Color.FromRgb(231, 238, 248), Color.FromRgb(191, 204, 224), new Point(0, 0), new Point(1, 1))
                : new LinearGradientBrush(Color.FromRgb(9, 9, 11), Color.FromRgb(20, 20, 23), new Point(0, 0), new Point(1, 1));
            background.Freeze();
            dc.DrawRectangle(background, null, new Rect(0, 0, w, h));

            DrawGlow(dc, new Point(w * 0.18, h * 0.18), Math.Max(w, h) * 0.48, a1, light ? (byte)40 : (byte)54);
            DrawGlow(dc, new Point(w * 0.84, h * 0.72), Math.Max(w, h) * 0.42, a2, light ? (byte)32 : (byte)42);
            DrawGlow(dc, new Point(w * 0.62, h * 0.12), Math.Max(w, h) * 0.3, a1, light ? (byte)14 : (byte)18);

            if (ShowGrid)
                DrawGrid(dc, w, h, light);

            if (!dark)
                DrawRange(dc, w, h, light, a1, a2);

            DrawHud(dc, w, h, light, a1, a2);

            var vignette = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.47),
                GradientOrigin = new Point(0.5, 0.47),
                RadiusX = 0.78,
                RadiusY = 0.82
            };
            vignette.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.48));
            vignette.GradientStops.Add(new GradientStop(Color.FromArgb(light ? (byte)18 : (byte)100, 7, 7, 9), 1));
            vignette.Freeze();
            dc.DrawRectangle(vignette, null, new Rect(0, 0, w, h));
        }
        finally
        {
            if (radius > 0)
                dc.Pop();
        }
    }

    private static void DrawGrid(DrawingContext dc, double w, double h, bool light)
    {
        var minor = new Pen(new SolidColorBrush(light ? Color.FromArgb(28, 74, 93, 120) : Color.FromArgb(20, 150, 150, 158)), 1);
        var major = new Pen(new SolidColorBrush(light ? Color.FromArgb(42, 74, 93, 120) : Color.FromArgb(30, 150, 150, 158)), 1);
        minor.Freeze();
        major.Freeze();

        const double step = 40;
        for (double x = step; x < w; x += step)
            dc.DrawLine(Math.Abs(x % (step * 4)) < 0.1 ? major : minor, new Point(x, 0), new Point(x, h));
        for (double y = step; y < h; y += step)
            dc.DrawLine(Math.Abs(y % (step * 4)) < 0.1 ? major : minor, new Point(0, y), new Point(w, y));
    }

    private static void DrawRange(DrawingContext dc, double w, double h, bool light, Color a1, Color a2)
    {
        var center = new Point(w * 0.5, h * 0.5);
        var unit = Math.Min(w, h);
        var baseColor = light ? Color.FromArgb(95, 86, 105, 136) : Color.FromArgb(115, 150, 150, 158);

        for (var i = 1; i <= 3; i++)
        {
            var radius = unit * (0.10 + i * 0.085);
            var pen = new Pen(new SolidColorBrush(Color.FromArgb((byte)Math.Max(30, baseColor.A - i * 16), baseColor.R, baseColor.G, baseColor.B)), i == 2 ? 1.4 : 1)
            {
                DashStyle = i == 2 ? new DashStyle(new double[] { 4, 7 }, 0) : DashStyles.Solid
            };
            dc.DrawEllipse(null, pen, center, radius, radius);
        }

        var axis = new Pen(new SolidColorBrush(light ? Color.FromArgb(30, 70, 90, 120) : Color.FromArgb(42, 150, 150, 160)), 1);
        dc.DrawLine(axis, new Point(center.X - unit * 0.42, center.Y), new Point(center.X + unit * 0.42, center.Y));
        dc.DrawLine(axis, new Point(center.X, center.Y - unit * 0.42), new Point(center.X, center.Y + unit * 0.42));

        for (var i = 0; i < 24; i++)
        {
            var angle = i * Math.PI * 2 / 24;
            var inner = unit * 0.365;
            var outer = inner + (i % 6 == 0 ? 11 : 5);
            var color = i % 6 == 0
                ? (light ? Color.FromArgb(110, 70, 90, 125) : Color.FromArgb(130, 150, 150, 160))
                : (light ? Color.FromArgb(55, 70, 90, 125) : Color.FromArgb(75, 150, 150, 160));
            var pen = new Pen(new SolidColorBrush(color), 1);
            dc.DrawLine(pen,
                new Point(center.X + Math.Cos(angle) * inner, center.Y + Math.Sin(angle) * inner),
                new Point(center.X + Math.Cos(angle) * outer, center.Y + Math.Sin(angle) * outer));
        }

        var rangePen = new Pen(new SolidColorBrush(light ? Color.FromArgb(30, 70, 90, 120) : Color.FromArgb(50, 130, 130, 140)), 1);
        var horizon = h * 0.79;
        dc.DrawLine(rangePen, new Point(0, horizon), new Point(w, horizon));
        for (var i = -8; i <= 8; i++)
        {
            var x = w * 0.5 + i * w * 0.065;
            dc.DrawLine(rangePen, new Point(w * 0.5 + i * w * 0.018, horizon), new Point(x, h));
        }

        DrawPlate(dc, new Point(w * 0.17, h * 0.62), 26, a1, light);
        DrawPlate(dc, new Point(w * 0.83, h * 0.59), 20, a2, light);
        DrawPlate(dc, new Point(w * 0.69, h * 0.27), 14, a1, light);
    }

    private static void DrawPlate(DrawingContext dc, Point center, double size, Color accent, bool light)
    {
        var fill = new SolidColorBrush(light ? Color.FromArgb(36, accent.R, accent.G, accent.B) : Color.FromArgb(30, accent.R, accent.G, accent.B));
        var pen = new Pen(new SolidColorBrush(light ? Color.FromArgb(90, accent.R, accent.G, accent.B) : Color.FromArgb(120, accent.R, accent.G, accent.B)), 1.2);
        fill.Freeze();
        pen.Freeze();
        dc.DrawEllipse(fill, pen, center, size, size);
        dc.DrawEllipse(null, new Pen(new SolidColorBrush(light ? Color.FromArgb(55, accent.R, accent.G, accent.B) : Color.FromArgb(80, accent.R, accent.G, accent.B)), 1), center, size * 0.62, size * 0.62);
        dc.DrawEllipse(new SolidColorBrush(light ? Color.FromArgb(110, accent.R, accent.G, accent.B) : Color.FromArgb(180, accent.R, accent.G, accent.B)), null, center, 2.2, 2.2);
    }

    private void DrawHud(DrawingContext dc, double w, double h, bool light, Color a1, Color a2)
    {
        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var primary = new SolidColorBrush(light ? Color.FromArgb(150, 53, 69, 92) : Color.FromArgb(155, 185, 185, 192));
        var faint = new SolidColorBrush(light ? Color.FromArgb(90, 65, 82, 106) : Color.FromArgb(100, 135, 135, 143));
        primary.Freeze();
        faint.Freeze();

        var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
        var text = new FormattedText("TRAINING RANGE  //  SECTOR 07", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 10, primary, dpi);
        dc.DrawText(text, new Point(22, 18));

        var sub = new FormattedText("PRISM TEST ENVIRONMENT", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 9, faint, dpi);
        dc.DrawText(sub, new Point(22, 35));

        var range = new FormattedText("100 M", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 10, primary, dpi);
        dc.DrawText(range, new Point(w - range.Width - 22, h - 34));

        var live = new FormattedText("● LIVE", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, 9,
            new SolidColorBrush(Color.FromArgb(200, a2.R, a2.G, a2.B)), dpi);
        dc.DrawText(live, new Point(w - live.Width - 22, 18));

        var pen = new Pen(new SolidColorBrush(light ? Color.FromArgb(75, 70, 90, 120) : Color.FromArgb(100, 140, 140, 148)), 1.2);
        var len = 18.0;
        dc.DrawLine(pen, new Point(16, 16), new Point(16 + len, 16));
        dc.DrawLine(pen, new Point(16, 16), new Point(16, 16 + len));
        dc.DrawLine(pen, new Point(w - 16, 16), new Point(w - 16 - len, 16));
        dc.DrawLine(pen, new Point(w - 16, 16), new Point(w - 16, 16 + len));
        dc.DrawLine(pen, new Point(16, h - 16), new Point(16 + len, h - 16));
        dc.DrawLine(pen, new Point(16, h - 16), new Point(16, h - 16 - len));
        dc.DrawLine(pen, new Point(w - 16, h - 16), new Point(w - 16 - len, h - 16));
        dc.DrawLine(pen, new Point(w - 16, h - 16), new Point(w - 16, h - 16 - len));
    }

    private static void DrawGlow(DrawingContext dc, Point center, double radius, Color color, byte alpha)
    {
        var brush = new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            GradientOrigin = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(alpha, color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1));
        brush.Freeze();
        dc.DrawEllipse(brush, null, center, radius, radius);
    }

    private static Color Parse(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return Colors.White; }
    }
}
