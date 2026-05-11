using SepeCam.Core;
using SepeCam.Core.Native;
using Xunit;

namespace SepeCam.Core.Tests;

public sealed class AppConfigTests
{
    [Fact]
    public void GetOrCreate_AddsNewProfile_WhenKeyMissing()
    {
        var cfg = new AppConfig();

        var profile = cfg.GetOrCreate("key-1", "Camera 1");

        Assert.Single(cfg.Devices);
        Assert.Equal("key-1", profile.DeviceKey);
        Assert.Equal("Camera 1", profile.FriendlyName);
    }

    [Fact]
    public void GetOrCreate_ReturnsExisting_WhenKeyPresent()
    {
        var cfg = new AppConfig();
        var first = cfg.GetOrCreate("key-1", "Camera 1");

        var second = cfg.GetOrCreate("key-1", "Camera 1");

        Assert.Same(first, second);
        Assert.Single(cfg.Devices);
    }

    [Fact]
    public void GetOrCreate_UpdatesFriendlyName_WhenProvided()
    {
        var cfg = new AppConfig();
        cfg.GetOrCreate("key-1", "Old Name");

        var profile = cfg.GetOrCreate("key-1", "New Name");

        Assert.Equal("New Name", profile.FriendlyName);
    }

    [Fact]
    public void GetOrCreate_PreservesFriendlyName_WhenEmptyProvided()
    {
        var cfg = new AppConfig();
        cfg.GetOrCreate("key-1", "Original");

        var profile = cfg.GetOrCreate("key-1", "");

        Assert.Equal("Original", profile.FriendlyName);
    }

    [Fact]
    public void Find_ReturnsNull_WhenAbsent()
    {
        var profile = new DeviceProfile();

        var result = profile.Find(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness);

        Assert.Null(result);
    }

    [Fact]
    public void Upsert_AddsNew_AndReturnsExisting()
    {
        var profile = new DeviceProfile();

        var added = profile.Upsert(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness);
        added.Value = 5;
        var fetched = profile.Upsert(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness);

        Assert.Same(added, fetched);
        Assert.Equal(5, fetched.Value);
        Assert.Single(profile.Properties);
    }

    [Fact]
    public void Upsert_DistinguishesKindAndId()
    {
        var profile = new DeviceProfile();

        profile.Upsert(PropertyKind.VideoProcAmp, 0);
        profile.Upsert(PropertyKind.CameraControl, 0);
        profile.Upsert(PropertyKind.VideoProcAmp, 1);

        Assert.Equal(3, profile.Properties.Count);
    }
}
