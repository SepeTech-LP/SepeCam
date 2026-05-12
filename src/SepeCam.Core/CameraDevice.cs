using System.Runtime.InteropServices;
using SepeCam.Core.Native;

namespace SepeCam.Core;

public sealed class CameraDevice : IDisposable
{
    private IBaseFilter? _filter;
    private IAMVideoProcAmp? _procAmp;
    private IAMCameraControl? _cameraControl;
    private bool _disposed;

    public CameraInfo Info { get; }

    private CameraDevice(CameraInfo info, IBaseFilter filter)
    {
        Info = info;
        _filter = filter;
        _procAmp = filter as IAMVideoProcAmp;
        _cameraControl = filter as IAMCameraControl;
    }

    public static CameraDevice? Open(CameraInfo info)
    {
        var filter = CameraEnumerator.CreateFilter(info.MonikerDisplayName);
        return filter is null ? null : new CameraDevice(info, filter);
    }

    internal IBaseFilter? Filter => _filter;

    public IReadOnlyList<CameraPropertyInfo> EnumerateProperties()
    {
        var list = new List<CameraPropertyInfo>();
        foreach (var (kind, id, name) in CameraPropertyInfo.Catalog)
        {
            if (TryGetRange(kind, id, out int min, out int max, out int step, out int def, out bool supportsAuto))
            {
                list.Add(new CameraPropertyInfo
                {
                    Name = name,
                    Kind = kind,
                    Id = id,
                    Min = min,
                    Max = max,
                    Step = step <= 0 ? 1 : step,
                    Default = def,
                    SupportsAuto = supportsAuto,
                    Supported = true,
                });
            }
            else
            {
                list.Add(new CameraPropertyInfo
                {
                    Name = name,
                    Kind = kind,
                    Id = id,
                    Supported = false,
                });
            }
        }
        return list;
    }

    public bool TryGetRange(PropertyKind kind, int id, out int min, out int max,
        out int step, out int def, out bool supportsAuto)
    {
        min = max = step = def = 0;
        supportsAuto = false;
        try
        {
            if (kind == PropertyKind.VideoProcAmp && _procAmp is not null)
            {
                int hr = _procAmp.GetRange((VideoProcAmpProperty)id, out min, out max,
                    out step, out def, out var flags);
                if (hr == 0)
                {
                    supportsAuto = (flags & VideoProcAmpFlags.Auto) != 0;
                    return true;
                }
            }
            else if (kind == PropertyKind.CameraControl && _cameraControl is not null)
            {
                int hr = _cameraControl.GetRange((CameraControlProperty)id, out min, out max,
                    out step, out def, out var flags);
                if (hr == 0)
                {
                    supportsAuto = (flags & CameraControlFlags.Auto) != 0;
                    return true;
                }
            }
        }
        catch (COMException) { }
        return false;
    }

    public bool TryGet(PropertyKind kind, int id, out int value, out bool isAuto)
    {
        value = 0;
        isAuto = false;
        try
        {
            if (kind == PropertyKind.VideoProcAmp && _procAmp is not null)
            {
                int hr = _procAmp.Get((VideoProcAmpProperty)id, out value, out var f);
                isAuto = (f & VideoProcAmpFlags.Auto) != 0;
                return hr == 0;
            }
            if (kind == PropertyKind.CameraControl && _cameraControl is not null)
            {
                int hr = _cameraControl.Get((CameraControlProperty)id, out value, out var f);
                isAuto = (f & CameraControlFlags.Auto) != 0;
                return hr == 0;
            }
        }
        catch (COMException) { }
        return false;
    }

    public bool TrySet(PropertyKind kind, int id, int value, bool auto)
    {
        try
        {
            if (kind == PropertyKind.VideoProcAmp && _procAmp is not null)
            {
                var flags = auto ? VideoProcAmpFlags.Auto : VideoProcAmpFlags.Manual;
                return _procAmp.Set((VideoProcAmpProperty)id, value, flags) == 0;
            }
            if (kind == PropertyKind.CameraControl && _cameraControl is not null)
            {
                var flags = auto ? CameraControlFlags.Auto : CameraControlFlags.Manual;
                return _cameraControl.Set((CameraControlProperty)id, value, flags) == 0;
            }
        }
        catch (COMException) { }
        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _procAmp = null;
        _cameraControl = null;
        if (_filter is not null)
        {
            try { Marshal.FinalReleaseComObject(_filter); } catch { }
            _filter = null;
        }
    }
}
