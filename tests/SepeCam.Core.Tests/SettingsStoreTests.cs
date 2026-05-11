using SepeCam.Core;
using SepeCam.Core.Native;
using Xunit;

namespace SepeCam.Core.Tests;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _path;

    public SettingsStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "SepeCam.Tests." + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _path = Path.Combine(_tempDir, "settings.json");
    }

    [Fact]
    public void Load_ReturnsEmpty_WhenFileMissing()
    {
        var store = new SettingsStore(_path);
        var cfg = store.Load();

        Assert.NotNull(cfg);
        Assert.Empty(cfg.Devices);
        Assert.Null(cfg.LastSelectedDeviceKey);
    }

    [Fact]
    public void Load_ReturnsEmpty_WhenFileCorrupt()
    {
        File.WriteAllText(_path, "{this is not valid json");
        var store = new SettingsStore(_path);

        var cfg = store.Load();

        Assert.NotNull(cfg);
        Assert.Empty(cfg.Devices);
    }

    [Fact]
    public void Save_Then_Load_RoundTripsProfile()
    {
        var store = new SettingsStore(_path);
        var cfg = new AppConfig
        {
            LastSelectedDeviceKey = "usb#vid_1234",
            StartMinimized = true,
        };
        var profile = cfg.GetOrCreate("usb#vid_1234", "Test Cam");
        var p1 = profile.Upsert(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness);
        p1.Value = 42;
        p1.Auto = false;
        p1.Locked = true;
        var p2 = profile.Upsert(PropertyKind.CameraControl, (int)CameraControlProperty.Exposure);
        p2.Value = -7;
        p2.Auto = true;
        p2.Locked = false;
        profile.LastUpdated = new DateTime(2026, 5, 11, 12, 0, 0, DateTimeKind.Utc);

        store.Save(cfg);

        var loaded = store.Load();

        Assert.Single(loaded.Devices);
        Assert.Equal("usb#vid_1234", loaded.LastSelectedDeviceKey);
        Assert.True(loaded.StartMinimized);

        var lp = loaded.Devices[0];
        Assert.Equal("Test Cam", lp.FriendlyName);
        Assert.Equal(2, lp.Properties.Count);

        var brightness = lp.Find(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness);
        Assert.NotNull(brightness);
        Assert.Equal(42, brightness!.Value);
        Assert.True(brightness.Locked);
        Assert.False(brightness.Auto);

        var exposure = lp.Find(PropertyKind.CameraControl, (int)CameraControlProperty.Exposure);
        Assert.NotNull(exposure);
        Assert.Equal(-7, exposure!.Value);
        Assert.True(exposure.Auto);
        Assert.False(exposure.Locked);
    }

    [Fact]
    public void Save_AtomicReplace_DoesNotLeaveTmpFile()
    {
        var store = new SettingsStore(_path);
        store.Save(new AppConfig());
        store.Save(new AppConfig());

        Assert.True(File.Exists(_path));
        Assert.False(File.Exists(_path + ".tmp"));
    }

    [Fact]
    public void Save_IsConcurrencySafe()
    {
        var store = new SettingsStore(_path);
        Parallel.For(0, 50, _ =>
        {
            var cfg = new AppConfig();
            cfg.GetOrCreate("k", "n").Upsert(PropertyKind.VideoProcAmp, 0).Value = Random.Shared.Next();
            store.Save(cfg);
        });

        var loaded = store.Load();
        Assert.NotNull(loaded);
    }

    [Theory]
    [InlineData(@"\\?\usb#vid_046d&pid_0892&mi_00#7&abc#{e5323777-f976-4f5b-9b55-b94699c46e44}",
                @"\\?\usb#vid_046d&pid_0892&mi_00#7&abc")]
    [InlineData(@"\\?\USB#VID_046D&PID_0892#serial#{6BDD1FC6-810F-11D0-BEC7-08002BE2092F}\global",
                @"\\?\usb#vid_046d&pid_0892#serial")]
    [InlineData("simple", "simple")]
    [InlineData("UPPER#nointerface", "upper#nointerface")]
    public void DeviceKeyFor_StripsInterfaceGuid_AndLowercases(string devicePath, string expected)
    {
        var info = new CameraInfo("Friendly", devicePath, "moniker");

        var key = SettingsStore.DeviceKeyFor(info);

        Assert.Equal(expected, key);
    }

    [Fact]
    public void DeviceKeyFor_FallsBackToMonikerName_WhenDevicePathEmpty()
    {
        var info = new CameraInfo("Friendly", "", @"@device:pnp:\\?\usb#VID_046D");

        var key = SettingsStore.DeviceKeyFor(info);

        Assert.Equal(@"@device:pnp:\\?\usb#vid_046d", key);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tempDir, recursive: true); } catch { }
    }
}
