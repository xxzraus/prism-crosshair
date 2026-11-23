using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using PrismCrosshair.Models;

namespace PrismCrosshair.Controls;

public sealed class CrosshairView : FrameworkElement
{
    public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
        nameof(Settings), typeof(CrosshairSettings), typeof(CrosshairView),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnSettingsChanged));

    public static readonly DependencyProperty RenderScaleProperty = DependencyProperty.Register(
        nameof(RenderScale), typeof(double), typeof(CrosshairView),
        new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PixelPerfectProperty = DependencyProperty.Register(
        nameof(PixelPerfect), typeof(bool), typeof(CrosshairView),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AnimateProperty = DependencyProperty.Register(
        nameof(Animate), typeof(bool), typeof(CrosshairView),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnAnimateChanged));

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private CrosshairSettings? _subscribed;
    private bool _frameAttached;

    public CrosshairView()
    {
        IsHitTestVisible = false;
        SnapsToDevicePixels = false;
        Loaded += (_, _) => UpdateAnimation();
        Unloaded += (_, _) => DetachFrame();
    }

    public CrosshairSettings? Settings
    {
        get => (CrosshairSettings?)GetValue(SettingsProperty);
        set => SetValue(SettingsProperty, value);
    }

    public double RenderScale
    {
        get => (double)GetValue(RenderScaleProperty);
        set => SetValue(RenderScaleProperty, value);
    }

    public bool PixelPerfect
    {
        get => (bool)GetValue(PixelPerfectProperty);
        set => SetValue(PixelPerfectProperty, value);
    }

    public bool Animate
    {
        get => (bool)GetValue(AnimateProperty);
        set => SetValue(AnimateProperty, value);
    }

    private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = (CrosshairView)d;
        if (view._subscribed is not null)
        {
            view._subscribed.PropertyChanged -= view.Settings_PropertyChanged;
            view._subscribed = null;
        }
        if (e.NewValue is CrosshairSettings settings)
        {
            view._subscribed = settings;
            settings.PropertyChanged += view.Settings_PropertyChanged;
        }
        view.UpdateAnimation();
        view.InvalidateVisual();
    }

    private static void OnAnimateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((CrosshairView)d).UpdateAnimation();

    private void Settings_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CrosshairSettings.Animation) or nameof(CrosshairSettings.AnimationSpeed))
            UpdateAnimation();
        InvalidateVisual();
    }

    private void UpdateAnimation()
    {
        var shouldAnimate = Animate && IsLoaded && Settings is { Animation: not ReticleAnimation.None };
        if (shouldAnimate && !_frameAttached)
        {
            CompositionTarget.Rendering += OnFrame;
            _frameAttached = true;
        }
        else if (!shouldAnimate)
        {
            DetachFrame();
        }
    }

    private void OnFrame(object? sender, EventArgs e) => InvalidateVisual();

    private void DetachFrame()
    {
        if (!_frameAttached)
            return;
        CompositionTarget.Rendering -= OnFrame;
        _frameAttached = false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var width = double.IsInfinity(availableSize.Width) ? 220 : availableSize.Width;
        var height = double.IsInfinity(availableSize.Height) ? 220 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (Settings is null || ActualWidth <= 0 || ActualHeight <= 0)
            return;

        var s = Settings;
        var dpi = PixelPerfect ? VisualTreeHelper.GetDpi(this).DpiScaleX : 1.0;
        if (dpi <= 0)
            dpi = 1;
        var scale = Math.Max(0.05, RenderScale * s.Scale / dpi);
        var center = new Point(ActualWidth / 2, ActualHeight / 2);

        var time = _clock.Elapsed.TotalSeconds * s.AnimationSpeed;
        var rotation = s.Rotation;
        var extraScale = 1.0;
        var extraOpacity = 1.0;
        switch (s.Animation)
        {
            case ReticleAnimation.Pulse:
                extraOpacity = 0.68 + 0.32 * (0.5 + 0.5 * Math.Sin(time * 2.4));
                break;
            case ReticleAnimation.Breathe:
                extraScale = 1 + 0.055 * Math.Sin(time * 2.0);
                break;
            case ReticleAnimation.Rotate:
                rotation += time * 16;
                break;
        }

        dc.PushOpacity(Math.Clamp(s.Opacity * extraOpacity, 0, 1));
        dc.PushTransform(new RotateTransform(rotation, center.X, center.Y));
        if (Math.Abs(extraScale - 1) > 0.0001)
            dc.PushTransform(new ScaleTransform(extraScale, extraScale, center.X, center.Y));

        try
        {
            DrawReticle(dc, s, center, scale);
        }
        finally
        {
            if (Math.Abs(extraScale - 1) > 0.0001)
                dc.Pop();
            dc.Pop();
            dc.Pop();
        }
    }

    private void DrawReticle(DrawingContext dc, CrosshairSettings s, Point center, double scale)
    {
        var half = Math.Max(1.2, s.Size * 0.5 * scale);
        var gap = Math.Min(s.Gap * scale, half * 0.82);
        var thickness = Math.Max(0.65, s.Thickness * scale);
        var outlineWidth = s.Outline ? Math.Max(0.4, s.OutlineWidth * scale) : 0;
        var cap = s.RoundedCaps ? PenLineCap.Round : PenLineCap.Square;
        var lineJoin = s.RoundedCaps ? PenLineJoin.Round : PenLineJoin.Miter;

        var c1 = ParseColor(s.Color1);
        var c2 = ParseColor(s.Color2);
        var colorBrush = CreateGradientBrush(s, center, scale, c1, c2);
        var outlineBrush = CreateSolidBrush(ParseColor(s.OutlineColor), 0.95);

        var geometry = new GeometryGroup { FillRule = FillRule.Nonzero };
        var dotDiameter = Math.Max(0, s.DotSize * scale);
        var dotVisible = s.ShowDot && dotDiameter > 0.2 && s.Style != ReticleStyle.Dot;

        void AddLine(double x1, double y1, double x2, double y2)
        {
            geometry.Children.Add(new LineGeometry(
                new Point(center.X + x1, center.Y + y1),
                new Point(center.X + x2, center.Y + y2)));
        }

        void AddArm(double angleDegrees)
        {
            var rad = angleDegrees * Math.PI / 180.0;
            var dx = Math.Cos(rad);
            var dy = Math.Sin(rad);
            AddLine(dx * gap, dy * gap, dx * half, dy * half);
        }

        switch (s.Style)
        {
            case ReticleStyle.Cross:
                AddArm(180);
                AddArm(0);
                AddArm(270);
                AddArm(90);
                break;

            case ReticleStyle.TShape:
                AddArm(180);
                AddArm(0);
                AddArm(90);
                break;

            case ReticleStyle.XShape:
                var diagonalGap = gap * 0.72;
                var diagonalHalf = half * 0.88;
                AddLine(-diagonalGap, -diagonalGap, -diagonalHalf, -diagonalHalf);
                AddLine(diagonalGap, -diagonalGap, diagonalHalf, -diagonalHalf);
                AddLine(-diagonalGap, diagonalGap, -diagonalHalf, diagonalHalf);
                AddLine(diagonalGap, diagonalGap, diagonalHalf, diagonalHalf);
                break;

            case ReticleStyle.Chevron:
                AddLine(-half * 0.72, half * 0.36, 0, -half * 0.58);
                AddLine(half * 0.72, half * 0.36, 0, -half * 0.58);
                break;

            case ReticleStyle.Brackets:
                var bx = half * 0.92;
                var arm = Math.Min(half * 0.82, Math.Max(4 * scale, half * 0.38));
                AddLine(-bx, -bx, -bx + arm, -bx);
                AddLine(-bx, -bx, -bx, -bx + arm);
                AddLine(bx, -bx, bx - arm, -bx);
                AddLine(bx, -bx, bx, -bx + arm);
                AddLine(-bx, bx, -bx + arm, bx);
                AddLine(-bx, bx, -bx, bx - arm);
                AddLine(bx, bx, bx - arm, bx);
                AddLine(bx, bx, bx, bx - arm);
                break;

            case ReticleStyle.Circle:
                AddCircle(geometry, center, half * 0.88);
                dotVisible = s.ShowDot && s.DotSize > 0.1;
                break;

            case ReticleStyle.Hybrid:
                AddArm(180);
                AddArm(0);
                AddArm(270);
                AddArm(90);
                AddCircle(geometry, center, half * 0.58);
                break;

            case ReticleStyle.Dot:
                dotDiameter = Math.Max(3.4, s.Size * 0.30) * scale;
                dotVisible = true;
                break;
        }

        if (s.Glow > 0)
        {
            var glowAmount = s.Glow * scale;
            for (var pass = 4; pass >= 1; pass--)
            {
                var width = thickness + glowAmount * 0.20 * pass;
                var alpha = 0.008 + 0.008 * pass;
                var pen = new Pen(colorBrush, width)
                {
                    StartLineCap = cap,
                    EndLineCap = cap,
                    LineJoin = lineJoin
                };
                dc.PushOpacity(alpha);
                dc.DrawGeometry(null, pen, geometry);
                dc.Pop();
            }
        }

        if (outlineWidth > 0)
        {
            var pen = new Pen(outlineBrush, thickness + outlineWidth * 2)
            {
                StartLineCap = cap,
                EndLineCap = cap,
                LineJoin = lineJoin
            };
            dc.DrawGeometry(null, pen, geometry);
        }

        var colorPen = new Pen(colorBrush, thickness)
        {
            StartLineCap = cap,
            EndLineCap = cap,
            LineJoin = lineJoin
        };
        dc.DrawGeometry(null, colorPen, geometry);

        if (dotVisible)
        {
            var radius = Math.Max(0.7, dotDiameter * 0.5);
            if (s.Glow > 0)
            {
                var glowRadius = radius + s.Glow * scale * 0.28;
                dc.PushOpacity(0.22);
                dc.DrawEllipse(colorBrush, null, center, glowRadius, glowRadius);
                dc.Pop();
            }
            if (outlineWidth > 0)
            {
                var outlineRadius = radius + outlineWidth;
                dc.DrawEllipse(outlineBrush, null, center, outlineRadius, outlineRadius);
            }
            dc.DrawEllipse(colorBrush, null, center, radius, radius);
        }
    }

    private static void AddCircle(GeometryGroup group, Point center, double radius)
        => group.Children.Add(new EllipseGeometry(center, radius, radius));

    private static Brush CreateGradientBrush(CrosshairSettings s, Point center, double scale, Color c1, Color c2)
    {
        if (!s.UseGradient || c1 == c2)
            return CreateSolidBrush(c1, 1);

        var radians = s.GradientAngle * Math.PI / 180.0;
        var vector = new Vector(Math.Cos(radians), Math.Sin(radians));
        var reach = Math.Max(8, s.Size * 0.85 * scale);
        var brush = new LinearGradientBrush
        {
            MappingMode = BrushMappingMode.Absolute,
            StartPoint = center - vector * reach,
            EndPoint = center + vector * reach
        };
        brush.GradientStops.Add(new GradientStop(c1, 0));
        brush.GradientStops.Add(new GradientStop(c2, 1));
        brush.Freeze();
        return brush;
    }

    private static SolidColorBrush CreateSolidBrush(Color color, double opacity)
    {
        var brush = new SolidColorBrush(Color.FromArgb(
            (byte)Math.Clamp(opacity * 255, 0, 255), color.R, color.G, color.B));
        brush.Freeze();
        return brush;
    }

    private static Color ParseColor(string hex)
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
}
