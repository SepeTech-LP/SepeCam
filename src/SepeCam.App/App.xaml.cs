using System.IO;
using System.Threading;
using System.Windows;
using SepeCam.Core;
using Application = System.Windows.Application;

namespace SepeCam.App;

public partial class App : Application
{
    private static Mutex? _singleInstance;
    public static SettingsStore Store { get; } = new();
    public static AppConfig Config { get; private set; } = new();

    private TrayService? _tray;
    private MainWindow? _main;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        _singleInstance = new Mutex(initiallyOwned: true, "SepeCam.SingleInstance", out bool created);
        if (!created)
        {
            Shutdown();
            return;
        }

        bool isFirstRun = !File.Exists(SettingsStore.DefaultPath);
        Config = Store.Load();

        try { SettingsApplier.ApplyToAllConnected(Config); } catch { }

        AutoStartup.RefreshPathIfRegistered();
        if (isFirstRun) StartMenuShortcut.Install();
        else StartMenuShortcut.RefreshIfInstalled();

        _tray = new TrayService();
        _tray.OpenRequested += (_, _) => ShowMain();
        _tray.ExitRequested += (_, _) =>
        {
            if (_main is not null)
            {
                _main.AllowClose = true;
                _main.Close();
            }
            _tray?.Dispose();
            Shutdown();
        };

        bool argMinimized = Array.Exists(e.Args,
            a => string.Equals(a, "--minimized", StringComparison.OrdinalIgnoreCase));
        bool startMinimized = argMinimized || Config.StartMinimized;

        if (!startMinimized) ShowMain();
    }

    public void ShowMain()
    {
        if (_main is null)
        {
            _main = new MainWindow();
            _main.Closed += (_, _) => _main = null;
        }
        _main.Show();
        if (_main.WindowState == WindowState.Minimized) _main.WindowState = WindowState.Normal;
        _main.Activate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try { Store.Save(Config); } catch { }
        _tray?.Dispose();
        _singleInstance?.Dispose();
        base.OnExit(e);
    }
}
