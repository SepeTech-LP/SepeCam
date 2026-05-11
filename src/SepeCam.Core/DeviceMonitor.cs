using System.Runtime.InteropServices;

namespace SepeCam.Core;

public sealed class DeviceChangeEventArgs : EventArgs
{
    public bool Arrived { get; init; }
    public IReadOnlyList<CameraInfo> Cameras { get; init; } = [];
}

public sealed class DeviceMonitor : IDisposable
{
    private static readonly Guid GuidDevInterfaceCamera =
        new("E5323777-F976-4F5B-9B55-B94699C46E44");
    private static readonly Guid GuidDevInterfaceWebcam =
        new("6BDD1FC6-810F-11D0-BEC7-08002BE2092F");

    private const int WM_DEVICECHANGE = 0x0219;
    private const int DBT_DEVICEARRIVAL = 0x8000;
    private const int DBT_DEVICEREMOVECOMPLETE = 0x8004;
    private const int DBT_DEVTYP_DEVICEINTERFACE = 0x00000005;
    private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

    private IntPtr _hwnd;
    private IntPtr _notif1;
    private IntPtr _notif2;
    private System.Threading.Timer? _debounce;

    public event EventHandler<DeviceChangeEventArgs>? DevicesChanged;

    public void Attach(IntPtr hwnd)
    {
        _hwnd = hwnd;
        _notif1 = Register(hwnd, GuidDevInterfaceCamera);
        _notif2 = Register(hwnd, GuidDevInterfaceWebcam);
    }

    public bool HandleMessage(int msg, IntPtr wparam, IntPtr lparam)
    {
        if (msg != WM_DEVICECHANGE) return false;
        int evt = wparam.ToInt32();
        if (evt != DBT_DEVICEARRIVAL && evt != DBT_DEVICEREMOVECOMPLETE) return false;

        bool arrived = evt == DBT_DEVICEARRIVAL;
        _debounce?.Dispose();
        _debounce = new System.Threading.Timer(_ =>
        {
            var cams = CameraEnumerator.Enumerate();
            DevicesChanged?.Invoke(this, new DeviceChangeEventArgs { Arrived = arrived, Cameras = cams });
        }, null, 400, System.Threading.Timeout.Infinite);
        return true;
    }

    private static IntPtr Register(IntPtr hwnd, Guid iface)
    {
        var filter = new DEV_BROADCAST_DEVICEINTERFACE
        {
            dbcc_size = Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
            dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE,
            dbcc_reserved = 0,
            dbcc_classguid = iface,
            dbcc_name = 0,
        };
        var buffer = Marshal.AllocHGlobal(filter.dbcc_size);
        try
        {
            Marshal.StructureToPtr(filter, buffer, false);
            return RegisterDeviceNotificationW(hwnd, buffer, DEVICE_NOTIFY_WINDOW_HANDLE);
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public void Dispose()
    {
        _debounce?.Dispose();
        if (_notif1 != IntPtr.Zero) UnregisterDeviceNotification(_notif1);
        if (_notif2 != IntPtr.Zero) UnregisterDeviceNotification(_notif2);
        _notif1 = _notif2 = IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DEV_BROADCAST_DEVICEINTERFACE
    {
        public int dbcc_size;
        public int dbcc_devicetype;
        public int dbcc_reserved;
        public Guid dbcc_classguid;
        public short dbcc_name;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr RegisterDeviceNotificationW(IntPtr hRecipient,
        IntPtr notificationFilter, int flags);

    [DllImport("user32.dll")]
    private static extern bool UnregisterDeviceNotification(IntPtr handle);
}
