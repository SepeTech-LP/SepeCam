using Microsoft.Win32;

namespace SepeCam.App;

public static class AutoStartup
{
    private const string Key = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string Name = "SepeCam";

    public static bool IsEnabled()
    {
        try
        {
            using var run = Registry.CurrentUser.OpenSubKey(Key);
            return run?.GetValue(Name) is not null;
        }
        catch { return false; }
    }

    public static void Apply(bool enabled)
    {
        try
        {
            using var run = Registry.CurrentUser.OpenSubKey(Key, writable: true)
                ?? Registry.CurrentUser.CreateSubKey(Key);
            if (run is null) return;

            if (enabled)
            {
                var exe = Environment.ProcessPath;
                if (string.IsNullOrEmpty(exe)) return;
                run.SetValue(Name, $"\"{exe}\" --minimized");
            }
            else
            {
                if (run.GetValue(Name) is not null) run.DeleteValue(Name, throwOnMissingValue: false);
            }
        }
        catch { }
    }

    public static void RefreshPathIfRegistered()
    {
        try
        {
            using var run = Registry.CurrentUser.OpenSubKey(Key, writable: true);
            if (run is null) return;
            if (run.GetValue(Name) is null) return;
            var exe = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(exe))
                run.SetValue(Name, $"\"{exe}\" --minimized");
        }
        catch { }
    }
}
