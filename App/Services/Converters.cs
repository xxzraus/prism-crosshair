using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using PrismCrosshair.Models;

namespace PrismCrosshair.Services;

public sealed class HexToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var hex = value as string ?? "#FFFFFF";
            var color = (Color)ColorConverter.ConvertFromString(hex);
            if (parameter is string p && byte.TryParse(p, out var alpha))
                color = Color.FromArgb(alpha, color.R, color.G, color.B);
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
        catch
        {
            return Brushes.White;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class GradientBrushConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var c1 = (Color)ColorConverter.ConvertFromString(values[0] as string ?? "#8B7CFF");
            var c2 = (Color)ColorConverter.ConvertFromString(values[1] as string ?? "#3EE8C4");
            var angle = values.Length > 2 && values[2] is double d ? d : 45;
            var radians = angle * Math.PI / 180.0;
            var dx = Math.Cos(radians) / 2;
            var dy = Math.Sin(radians) / 2;
            var brush = new LinearGradientBrush(c1, c2, new Point(0.5 - dx, 0.5 - dy), new Point(0.5 + dx, 0.5 + dy));
            brush.Freeze();
            return brush;
        }
        catch
        {
            return Brushes.White;
        }
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public sealed class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var flag = value is true;
        if (parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase))
            flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible;
}

public sealed class EmptyStringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class OpacityBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(value as string ?? "#8B7CFF");
            var alpha = parameter is string p && byte.TryParse(p, out var a) ? a : (byte)40;
            var brush = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
            brush.Freeze();
            return brush;
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class ThemeBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var theme = ThemeService.Themes.FirstOrDefault(t => t.Key == value as string) ?? ThemeService.Themes[0];
        var c1 = ThemeService.Parse(theme.Accent1);
        var c2 = ThemeService.Parse(theme.Accent2);
        var brush = new LinearGradientBrush(c1, c2, new Point(0, 0), new Point(1, 1));
        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class AccentPreviewBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            var color = (Color)ColorConverter.ConvertFromString(value as string ?? "#8B7CFF");
            var brush = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
            };
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(28, color.R, color.G, color.B), 0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(9, color.R, color.G, color.B), 0.56));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1));
            brush.Freeze();
            return brush;
        }
        catch
        {
            return Brushes.Transparent;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

public sealed class MultiplyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var number = value is double d ? d : 0;
        var factor = 1.0;
        if (parameter is string text)
            double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out factor);
        return number * factor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}
