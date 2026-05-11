namespace SepeCam.Core.Native;

public enum VideoProcAmpProperty
{
    Brightness = 0,
    Contrast = 1,
    Hue = 2,
    Saturation = 3,
    Sharpness = 4,
    Gamma = 5,
    ColorEnable = 6,
    WhiteBalance = 7,
    BacklightCompensation = 8,
    Gain = 9,
    PowerlineFrequency = 10,
}

public enum CameraControlProperty
{
    Pan = 0,
    Tilt = 1,
    Roll = 2,
    Zoom = 3,
    Exposure = 4,
    Iris = 5,
    Focus = 6,
    Flash = 7,
}

[Flags]
public enum CameraControlFlags
{
    None = 0,
    Auto = 0x0001,
    Manual = 0x0002,
}

[Flags]
public enum VideoProcAmpFlags
{
    None = 0,
    Auto = 0x0001,
    Manual = 0x0002,
}
