using SepeCam.Core;

namespace SepeCam.App.ViewModels;

public sealed class PropertyViewModel : Observable
{
    private readonly Action<PropertyViewModel> _onChanged;
    private int _value;
    private bool _auto;
    private bool _locked;
    private bool _suppress;

    public PropertyViewModel(CameraPropertyInfo info, Action<PropertyViewModel> onChanged)
    {
        Info = info;
        _onChanged = onChanged;
    }

    public CameraPropertyInfo Info { get; }
    public string Name => Info.Name;
    public int Min => Info.Min;
    public int Max => Info.Max;
    public int Step => Info.Step;
    public int Default => Info.Default;
    public bool Supported => Info.Supported;
    public bool SupportsAuto => Info.SupportsAuto;

    public int Value
    {
        get => _value;
        set
        {
            if (Set(ref _value, Math.Clamp(value, Min, Max)) && !_suppress) _onChanged(this);
        }
    }

    public bool Auto
    {
        get => _auto;
        set { if (Set(ref _auto, value) && !_suppress) _onChanged(this); }
    }

    public bool Locked
    {
        get => _locked;
        set { if (Set(ref _locked, value) && !_suppress) _onChanged(this); }
    }

    public void SilentLoad(int value, bool auto, bool locked)
    {
        _suppress = true;
        try
        {
            Value = value;
            Auto = auto;
            Locked = locked;
        }
        finally { _suppress = false; }
    }

    public void ResetToDefault() => Value = Default;
}
