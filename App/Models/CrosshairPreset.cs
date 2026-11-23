using System.Text.Json.Serialization;
using PrismCrosshair.Localization;

namespace PrismCrosshair.Models;

public sealed class CrosshairPreset : ObservableObject
{
    private string _id = Guid.NewGuid().ToString("N");
    private string _name = "My crosshair";
    private string _category = "Mine";
    private bool _isUser = true;
    private bool _isActive;
    private CrosshairSettings _settings = new();

    public string Id { get => _id; set => Set(ref _id, value); }
    public string Name { get => _name; set => Set(ref _name, value); }
    public string Category { get => _category; set { if (Set(ref _category, value)) OnPropertyChanged(nameof(Badge)); } }
    public bool IsUser { get => _isUser; set { if (Set(ref _isUser, value)) OnPropertyChanged(nameof(Badge)); } }
    [JsonIgnore] public bool IsActive { get => _isActive; set => Set(ref _isActive, value); }
    public CrosshairSettings Settings { get => _settings; set => Set(ref _settings, value); }

    [JsonIgnore] public string Badge => Loc.T(IsUser ? "MINE" : Category.ToUpperInvariant());
    [JsonIgnore] public string Accent => Settings.Color1;

    public CrosshairPreset Clone() => new()
    {
        Id = Id,
        Name = Name,
        Category = Category,
        IsUser = IsUser,
        Settings = Settings.Clone()
    };
}

public static class PresetLibrary
{
    public static List<CrosshairPreset> CreateBuiltIns() => new()
    {
        new() { Id = "builtin-aurora", Name = "Classic", Category = "Basic", IsUser = false, Settings = new() { Name = "Classic", Style = ReticleStyle.Cross, Size = 28, Thickness = 2, Gap = 7, DotSize = 3, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 10, Outline = true, OutlineWidth = 1, GradientAngle = 45 } },
        new() { Id = "builtin-pulse", Name = "Pulse", Category = "Basic", IsUser = false, Settings = new() { Name = "Pulse", Style = ReticleStyle.Dot, Size = 14, Thickness = 2, DotSize = 5, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 14, Outline = false, GradientAngle = 135 } },
        new() { Id = "builtin-tactical", Name = "Tactical", Category = "Tactical", IsUser = false, Settings = new() { Name = "Tactical", Style = ReticleStyle.TShape, Size = 24, Thickness = 2, Gap = 5, DotSize = 0, ShowDot = false, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 0, Outline = true, OutlineWidth = 1.2, RoundedCaps = false } },
        new() { Id = "builtin-halo", Name = "Halo", Category = "Tactical", IsUser = false, Settings = new() { Name = "Halo", Style = ReticleStyle.Circle, Size = 30, Thickness = 2, Gap = 5, DotSize = 3, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 8, Outline = true, OutlineWidth = 0.8, GradientAngle = 180 } },
        new() { Id = "builtin-ion", Name = "Ion", Category = "Basic", IsUser = false, Settings = new() { Name = "Ion", Style = ReticleStyle.XShape, Size = 22, Thickness = 2, Gap = 7, DotSize = 2.5, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 6, Outline = true, OutlineWidth = 0.8, GradientAngle = 90 } },
        new() { Id = "builtin-frost", Name = "Frost", Category = "Minimal", IsUser = false, Settings = new() { Name = "Frost", Style = ReticleStyle.Brackets, Size = 26, Thickness = 1.5, Gap = 0, DotSize = 2, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 4, Outline = true, OutlineWidth = 0.8, RoundedCaps = false } },
        new() { Id = "builtin-ember", Name = "Ember", Category = "Signature", IsUser = false, Settings = new() { Name = "Ember", Style = ReticleStyle.Hybrid, Size = 30, Thickness = 2, Gap = 6, DotSize = 3, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 12, Outline = true, OutlineWidth = 1, GradientAngle = 120 } },
        new() { Id = "builtin-minimal", Name = "Minimal", Category = "Minimal", IsUser = false, Settings = new() { Name = "Minimal", Style = ReticleStyle.Dot, Size = 8, Thickness = 1.5, DotSize = 3, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 2, Outline = true, OutlineWidth = 1 } },
        new() { Id = "builtin-vector", Name = "Vector", Category = "Tactical", IsUser = false, Settings = new() { Name = "Vector", Style = ReticleStyle.Chevron, Size = 26, Thickness = 2.2, Gap = 3, DotSize = 2, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 5, Outline = true, OutlineWidth = 1, RoundedCaps = false } },
        new() { Id = "builtin-spectre", Name = "Spectre", Category = "Signature", IsUser = false, Settings = new() { Name = "Spectre", Style = ReticleStyle.Hybrid, Size = 34, Thickness = 1.8, Gap = 8, DotSize = 2.5, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 16, Outline = true, OutlineWidth = 0.8, GradientAngle = 35 } },
        new() { Id = "builtin-cross", Name = "Classic", Category = "Minimal", IsUser = false, Settings = new() { Name = "Classic", Style = ReticleStyle.Cross, Size = 22, Thickness = 2, Gap = 4, DotSize = 0, ShowDot = false, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 0, Outline = true, OutlineWidth = 1.2, RoundedCaps = false } },
        new() { Id = "builtin-ring", Name = "Orbit", Category = "Signature", IsUser = false, Settings = new() { Name = "Orbit", Style = ReticleStyle.Circle, Size = 38, Thickness = 1.5, Gap = 4, DotSize = 4, ShowDot = true, UseGradient = false, Color1 = "#FFFFFF", Color2 = "#FFFFFF", Glow = 18, Outline = true, OutlineWidth = 0.6, GradientAngle = 70 } }
    };
}
