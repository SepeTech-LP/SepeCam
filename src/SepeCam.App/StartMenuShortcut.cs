using System.IO;
using System.Runtime.InteropServices;

namespace SepeCam.App;

public static class StartMenuShortcut
{
    public static string ShortcutPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Microsoft", "Windows", "Start Menu", "Programs", "SepeCam.lnk");

    public static bool IsInstalled() => File.Exists(ShortcutPath);

    public static bool Install()
    {
        var exe = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exe)) return false;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ShortcutPath)!);
        }
        catch { return false; }

        object? shell = null;
        object? link = null;
        try
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null) return false;
            shell = Activator.CreateInstance(shellType);
            if (shell is null) return false;

            link = shellType.InvokeMember("CreateShortcut",
                System.Reflection.BindingFlags.InvokeMethod, null, shell,
                new object[] { ShortcutPath });
            if (link is null) return false;

            var linkType = link.GetType();
            linkType.InvokeMember("TargetPath",
                System.Reflection.BindingFlags.SetProperty, null, link, new object[] { exe });
            linkType.InvokeMember("WorkingDirectory",
                System.Reflection.BindingFlags.SetProperty, null, link,
                new object[] { Path.GetDirectoryName(exe) ?? "" });
            linkType.InvokeMember("Description",
                System.Reflection.BindingFlags.SetProperty, null, link,
                new object[] { "SepeCam — camera settings manager" });
            linkType.InvokeMember("IconLocation",
                System.Reflection.BindingFlags.SetProperty, null, link,
                new object[] { exe + ",0" });
            linkType.InvokeMember("Save",
                System.Reflection.BindingFlags.InvokeMethod, null, link, null);
            return true;
        }
        catch { return false; }
        finally
        {
            if (link is not null) { try { Marshal.FinalReleaseComObject(link); } catch { } }
            if (shell is not null) { try { Marshal.FinalReleaseComObject(shell); } catch { } }
        }
    }

    public static void Uninstall()
    {
        try { if (File.Exists(ShortcutPath)) File.Delete(ShortcutPath); } catch { }
    }

    public static void RefreshIfInstalled()
    {
        if (!IsInstalled()) return;
        Install();
    }
}
