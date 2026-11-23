using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using PrismCrosshair.Models;

namespace PrismCrosshair.Services;

public sealed class PersistedState
{
    public int Version { get; set; } = 1;
    public CrosshairSettings? Current { get; set; }
    public List<CrosshairPreset> Profiles { get; set; } = new();
    public AppPreferences Preferences { get; set; } = new();
    public HotkeyConfig Hotkeys { get; set; } = new();
    public string ActivePresetId { get; set; } = "builtin-aurora";
}

public static class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PrismCrosshair");

    public static string SettingsPath => Path.Combine(DataDirectory, "settings.json");
    public static string LogPath => Path.Combine(DataDirectory, "prism.log");

    public static PersistedState Load()
    {
        try
        {
            if (!File.Exists(SettingsPath))
                return new PersistedState();

            var json = File.ReadAllText(SettingsPath);
            var state = JsonSerializer.Deserialize<PersistedState>(json, Options);
            if (state is null)
                return new PersistedState();

            state.Profiles ??= new();
            state.Preferences ??= new();
            state.Hotkeys ??= new();
            return state;
        }
        catch (Exception ex)
        {
            Log("Failed to load settings: " + ex);
            TryBackupBrokenFile();
            return new PersistedState();
        }
    }

    public static void Save(PersistedState state)
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            var temp = SettingsPath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(state, Options));
            File.Move(temp, SettingsPath, true);
        }
        catch (Exception ex)
        {
            Log("Failed to save settings: " + ex);
        }
    }

    public static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(DataDirectory);
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never take the app down.
        }
    }

    private static void TryBackupBrokenFile()
    {
        try
        {
            if (File.Exists(SettingsPath))
                File.Copy(SettingsPath, SettingsPath + ".broken", true);
        }
        catch
        {
            // ignore
        }
    }
}
