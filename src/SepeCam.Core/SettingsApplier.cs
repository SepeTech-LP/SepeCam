namespace SepeCam.Core;

public static class SettingsApplier
{
    public static int Apply(CameraDevice device, DeviceProfile profile)
    {
        int applied = 0;
        foreach (var p in profile.Properties)
        {
            if (device.TrySet(p.Kind, p.Id, p.Value, p.Auto))
                applied++;
        }
        return applied;
    }

    public static int ApplyToAllConnected(AppConfig config)
    {
        int total = 0;
        var cams = CameraEnumerator.Enumerate();
        foreach (var cam in cams)
        {
            var key = SettingsStore.DeviceKeyFor(cam);
            var profile = config.Devices.FirstOrDefault(d => d.DeviceKey == key);
            if (profile is null || profile.Properties.Count == 0) continue;

            using var device = CameraDevice.Open(cam);
            if (device is null) continue;
            total += Apply(device, profile);
        }
        return total;
    }
}
