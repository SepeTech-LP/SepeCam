using SepeCam.Core;
using Xunit;
using Xunit.Abstractions;

namespace SepeCam.Core.Tests;

[Trait("Category", "Integration")]
public sealed class CameraIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public CameraIntegrationTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Enumerate_DoesNotThrow()
    {
        var cameras = CameraEnumerator.Enumerate();

        Assert.NotNull(cameras);
        _output.WriteLine($"Found {cameras.Count} camera(s)");
        foreach (var c in cameras)
            _output.WriteLine($"  - {c.FriendlyName} | path={c.DevicePath}");
    }

    [Fact]
    public void Enumerate_ReturnsDistinctMonikerNames()
    {
        var cameras = CameraEnumerator.Enumerate();

        var dupes = cameras.GroupBy(c => c.MonikerDisplayName)
            .Where(g => g.Count() > 1).ToList();
        Assert.Empty(dupes);
    }

    [SkippableFact]
    public void Open_FirstCamera_AndQueryProperties()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera attached - integration test skipped");

        using var device = CameraDevice.Open(cameras[0]);
        Assert.NotNull(device);

        var props = device!.EnumerateProperties();
        Assert.NotEmpty(props);

        int supported = props.Count(p => p.Supported);
        _output.WriteLine($"{cameras[0].FriendlyName}: {supported}/{props.Count} properties supported");

        Assert.True(supported > 0, "Camera reports zero supported properties");
    }

    [SkippableFact]
    public void GetSet_RoundTripsBrightness()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera attached");

        using var device = CameraDevice.Open(cameras[0]);
        Assert.NotNull(device);

        var brightness = device!.EnumerateProperties()
            .FirstOrDefault(p => p.Name == "Brightness" && p.Supported);
        Skip.If(brightness is null, "Brightness not supported by this camera");

        Assert.True(device.TryGet(brightness!.Kind, brightness.Id, out int original, out _));

        var target = original == brightness.Min ? brightness.Min + brightness.Step : brightness.Min;
        Assert.True(device.TrySet(brightness.Kind, brightness.Id, target, auto: false));
        Assert.True(device.TryGet(brightness.Kind, brightness.Id, out int readBack, out _));
        _output.WriteLine($"target={target} readBack={readBack} step={brightness.Step}");
        Assert.InRange(readBack, target - brightness.Step, target + brightness.Step);

        device.TrySet(brightness.Kind, brightness.Id, original, auto: false);
    }
}
