using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using PrismCrosshair.Controls;

namespace PrismCrosshair.Localization;

public static class Loc
{
    private static readonly ConditionalWeakTable<DependencyObject, Dictionary<string, string>> Originals = new();
    private static readonly ConditionalWeakTable<TextBlock, DependencyPropertyDescriptor> TextWatchers = new();
    private static Dictionary<string, string> _translations = new();
    private static bool _initialized;

    public static string CurrentLanguage { get; private set; } = "en";
    public static event EventHandler? LanguageChanged;

    public static void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;
        EventManager.RegisterClassHandler(
            typeof(FrameworkElement),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler((sender, _) => TranslateElement(sender as DependencyObject)),
            true);
    }

    public static void SetLanguage(string? language)
    {
        var normalized = string.Equals(language, "ru", StringComparison.OrdinalIgnoreCase) ? "ru" : "en";
        if (normalized == CurrentLanguage && _translations.Count > 0)
        {
            // Still notify after an explicit switch request.
            LanguageChanged?.Invoke(null, EventArgs.Empty);
            return;
        }

        CurrentLanguage = normalized;
        _translations = normalized == "ru" ? LoadTranslations("ru") : new Dictionary<string, string>(StringComparer.Ordinal);
        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string T(string english, params object?[] args)
    {
        var text = english;
        if (CurrentLanguage != "en" && _translations.TryGetValue(english, out var translated))
            text = translated;
        return args.Length == 0 ? text : string.Format(CultureInfo.CurrentCulture, text, args);
    }

    public static void ApplyTo(DependencyObject root)
    {
        if (!_initialized)
            return;
        TranslateElement(root);
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
            ApplyTo(VisualTreeHelper.GetChild(root, i));
    }

    private static void TranslateElement(DependencyObject? obj)
    {
        if (!_initialized || obj is null)
            return;

        if (CurrentLanguage == "en")
        {
            RestoreElement(obj);
            return;
        }

        if (obj is TextBlock textBlock)
        {
            ObserveText(textBlock);
            if (!string.IsNullOrWhiteSpace(textBlock.Text) &&
                BindingOperations.GetBindingExpressionBase(textBlock, TextBlock.TextProperty) is null)
            {
                TranslateProperty(obj, "Text", () => textBlock.Text, value => textBlock.Text = value);
            }
        }

        if (obj is ContentControl contentControl &&
            contentControl.Content is string content &&
            BindingOperations.GetBindingExpressionBase(contentControl, ContentControl.ContentProperty) is null)
        {
            TranslateProperty(obj, "Content", () => (string)contentControl.Content, value => contentControl.Content = value);
        }

        if (obj is ColorField colorField && !string.IsNullOrWhiteSpace(colorField.Label))
            TranslateProperty(obj, "Label", () => colorField.Label, value => colorField.Label = value);

        if (obj is Window window && !string.IsNullOrWhiteSpace(window.Title))
            TranslateProperty(obj, "Title", () => window.Title, value => window.Title = value);

        if (obj is FrameworkElement element && element.ToolTip is string tooltip && !string.IsNullOrWhiteSpace(tooltip))
        {
            TranslateProperty(obj, "ToolTip", () => (string)element.ToolTip!, value => element.ToolTip = value);
        }
    }

    private static void ObserveText(TextBlock textBlock)
    {
        if (CurrentLanguage == "en" || TextWatchers.TryGetValue(textBlock, out _))
            return;

        var descriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
        descriptor.AddValueChanged(textBlock, TextBlock_TextChanged);
        TextWatchers.Add(textBlock, descriptor);
        textBlock.Unloaded += (_, _) =>
        {
            if (TextWatchers.TryGetValue(textBlock, out var watcher))
            {
                watcher.RemoveValueChanged(textBlock, TextBlock_TextChanged);
                TextWatchers.Remove(textBlock);
            }
        };
    }

    private static void TextBlock_TextChanged(object? sender, EventArgs e)
    {
        if (CurrentLanguage == "en" || sender is not TextBlock textBlock)
            return;
        TranslateElement(textBlock);
    }

    private static void TranslateProperty(DependencyObject obj, string key, Func<string> getter, Action<string> setter)
    {
        var current = getter();
        if (string.IsNullOrEmpty(current))
            return;

        var originals = Originals.GetOrCreateValue(obj);
        if (!originals.TryGetValue(key, out var original))
        {
            original = current;
            originals[key] = original;
        }

        var translated = T(original);
        if (!string.Equals(current, translated, StringComparison.Ordinal))
            setter(translated);
    }

    private static void RestoreElement(DependencyObject obj)
    {
        if (!Originals.TryGetValue(obj, out var originals))
            return;

        if (obj is TextBlock textBlock && originals.TryGetValue("Text", out var text) &&
            BindingOperations.GetBindingExpressionBase(textBlock, TextBlock.TextProperty) is null)
            textBlock.Text = text;

        if (obj is ContentControl contentControl && originals.TryGetValue("Content", out var content) &&
            BindingOperations.GetBindingExpressionBase(contentControl, ContentControl.ContentProperty) is null)
            contentControl.Content = content;

        if (obj is ColorField colorField && originals.TryGetValue("Label", out var label))
            colorField.Label = label;

        if (obj is Window window && originals.TryGetValue("Title", out var title))
            window.Title = title;

        if (obj is FrameworkElement element && originals.TryGetValue("ToolTip", out var tooltip))
            element.ToolTip = tooltip;
    }

    private static Dictionary<string, string> LoadTranslations(string language)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = $"PrismCrosshair.Localization.{language}.json";
            using var stream = assembly.GetManifestResourceStream(name);
            if (stream is null)
                return new Dictionary<string, string>(StringComparer.Ordinal);
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            var values = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return values is null
                ? new Dictionary<string, string>(StringComparer.Ordinal)
                : new Dictionary<string, string>(values, StringComparer.Ordinal);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
