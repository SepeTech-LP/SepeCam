using SepeCam.Core;
using SepeCam.Core.Native;
using Xunit;
using Xunit.Abstractions;

namespace SepeCam.Core.Tests;

[Trait("Category", "Integration")]
public sealed class PersistenceReapplyTests
{
    private readonly ITestOutputHelper _output;
    public PersistenceReapplyTests(ITestOutputHelper output) => _output = output;

    [SkippableFact]
    public void ApplyToAllConnected_PushesStoredValue_ToRealCamera()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera attached");

        var cam = cameras[0];
        var key = SettingsStore.DeviceKeyFor(cam);

        int original;
        int min, max, step, def;
        using (var probe = CameraDevice.Open(cam))
        {
            Assert.NotNull(probe);
            Skip.IfNot(probe!.TryGetRange(PropertyKind.VideoProcAmp,
                (int)VideoProcAmpProperty.Brightness, out min, out max, out step, out def, out _),
                "Brightness not supported");
            Assert.True(probe.TryGet(PropertyKind.VideoProcAmp,
                (int)VideoProcAmpProperty.Brightness, out original, out _));
        }
        if (step <= 0) step = 1;

        int target = original == min ? min + step : min;
        _output.WriteLine($"original={original} target={target} range=[{min},{max}] step={step}");

        var config = new AppConfig { LastSelectedDeviceKey = key };
        var profile = config.GetOrCreate(key, cam.FriendlyName);
        var sp = profile.Upsert(PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness);
        sp.Value = target;
        sp.Auto = false;
        sp.Locked = true;

        int applied = SettingsApplier.ApplyToAllConnected(config);
        Assert.True(applied > 0, "Nothing was applied");

        int readBack;
        using (var verify = CameraDevice.Open(cam))
        {
            Assert.NotNull(verify);
            Assert.True(verify!.TryGet(PropertyKind.VideoProcAmp,
                (int)VideoProcAmpProperty.Brightness, out readBack, out _));
            verify.TrySet(PropertyKind.VideoProcAmp,
                (int)VideoProcAmpProperty.Brightness, original, false);
        }

        _output.WriteLine($"readBack={readBack}");
        Assert.InRange(readBack, target - step, target + step);
    }
}
