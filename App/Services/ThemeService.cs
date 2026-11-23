using System.Windows;
using System.Windows.Media;

namespace PrismCrosshair.Services;

public sealed record AppTheme(
    string Key,
    string Label,
    string Hint,
    string Accent1,
    string Accent2,
    string Accent3,
    string BgDeep,
    string Bg,
    string Surface,
    string Surface2,
    string Surface3,
    string Elevated,
    string SurfaceDeep,
    string Stroke,
    string StrokeSoft,
    string Sidebar,
    string Hero1,
    string Hero2);

public static class ThemeService
{
    public static IReadOnlyList<AppTheme> Themes { get; } = new[]
    {
        new AppTheme("Graphite", "Graphite", "graphite · silver",
            "#D4D7DC", "#858C96", "#B8BEC7",
            "#070708", "#0B0B0D", "#121214", "#18181B", "#1F2023", "#1B1C1F", "#0A0A0C",
            "#2B2D31", "#202125", "#0C0C0E", "#1A1B1E", "#0C0D0F"),
        new AppTheme("Aurora", "Aurora", "violet · mint",
            "#8B7CFF", "#3EE8C4", "#FF7AD9",
            "#08060F", "#0C0816", "#151022", "#1A142A", "#221A34", "#1E172D", "#0B0812",
            "#3B2D52", "#2A203C", "#0B0712", "#241933", "#100A18"),
        new AppTheme("Ice", "Ice", "azure · ice",
            "#5AA7FF", "#9BF3FF", "#7C8CFF",
            "#060B10", "#08111A", "#0F1B27", "#142433", "#1A2E40", "#172738", "#081018",
            "#274159", "#1B2F42", "#071018", "#102537", "#081017"),
        new AppTheme("Ember", "Sunset", "amber · coral",
            "#FFB067", "#FF6D8D", "#FFD36E",
            "#0D0705", "#140B08", "#20110C", "#291711", "#342018", "#2D1912", "#110907",
            "#4B3023", "#352117", "#120A07", "#301B12", "#130A07"),
        new AppTheme("Violet", "Ultraviolet", "orchid · neon",
            "#B06BFF", "#FF7AD9", "#6C8CFF",
            "#0B0610", "#110A18", "#1B1026", "#231630", "#2E1E3E", "#291A38", "#0F0816",
            "#49355D", "#322342", "#100814", "#2B1939", "#120A18"),
        new AppTheme("Forest", "Forest", "moss · mint",
            "#9ADF87", "#2DD4A8", "#C6F26D",
            "#070C08", "#0B120C", "#111D13", "#162618", "#1D321F", "#192B1B", "#0A100B",
            "#2B4B30", "#1F3523", "#0A110B", "#162A18", "#0A100B")
    };

    public static AppTheme Current { get; private set; } = Themes[0];

    public static void Apply(string? key)
    {
        var theme = Themes.FirstOrDefault(t => t.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) ?? Themes[0];
        Current = theme;

        var c1 = Parse(theme.Accent1);
        var c2 = Parse(theme.Accent2);
        var c3 = Parse(theme.Accent3);
        var resources = Application.Current.Resources;

        resources["AccentGradient"] = Gradient(c1, c2, 0, 0, 1, 1);
        resources["AccentGradientReverse"] = Gradient(c2, c1, 0, 0, 1, 1);
        resources["AccentGradientHorizontal"] = Gradient(c1, c2, 0, 0.5, 1, 0.5);
        resources["AccentGradientSoft"] = Gradient(WithAlpha(c1, 0x24), WithAlpha(c2, 0x12), 0, 0, 1, 1);
        resources["AccentGradientGlow"] = Gradient(WithAlpha(c1, 0x44), WithAlpha(c3, 0x1C), 0, 0, 1, 1);
        resources["AccentSolid"] = Solid(c1);
        resources["AccentSecondary"] = Solid(c2);
        resources["AccentThird"] = Solid(c3);
        resources["AccentTint"] = Solid(WithAlpha(c1, 0x16));
        resources["AccentTintStrong"] = Solid(WithAlpha(c1, 0x26));
        resources["AccentGlow"] = Solid(WithAlpha(c1, 0x3A));
        resources["AccentBorder"] = Solid(WithAlpha(c1, 0x5C));
        resources["AccentText"] = Solid(c1);
        resources["AccentTextSoft"] = Solid(WithAlpha(c1, 0xCC));
        resources["AccentContrast"] = Solid(c2);

        // Core surfaces are themed too, so switching a theme changes the whole mood,
        // not just the highlight color.
        resources["BgDeepBrush"] = Solid(Parse(theme.BgDeep));
        resources["BgBrush"] = Solid(Parse(theme.Bg));
        resources["SurfaceBrush"] = Solid(Parse(theme.Surface));
        resources["Surface2Brush"] = Solid(Parse(theme.Surface2));
        resources["Surface3Brush"] = Solid(Parse(theme.Surface3));
        resources["SurfaceElevatedBrush"] = Solid(Parse(theme.Elevated));
        resources["SurfaceDeepBrush"] = Solid(Parse(theme.SurfaceDeep));
        resources["StrokeBrush"] = Solid(Parse(theme.Stroke));
        resources["StrokeSoftBrush"] = Solid(Parse(theme.StrokeSoft));
        resources["SidebarBrush"] = Solid(WithAlpha(Parse(theme.Sidebar), 0xF2));
        resources["AppBackgroundBrush"] = Gradient(Parse(theme.Bg), Parse(theme.BgDeep), 0, 0, 1, 1);
        resources["HeroBackgroundBrush"] = Gradient(Parse(theme.Hero1), Parse(theme.Hero2), 0, 0, 1, 1);
    }

    public static Color Parse(string hex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return Colors.White;
        }
    }

    public static SolidColorBrush Solid(Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    public static LinearGradientBrush Gradient(Color a, Color b, double x1, double y1, double x2, double y2)
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(x1, y1),
            EndPoint = new Point(x2, y2)
        };
        brush.GradientStops.Add(new GradientStop(a, 0));
        brush.GradientStops.Add(new GradientStop(b, 1));
        brush.Freeze();
        return brush;
    }

    public static Color WithAlpha(Color c, byte alpha) => Color.FromArgb(alpha, c.R, c.G, c.B);
}
