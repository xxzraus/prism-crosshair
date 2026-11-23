using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PrismCrosshair.Localization;

namespace PrismCrosshair.Controls;

public partial class ColorField : UserControl
{
    public static readonly DependencyProperty ColorHexProperty = DependencyProperty.Register(
        nameof(ColorHex), typeof(string), typeof(ColorField),
        new FrameworkPropertyMetadata("#FFFFFF", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorHexChanged));

    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(ColorField), new PropertyMetadata("Color"));

    public ObservableCollection<string> Palette { get; } = new()
    {
        // Neutrals
        "#FFFFFF", "#F5F7FA", "#D9DEE5", "#AEB6C1", "#6E7783", "#3A414B", "#1B1F25", "#080A0D",
        // Warm
        "#FF3B5C", "#FF6B7A", "#FF8FA3", "#FFC2D1", "#FF6B3D", "#FF9E64", "#FFB067", "#FFD36E",
        // Yellow / lime
        "#FFE066", "#F2C94C", "#D99A1E", "#8F5C12", "#E6FF67", "#C6F26D", "#A7FF6B", "#8FE388",
        // Green / mint
        "#5EE39A", "#3EE8C4", "#2DD4A8", "#1FA88A", "#0F6E5B", "#0B3F35", "#9BF3FF", "#58D7FF",
        // Blue / cyan
        "#3BC9F5", "#5AA7FF", "#3D7BFF", "#6C8CFF", "#2A4FA8", "#172C63", "#C7B5FF", "#B06BFF",
        // Purple / pink
        "#8B7CFF", "#7A5CFF", "#A855F7", "#7E3AF2", "#FF7AD9", "#FF4DB8", "#D9368F", "#8F1E63"
    };

    public ColorField()
    {
        InitializeComponent();
    }

    public string ColorHex
    {
        get => (string)GetValue(ColorHexProperty);
        set => SetValue(ColorHexProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnColorHexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var field = (ColorField)d;
        var normalized = Models.CrosshairSettings.NormalizeHex(e.NewValue as string, "#FFFFFF");
        if (!string.Equals(normalized, e.NewValue as string, StringComparison.Ordinal))
            field.SetCurrentValue(ColorHexProperty, normalized);
    }

    private void OpenPalette(object sender, MouseButtonEventArgs e)
    {
        PalettePopup.IsOpen = !PalettePopup.IsOpen;
        if (PalettePopup.IsOpen && PalettePopup.Child is DependencyObject child)
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() => Loc.ApplyTo(child)));
    }

    private void Swatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string hex })
        {
            SetCurrentValue(ColorHexProperty, hex);
            PalettePopup.IsOpen = false;
        }
    }

    private void HexBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        PalettePopup.IsOpen = false;
        if (sender is TextBox box)
        {
            box.Focus();
            e.Handled = false;
        }
    }

    private void HexBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (sender is TextBox box)
            {
                box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
                Keyboard.ClearFocus();
            }
            e.Handled = true;
        }
    }
}
