using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using SepeCam.Core;
using Application = System.Windows.Application;

namespace SepeCam.App.ViewModels;

public sealed class MainViewModel : Observable, IDisposable
{
    private readonly SettingsStore _store;
    private readonly AppConfig _config;
    private readonly DispatcherTimer _saveDebounce;
    private readonly DispatcherTimer _setThrottle;
    private readonly Dictionary<(PropertyKind kind, int id), (int value, bool auto)> _pendingSets = new();
    private CameraDevice? _device;
    private CameraInfo? _selectedInfo;
    private string _statusText = "Ready";
    private bool _previewEnabled = true;
    private bool _launchOnStartup;
    private bool _startMinimized;
    private bool _showInStartMenu;

    public ObservableCollection<CameraInfo> Cameras { get; } = [];
    public ObservableCollection<PropertyViewModel> Properties { get; } = [];

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ResetAllCommand { get; }
    public RelayCommand SaveAllCommand { get; }
    public RelayCommand LockAllCommand { get; }
    public RelayCommand UnlockAllCommand { get; }

    public event EventHandler? PreviewToggled;
    public event EventHandler? DeviceChanged;

    public MainViewModel(SettingsStore store, AppConfig config)
    {
        _store = store;
        _config = config;
        _launchOnStartup = AutoStartup.IsEnabled();
        _startMinimized = config.StartMinimized;
        _showInStartMenu = StartMenuShortcut.IsInstalled();

        _saveDebounce = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(400),
        };
        _saveDebounce.Tick += (_, _) =>
        {
            _saveDebounce.Stop();
            try { _store.Save(_config); } catch { }
        };

        _setThrottle = new DispatcherTimer(DispatcherPriority.Input)
        {
            Interval = TimeSpan.FromMilliseconds(60),
        };
        _setThrottle.Tick += (_, _) => FlushPendingSets();

        RefreshCommand = new RelayCommand(() => Refresh());
        ResetAllCommand = new RelayCommand(ResetAll);
        SaveAllCommand = new RelayCommand(SaveCurrent);
        LockAllCommand = new RelayCommand(() => SetAllLocked(true));
        UnlockAllCommand = new RelayCommand(() => SetAllLocked(false));
    }

    private void FlushPendingSets()
    {
        _setThrottle.Stop();
        if (_device is null) { _pendingSets.Clear(); return; }
        foreach (var (key, value) in _pendingSets.ToArray())
        {
            _device.TrySet(key.kind, key.id, value.value, value.auto);
        }
        _pendingSets.Clear();
    }

    private void QueueSave()
    {
        _saveDebounce.Stop();
        _saveDebounce.Start();
    }

    public void FlushSave()
    {
        if (_saveDebounce.IsEnabled)
        {
            _saveDebounce.Stop();
            try { _store.Save(_config); } catch { }
        }
    }

    public CameraInfo? SelectedCamera
    {
        get => _selectedInfo;
        set
        {
            if (Equals(_selectedInfo, value)) return;
            _selectedInfo = value;
            Raise();
            OnCameraSelected();
        }
    }

    public string StatusText
    {
        get => _statusText;
        set => Set(ref _statusText, value);
    }

    public bool PreviewEnabled
    {
        get => _previewEnabled;
        set
        {
            if (Set(ref _previewEnabled, value))
                PreviewToggled?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool LaunchOnStartup
    {
        get => _launchOnStartup;
        set
        {
            if (!Set(ref _launchOnStartup, value)) return;
            AutoStartup.Apply(value);
        }
    }

    public bool StartMinimized
    {
        get => _startMinimized;
        set
        {
            if (!Set(ref _startMinimized, value)) return;
            _config.StartMinimized = value;
            QueueSave();
        }
    }

    public bool ShowInStartMenu
    {
        get => _showInStartMenu;
        set
        {
            if (!Set(ref _showInStartMenu, value)) return;
            if (value) StartMenuShortcut.Install();
            else StartMenuShortcut.Uninstall();
        }
    }

    public void Refresh()
    {
        var current = _selectedInfo?.MonikerDisplayName;
        Cameras.Clear();
        foreach (var c in CameraEnumerator.Enumerate()) Cameras.Add(c);

        if (Cameras.Count == 0)
        {
            SelectedCamera = null;
            StatusText = "No cameras detected";
            return;
        }

        CameraInfo? target = null;
        if (current is not null)
            target = Cameras.FirstOrDefault(c => c.MonikerDisplayName == current);
        if (target is null && _config.LastSelectedDeviceKey is not null)
        {
            target = Cameras.FirstOrDefault(c =>
                SettingsStore.DeviceKeyFor(c) == _config.LastSelectedDeviceKey);
        }
        target ??= Cameras[0];
        SelectedCamera = target;
    }

    private void OnCameraSelected()
    {
        _device?.Dispose();
        _device = null;
        Properties.Clear();

        if (_selectedInfo is null)
        {
            DeviceChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        _device = CameraDevice.Open(_selectedInfo);
        if (_device is null)
        {
            StatusText = $"Cannot open {_selectedInfo.FriendlyName}";
            DeviceChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        var key = SettingsStore.DeviceKeyFor(_selectedInfo);
        _config.LastSelectedDeviceKey = key;
        var profile = _config.GetOrCreate(key, _selectedInfo.FriendlyName);

        SettingsApplier.Apply(_device, profile);

        foreach (var info in _device.EnumerateProperties())
        {
            if (!info.Supported) continue;
            var vm = new PropertyViewModel(info, OnPropertyChanged);

            int v = info.Default;
            bool auto = false;
            _device.TryGet(info.Kind, info.Id, out v, out auto);

            var stored = profile.Find(info.Kind, info.Id);
            if (stored is not null)
            {
                v = stored.Value;
                auto = stored.Auto;
            }

            vm.SilentLoad(v, auto, stored?.Locked ?? false);
            Properties.Add(vm);
        }

        StatusText = $"{_selectedInfo.FriendlyName} — {Properties.Count} controls";
        QueueSave();
        DeviceChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnPropertyChanged(PropertyViewModel vm)
    {
        if (_selectedInfo is null) return;

        _pendingSets[(vm.Info.Kind, vm.Info.Id)] = (vm.Value, vm.Auto);
        if (!_setThrottle.IsEnabled) _setThrottle.Start();

        var key = SettingsStore.DeviceKeyFor(_selectedInfo);
        var profile = _config.GetOrCreate(key, _selectedInfo.FriendlyName);
        var stored = profile.Upsert(vm.Info.Kind, vm.Info.Id);
        stored.Value = vm.Value;
        stored.Auto = vm.Auto;
        stored.Locked = vm.Locked;
        profile.LastUpdated = DateTime.UtcNow;

        QueueSave();
    }

    public void ReleaseDevice()
    {
        FlushPendingSets();
        _device?.Dispose();
        _device = null;
    }

    public void ReacquireDevice()
    {
        if (_device is not null || _selectedInfo is null) return;
        _device = CameraDevice.Open(_selectedInfo);
    }

    private void ResetAll()
    {
        foreach (var p in Properties) p.ResetToDefault();
    }

    private void SaveCurrent()
    {
        if (_selectedInfo is null) return;
        var key = SettingsStore.DeviceKeyFor(_selectedInfo);
        var profile = _config.GetOrCreate(key, _selectedInfo.FriendlyName);
        profile.Properties.Clear();
        foreach (var p in Properties)
        {
            var s = profile.Upsert(p.Info.Kind, p.Info.Id);
            s.Value = p.Value;
            s.Auto = p.Auto;
            s.Locked = true;
            p.Locked = true;
        }
        profile.LastUpdated = DateTime.UtcNow;
        QueueSave();
        StatusText = "Saved profile for " + _selectedInfo.FriendlyName;
    }

    private void SetAllLocked(bool locked)
    {
        foreach (var p in Properties) p.Locked = locked;
    }

    public CameraInfo? CurrentCamera => _selectedInfo;

    public void OnDeviceChange()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            try { SettingsApplier.ApplyToAllConnected(_config); } catch { }
            Refresh();
        });
    }

    public void Dispose()
    {
        FlushPendingSets();
        FlushSave();
        _device?.Dispose();
        _device = null;
    }
}
