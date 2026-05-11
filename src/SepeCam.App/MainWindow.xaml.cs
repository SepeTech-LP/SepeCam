using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using SepeCam.App.ViewModels;
using SepeCam.Core;
using Application = System.Windows.Application;
using WinFormsPanel = System.Windows.Forms.Panel;

namespace SepeCam.App;

public partial class MainWindow : Window
{
    private const int WM_DEVICECHANGE = 0x0219;

    private readonly MainViewModel _vm;
    private readonly DeviceMonitor _monitor = new();
    private readonly PreviewSession _preview = new();
    private readonly WinFormsPanel _previewPanel;
    private int _previewRetries;
    private bool _previewStarted;

    public bool AllowClose { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        _vm = new MainViewModel(App.Store, App.Config);
        DataContext = _vm;

        _previewPanel = new WinFormsPanel
        {
            BackColor = System.Drawing.Color.Black,
            Dock = System.Windows.Forms.DockStyle.Fill,
        };
        PreviewHost.Child = _previewPanel;

        Loaded += OnLoaded;
        Closed += OnClosed;
        StateChanged += OnStateChanged;
        IsVisibleChanged += (_, _) => SyncPreview();
        PreviewHost.SizeChanged += (_, _) => ResizePreview();

        _vm.PreviewToggled += (_, _) => SyncPreview();
        _vm.DeviceChanged += (_, _) => SyncPreview();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var src = HwndSource.FromHwnd(helper.Handle);
        src?.AddHook(WndProc);

        _monitor.Attach(helper.Handle);
        _monitor.DevicesChanged += (_, _) => _vm.OnDeviceChange();

        _vm.Refresh();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam, ref bool handled)
    {
        if (msg == WM_DEVICECHANGE)
            _monitor.HandleMessage(msg, wparam, lparam);
        return IntPtr.Zero;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            StopPreview();
            Hide();
        }
        else
        {
            SyncPreview();
        }
    }

    private void SyncPreview()
    {
        StopPreview();

        if (!_vm.PreviewEnabled || _vm.CurrentCamera is null || !IsVisible)
        {
            PreviewHost.Visibility = Visibility.Collapsed;
            PreviewOverlay.Visibility = Visibility.Visible;
            PreviewOverlay.Text = _vm.CurrentCamera is null
                ? (_vm.Cameras.Count == 0 ? "No camera detected" : "Select a camera")
                : "Preview disabled";
            return;
        }

        PreviewOverlay.Visibility = Visibility.Collapsed;
        PreviewHost.Visibility = Visibility.Visible;

        _previewRetries = 0;
        Dispatcher.BeginInvoke(new Action(TryStartPreview), DispatcherPriority.Loaded);
    }

    private void TryStartPreview()
    {
        if (_vm.CurrentCamera is null || !_vm.PreviewEnabled || !IsVisible) return;
        if (_previewStarted) return;

        var width = (int)PreviewHost.ActualWidth;
        var height = (int)PreviewHost.ActualHeight;
        if (width < 16 || height < 16)
        {
            if (_previewRetries++ < 20)
                Dispatcher.BeginInvoke(new Action(TryStartPreview), DispatcherPriority.Loaded);
            return;
        }

        if (!_previewPanel.IsHandleCreated) _previewPanel.CreateControl();
        var handle = _previewPanel.Handle;
        if (handle == IntPtr.Zero) return;

        bool ok = _preview.Start(_vm.CurrentCamera, handle, width, height);
        if (ok)
        {
            _previewStarted = true;
        }
        else
        {
            PreviewHost.Visibility = Visibility.Collapsed;
            PreviewOverlay.Visibility = Visibility.Visible;
            PreviewOverlay.Text = "Preview unavailable";
        }
    }

    private void StopPreview()
    {
        _preview.Stop();
        _previewStarted = false;
    }

    private void ResizePreview()
    {
        if (!_previewStarted) return;
        var width = (int)PreviewHost.ActualWidth;
        var height = (int)PreviewHost.ActualHeight;
        if (width > 0 && height > 0) _preview.Resize(width, height);
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        if (AllowClose) return;
        e.Cancel = true;
        StopPreview();
        Hide();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _preview.Dispose();
        _monitor.Dispose();
        _vm.Dispose();
    }
}
