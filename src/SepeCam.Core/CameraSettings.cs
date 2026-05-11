using System.Text.Json.Serialization;

namespace SepeCam.Core;

public sealed class StoredProperty
{
    public PropertyKind Kind { get; set; }
    public int Id { get; set; }
    public int Value { get; set; }
    public bool Auto { get; set; }
    public bool Locked { get; set; }
}

public sealed class DeviceProfile
{
    public string DeviceKey { get; set; } = "";
    public string FriendlyName { get; set; } = "";
    public List<StoredProperty> Properties { get; set; } = [];
    public DateTime LastUpdated { get; set; }

    public StoredProperty? Find(PropertyKind kind, int id) =>
        Properties.FirstOrDefault(p => p.Kind == kind && p.Id == id);

    public StoredProperty Upsert(PropertyKind kind, int id)
    {
        var existing = Find(kind, id);
        if (existing is not null) return existing;
        var fresh = new StoredProperty { Kind = kind, Id = id };
        Properties.Add(fresh);
        return fresh;
    }
}

public sealed class AppConfig
{
    public List<DeviceProfile> Devices { get; set; } = [];
    public bool StartMinimized { get; set; }
    public string? LastSelectedDeviceKey { get; set; }

    public DeviceProfile GetOrCreate(string key, string friendlyName)
    {
        var profile = Devices.FirstOrDefault(d => d.DeviceKey == key);
        if (profile is null)
        {
            profile = new DeviceProfile { DeviceKey = key, FriendlyName = friendlyName };
            Devices.Add(profile);
        }
        else if (!string.IsNullOrEmpty(friendlyName))
        {
            profile.FriendlyName = friendlyName;
        }
        return profile;
    }
}
