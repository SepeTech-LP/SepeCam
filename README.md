# SepeCam

A portable Windows app for managing camera settings (brightness, contrast, exposure, white balance, zoom, etc.) for any DirectShow / UVC-compatible webcam. Settings are applied system-wide and are reapplied automatically when devices are reconnected or after reboot.

## Features

- All standard camera controls: brightness, contrast, hue, saturation, sharpness, gamma, white balance, backlight compensation, gain, color enable, powerline frequency, zoom, focus, exposure, iris, pan, tilt, roll.
- Per-device profiles stored in `%APPDATA%\SepeCam\settings.json`.
- Auto-reapplies saved values on device reconnect (uses `WM_DEVICECHANGE`).
- Auto-reapplies on app startup, so values survive reboots.
- Live in-app preview.
- Tray icon and "Start with Windows" option keep the watcher running in the background.
- Auto/manual flag for properties that support it.
- Single-file, fully portable EXE - no installer, no .NET runtime needed on the target machine.

## Requirements

- Windows 10 1809 or later (Windows 11 supported).
- .NET 10 SDK to build.
- **No runtime needed on the target machine** when you publish self-contained (default).

## Build a portable single-file EXE

```powershell
dotnet publish src\SepeCam.App -c Release
```

Output: `src\SepeCam.App\bin\Release\net10.0-windows\win-x64\publish\SepeCam.exe`

That single `SepeCam.exe` is the whole app. Copy it anywhere (USB, Desktop, network share) and double-click - it does not require .NET to be installed.

If you want a quick dev build instead:

```powershell
dotnet build -c Debug
```

(Dev builds run against the locally installed .NET 10 runtime - not portable.)

## How persistence works

When you change a slider, SepeCam writes the value to the camera (DirectShow `IAMVideoProcAmp` / `IAMCameraControl`) and saves it to the profile for that device. On the next app launch or whenever Windows reports a `DBT_DEVICEARRIVAL` for a camera, SepeCam reapplies the saved values.

Because the values are set through DirectShow, every other DirectShow / Media Foundation app on the system sees the new values - that is what "system-wide" means here.

The "Lock" checkbox marks a value to be re-pushed on every reconnect. The "Save Profile" button locks all current values at once.

## How devices are identified

Each device is keyed by its symbolic device path (USB serial / instance ID), stripped of the volatile interface GUID suffix. This keeps the same camera matched across reconnects even if the COM port / driver instance changes.

## Tray / autostart

Closing the window minimises to a tray icon - the watcher keeps running and reapplies settings on hot-plug. Right-click the tray icon for **Exit**.

"Start with Windows" registers a `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` entry that launches SepeCam with `--minimized`.

## License

MIT - see [LICENSE](LICENSE).
