using System.Text.Json;

namespace SepeCam.Core;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
    };

    private readonly string _path;
    private readonly object _gate = new();

    public SettingsStore(string? overridePath = null)
    {
        _path = overridePath ?? DefaultPath;
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public static string DefaultPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SepeCam", "settings.json");

    public AppConfig Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_path)) return new AppConfig();
            try
            {
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppConfig>(json, Options) ?? new AppConfig();
            }
            catch
            {
                return new AppConfig();
            }
        }
    }

    public void Save(AppConfig config)
    {
        lock (_gate)
        {
            var json = JsonSerializer.Serialize(config, Options);
            var tmp = _path + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(_path)) File.Replace(tmp, _path, null);
            else File.Move(tmp, _path);
        }
    }

    public static string DeviceKeyFor(CameraInfo info)
    {
        var key = !string.IsNullOrWhiteSpace(info.DevicePath)
            ? info.DevicePath
            : info.MonikerDisplayName;
        return Normalize(key);
    }

    private static string Normalize(string s)
    {
        var lower = s.ToLowerInvariant();
        var idx = lower.IndexOf("#{", StringComparison.Ordinal);
        return idx > 0 ? lower[..idx] : lower;
    }
}
