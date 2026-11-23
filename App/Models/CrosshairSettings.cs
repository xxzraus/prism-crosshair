using System.Text.Json.Serialization;
using PrismCrosshair.Localization;

namespace PrismCrosshair.Models;

public enum ReticleStyle
{
    Cross,
    Dot,
    Circle,
    TShape,
    XShape,
    Chevron,
    Brackets,
    Hybrid
}

public enum ReticleAnimation
{
    None,
    Pulse,
    Breathe,
    Rotate
}

public sealed class CrosshairSettings : ObservableObject
{
    private string _name = "Classic";
    private ReticleStyle _style = ReticleStyle.Cross;
    private double _size = 28;
    private double _thickness = 2;
    private double _gap = 6;
    private double _dotSize = 3;
    private bool _showDot = true;
    private bool _useGradient;
    private string _color1 = "#FFFFFF";
    private string _color2 = "#FFFFFF";
    private double _opacity = 1;
    private double _rotation;
    private bool _outline = true;
    private double _outlineWidth = 1;
    private string _outlineColor = "#05070C";
    private double _glow = 8;
    private double _gradientAngle = 45;
    private bool _roundedCaps = true;
    private ReticleAnimation _animation = ReticleAnimation.None;
    private double _animationSpeed = 1;
    private double _scale = 1;
    private double _offsetX;
    private double _offsetY;
    private bool _clickThrough = true;
    private bool _alwaysOnTop = true;
    private bool _visible = true;

    public string Name { get => _name; set => Set(ref _name, value); }
    public ReticleStyle Style { get => _style; set { if (Set(ref _style, value)) { OnPropertyChanged(nameof(StyleLabel)); OnPropertyChanged(nameof(IsDotStyle)); } } }
    public double Size { get => _size; set => Set(ref _size, Math.Round(Math.Clamp(value, 2, 80), 1)); }
    public double Thickness { get => _thickness; set => Set(ref _thickness, Math.Round(Math.Clamp(value, 0.5, 12), 1)); }
    public double Gap { get => _gap; set => Set(ref _gap, Math.Round(Math.Clamp(value, 0, 48), 1)); }
    public double DotSize { get => _dotSize; set => Set(ref _dotSize, Math.Round(Math.Clamp(value, 0, 16), 1)); }
    public bool ShowDot { get => _showDot; set => Set(ref _showDot, value); }
    public bool UseGradient { get => _useGradient; set => Set(ref _useGradient, value); }
    public string Color1 { get => _color1; set => Set(ref _color1, NormalizeHex(value, "#FFFFFF")); }
    public string Color2 { get => _color2; set => Set(ref _color2, NormalizeHex(value, "#FFFFFF")); }
    public double Opacity { get => _opacity; set => Set(ref _opacity, Math.Round(Math.Clamp(value, 0.05, 1), 2)); }
    public double Rotation { get => _rotation; set => Set(ref _rotation, Math.Round(Math.Clamp(value, -180, 180), 0)); }
    public bool Outline { get => _outline; set => Set(ref _outline, value); }
    public double OutlineWidth { get => _outlineWidth; set => Set(ref _outlineWidth, Math.Round(Math.Clamp(value, 0, 5), 1)); }
    public string OutlineColor { get => _outlineColor; set => Set(ref _outlineColor, NormalizeHex(value, "#05070C")); }
    public double Glow { get => _glow; set => Set(ref _glow, Math.Round(Math.Clamp(value, 0, 24), 0)); }
    public double GradientAngle { get => _gradientAngle; set => Set(ref _gradientAngle, Math.Round(value % 360)); }
    public bool RoundedCaps { get => _roundedCaps; set => Set(ref _roundedCaps, value); }
    public ReticleAnimation Animation { get => _animation; set { if (Set(ref _animation, value)) OnPropertyChanged(nameof(AnimationLabel)); } }
    public double AnimationSpeed { get => _animationSpeed; set => Set(ref _animationSpeed, Math.Round(Math.Clamp(value, 0.2, 3), 1)); }
    public double Scale { get => _scale; set => Set(ref _scale, Math.Round(Math.Clamp(value, 0.4, 3), 2)); }
    public double OffsetX { get => _offsetX; set => Set(ref _offsetX, Math.Round(Math.Clamp(value, -1920, 1920), 0)); }
    public double OffsetY { get => _offsetY; set => Set(ref _offsetY, Math.Round(Math.Clamp(value, -1080, 1080), 0)); }
    public bool ClickThrough { get => _clickThrough; set => Set(ref _clickThrough, value); }
    public bool AlwaysOnTop { get => _alwaysOnTop; set => Set(ref _alwaysOnTop, value); }
    public bool Visible { get => _visible; set { if (Set(ref _visible, value)) OnPropertyChanged(nameof(VisibilityLabel)); } }

    [JsonIgnore] public bool IsDotStyle => Style == ReticleStyle.Dot;
    [JsonIgnore] public string StyleLabel => Loc.T(Style switch
    {
        ReticleStyle.Cross => "Cross",
        ReticleStyle.Dot => "Dot",
        ReticleStyle.Circle => "Circle",
        ReticleStyle.TShape => "T-shape",
        ReticleStyle.XShape => "X",
        ReticleStyle.Chevron => "Chevron",
        ReticleStyle.Brackets => "Brackets",
        ReticleStyle.Hybrid => "Hybrid",
        _ => "Crosshair"
    });
    [JsonIgnore] public string AnimationLabel => Loc.T(Animation switch
    {
        ReticleAnimation.Pulse => "Pulse",
        ReticleAnimation.Breathe => "Breathe",
        ReticleAnimation.Rotate => "Rotate",
        _ => "No animation"
    });
    [JsonIgnore] public string VisibilityLabel => Loc.T(Visible ? "Overlay on" : "Overlay off");

    public void NotifyLocalizationChanged()
    {
        OnPropertyChanged(nameof(StyleLabel));
        OnPropertyChanged(nameof(AnimationLabel));
        OnPropertyChanged(nameof(VisibilityLabel));
    }

    public CrosshairSettings Clone() => (CrosshairSettings)MemberwiseClone();

    public void CopyFrom(CrosshairSettings source)
    {
        Name = source.Name;
        Style = source.Style;
        Size = source.Size;
        Thickness = source.Thickness;
        Gap = source.Gap;
        DotSize = source.DotSize;
        ShowDot = source.ShowDot;
        UseGradient = source.UseGradient;
        Color1 = source.Color1;
        Color2 = source.Color2;
        Opacity = source.Opacity;
        Rotation = source.Rotation;
        Outline = source.Outline;
        OutlineWidth = source.OutlineWidth;
        OutlineColor = source.OutlineColor;
        Glow = source.Glow;
        GradientAngle = source.GradientAngle;
        RoundedCaps = source.RoundedCaps;
        Animation = source.Animation;
        AnimationSpeed = source.AnimationSpeed;
        Scale = source.Scale;
        OffsetX = source.OffsetX;
        OffsetY = source.OffsetY;
        ClickThrough = source.ClickThrough;
        AlwaysOnTop = source.AlwaysOnTop;
        Visible = source.Visible;
    }

    public static string NormalizeHex(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        var text = value.Trim();
        if (!text.StartsWith('#'))
            text = "#" + text;
        if (text.Length is not (4 or 7 or 9))
            return fallback;
        foreach (var c in text.AsSpan(1))
            if (!Uri.IsHexDigit(c))
                return fallback;
        return text.ToUpperInvariant();
    }
}
