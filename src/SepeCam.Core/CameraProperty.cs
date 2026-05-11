using SepeCam.Core.Native;

namespace SepeCam.Core;

public enum PropertyKind
{
    VideoProcAmp,
    CameraControl,
}

public sealed class CameraPropertyInfo
{
    public required string Name { get; init; }
    public required PropertyKind Kind { get; init; }
    public required int Id { get; init; }
    public int Min { get; init; }
    public int Max { get; init; }
    public int Step { get; init; }
    public int Default { get; init; }
    public bool SupportsAuto { get; init; }
    public bool Supported { get; init; }

    public static readonly (PropertyKind kind, int id, string name)[] Catalog =
    [
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Brightness, "Brightness"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Contrast, "Contrast"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Hue, "Hue"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Saturation, "Saturation"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Sharpness, "Sharpness"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Gamma, "Gamma"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.WhiteBalance, "White Balance"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.BacklightCompensation, "Backlight"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.Gain, "Gain"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.ColorEnable, "Color Enable"),
        (PropertyKind.VideoProcAmp, (int)VideoProcAmpProperty.PowerlineFrequency, "Powerline Frequency"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Zoom, "Zoom"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Focus, "Focus"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Exposure, "Exposure"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Iris, "Iris"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Pan, "Pan"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Tilt, "Tilt"),
        (PropertyKind.CameraControl, (int)CameraControlProperty.Roll, "Roll"),
    ];
}

public sealed class PropertyValue
{
    public int Value { get; set; }
    public bool Auto { get; set; }
}
