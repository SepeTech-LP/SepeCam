using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using SepeCam.Core.Native;

namespace SepeCam.Core;

public sealed record CameraInfo(string FriendlyName, string DevicePath, string MonikerDisplayName);

public static class CameraEnumerator
{
    public static IReadOnlyList<CameraInfo> Enumerate()
    {
        var result = new List<CameraInfo>();

        var devEnumType = Type.GetTypeFromCLSID(Guids.CLSID_SystemDeviceEnum, throwOnError: true)!;
        var devEnumObj = Activator.CreateInstance(devEnumType);
        if (devEnumObj is not ICreateDevEnum devEnum)
            return result;

        try
        {
            var category = Guids.CLSID_VideoInputDeviceCategory;
            int hr = devEnum.CreateClassEnumerator(ref category, out var enumMoniker, 0);
            if (hr != 0 || enumMoniker is null)
                return result;

            var monikers = new IMoniker[1];
            while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
            {
                var moniker = monikers[0];
                if (moniker is null) continue;

                string display = "";
                try { moniker.GetDisplayName(null!, null!, out display); }
                catch { display = ""; }

                string friendly = ReadProperty(moniker, "FriendlyName") ?? "Unknown";
                string devicePath = ReadProperty(moniker, "DevicePath") ?? display;

                result.Add(new CameraInfo(friendly, devicePath, display));
                Marshal.ReleaseComObject(moniker);
            }

            Marshal.ReleaseComObject(enumMoniker);
        }
        finally
        {
            Marshal.ReleaseComObject(devEnum);
        }

        return result;
    }

    private static string? ReadProperty(IMoniker moniker, string name)
    {
        var bagId = Guids.IID_IPropertyBag;
        try
        {
            moniker.BindToStorage(null!, null!, ref bagId, out object bagObj);
            if (bagObj is IPropertyBag bag)
            {
                object value = null!;
                if (bag.Read(name, ref value, IntPtr.Zero) == 0 && value is string s)
                {
                    Marshal.ReleaseComObject(bag);
                    return s;
                }
                Marshal.ReleaseComObject(bag);
            }
        }
        catch { }
        return null;
    }

    internal static IBaseFilter? CreateFilter(string monikerDisplayName)
    {
        var devEnumType = Type.GetTypeFromCLSID(Guids.CLSID_SystemDeviceEnum, throwOnError: true)!;
        var devEnumObj = Activator.CreateInstance(devEnumType);
        if (devEnumObj is not ICreateDevEnum devEnum) return null;

        try
        {
            var category = Guids.CLSID_VideoInputDeviceCategory;
            if (devEnum.CreateClassEnumerator(ref category, out var enumMoniker, 0) != 0 || enumMoniker is null)
                return null;

            var monikers = new IMoniker[1];
            while (enumMoniker.Next(1, monikers, IntPtr.Zero) == 0)
            {
                var moniker = monikers[0];
                if (moniker is null) continue;

                moniker.GetDisplayName(null!, null!, out string display);
                if (string.Equals(display, monikerDisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    var iidBaseFilter = Guids.IID_IBaseFilter;
                    moniker.BindToObject(null!, null!, ref iidBaseFilter, out object filterObj);
                    Marshal.ReleaseComObject(enumMoniker);
                    Marshal.ReleaseComObject(moniker);
                    return filterObj as IBaseFilter;
                }
                Marshal.ReleaseComObject(moniker);
            }
            Marshal.ReleaseComObject(enumMoniker);
        }
        finally
        {
            Marshal.ReleaseComObject(devEnum);
        }
        return null;
    }
}
