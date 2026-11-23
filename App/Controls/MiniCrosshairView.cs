using System.Windows;
using System.Windows.Media;
using PrismCrosshair.Models;

namespace PrismCrosshair.Controls;

public sealed class MiniCrosshairView : FrameworkElement
{
    public static readonly DependencyProperty ReticleStyleProperty = DependencyProperty.Register(
        nameof(ReticleStyle), typeof(ReticleStyle), typeof(MiniCrosshairView),
        new FrameworkPropertyMetadata(ReticleStyle.Cross, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Accent1Property = DependencyProperty.Register(
        nameof(Accent1), typeof(string), typeof(MiniCrosshairView),
        new FrameworkPropertyMetadata("#8B7CFF", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Accent2Property = DependencyProperty.Register(
        nameof(Accent2), typeof(string), typeof(MiniCrosshairView),
        new FrameworkPropertyMetadata("#3EE8C4", FrameworkPropertyMetadataOptions.AffectsRender));

    public ReticleStyle ReticleStyle
    {
        get => (ReticleStyle)GetValue(ReticleStyleProperty);
        set => SetValue(ReticleStyleProperty, value);
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

    protected override Size MeasureOverride(Size availableSize) => new(32, 32);

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var c1 = Parse(Accent1);
        var c2 = Parse(Accent2);
        var brush = new LinearGradientBrush(c1, c2, new Point(0, 0), new Point(1, 1));
        brush.Freeze();
        var pen = new Pen(brush, 1.8)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
        var half = Math.Min(ActualWidth, ActualHeight) * 0.34;
        var gap = 2.6;
        var geometry = new GeometryGroup();

        void Line(double x1, double y1, double x2, double y2)
            => geometry.Children.Add(new LineGeometry(new Point(center.X + x1, center.Y + y1), new Point(center.X + x2, center.Y + y2)));

        switch (ReticleStyle)
        {
            case Models.ReticleStyle.Cross:
                Line(-gap, 0, -half, 0); Line(gap, 0, half, 0);
                Line(0, -gap, 0, -half); Line(0, gap, 0, half);
                break;
            case Models.ReticleStyle.Dot:
                dc.DrawEllipse(brush, null, center, 2.6, 2.6);
                break;
            case Models.ReticleStyle.Circle:
                dc.DrawEllipse(null, new Pen(brush, 1.7), center, half * 0.84, half * 0.84);
                dc.DrawEllipse(brush, null, center, 1.6, 1.6);
                break;
            case Models.ReticleStyle.TShape:
                Line(-gap, 0, -half, 0); Line(gap, 0, half, 0);
                Line(0, gap, 0, half);
                break;
            case Models.ReticleStyle.XShape:
                Line(-gap, -gap, -half, -half); Line(gap, -gap, half, -half);
                Line(-gap, gap, -half, half); Line(gap, gap, half, half);
                break;
            case Models.ReticleStyle.Chevron:
                Line(-half * 0.7, half * 0.32, 0, -half * 0.55);
                Line(half * 0.7, half * 0.32, 0, -half * 0.55);
                break;
            case Models.ReticleStyle.Brackets:
                var b = half * 0.9;
                var a = half * 0.55;
                Line(-b, -b, -b + a, -b); Line(-b, -b, -b, -b + a);
                Line(b, -b, b - a, -b); Line(b, -b, b, -b + a);
                Line(-b, b, -b + a, b); Line(-b, b, -b, b - a);
                Line(b, b, b - a, b); Line(b, b, b, b - a);
                break;
            case Models.ReticleStyle.Hybrid:
                Line(-gap, 0, -half, 0); Line(gap, 0, half, 0);
                Line(0, -gap, 0, -half); Line(0, gap, 0, half);
                dc.DrawEllipse(null, new Pen(brush, 1.2), center, half * 0.55, half * 0.55);
                break;
        }

        dc.DrawGeometry(null, pen, geometry);
        if (ReticleStyle is not (Models.ReticleStyle.Dot or Models.ReticleStyle.Circle))
            dc.DrawEllipse(brush, null, center, 1.1, 1.1);
    }

    private static Color Parse(string hex)
    {
        try { return (Color)ColorConverter.ConvertFromString(hex); }
        catch { return Colors.White; }
    }
}
