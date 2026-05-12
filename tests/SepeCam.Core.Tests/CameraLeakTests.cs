using System.Runtime.InteropServices;
using SepeCam.Core;
using SepeCam.Core.Native;
using Xunit;
using Xunit.Abstractions;

namespace SepeCam.Core.Tests;

[Trait("Category", "Integration")]
public sealed class CameraLeakTests
{
    private const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;

    private readonly ITestOutputHelper _output;
    public CameraLeakTests(ITestOutputHelper output) => _output = output;

    [SkippableFact]
    public void RapidSet_LikeSliderDrag_DoesNotLockCamera()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera");

        using var device = CameraDevice.Open(cameras[0]);
        Assert.NotNull(device);

        var brightness = device!.EnumerateProperties()
            .FirstOrDefault(p => p.Name == "Brightness" && p.Supported);
        Skip.If(brightness is null, "Brightness not supported");

        Assert.True(device.TryGet(brightness!.Kind, brightness.Id, out int original, out _));

        int failures = 0;
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < 300; i++)
        {
            int target = brightness.Min + (i % (brightness.Max - brightness.Min + 1));
            if (!device.TrySet(brightness.Kind, brightness.Id, target, auto: false)) failures++;
        }
        sw.Stop();
        _output.WriteLine($"300 Set calls in {sw.ElapsedMilliseconds}ms ({300.0 / sw.Elapsed.TotalSeconds:F1} Hz), failures={failures}");

        device.TrySet(brightness.Kind, brightness.Id, original, false);

        Assert.True(device.TryGet(brightness.Kind, brightness.Id, out _, out _),
            "Get failed after rapid Set storm — camera state is bad");
    }

    [SkippableFact]
    public void HeldDeviceAndPreview_DoesNotPreventSecondClient()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera");

        IntPtr hwnd = CreateWindowExW(0, "STATIC", "test", WS_OVERLAPPEDWINDOW,
            0, 0, 320, 240, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        try
        {
            using var heldDevice = CameraDevice.Open(cameras[0]);
            Assert.NotNull(heldDevice);

            using var sepecamPreview = new PreviewSession();
            Assert.True(sepecamPreview.Start(cameras[0], hwnd, 320, 240),
                "SepeCam's own preview failed to start");
            Thread.Sleep(300);

            using var secondClient = new PreviewSession();
            bool secondOpened = secondClient.Start(cameras[0], hwnd, 320, 240);
            _output.WriteLine($"second client opened={secondOpened}");
            Assert.True(secondOpened, "Camera is exclusively locked - second client cannot open");
        }
        finally
        {
            DestroyWindow(hwnd);
        }
    }

    [SkippableFact]
    public void AfterDispose_SecondClient_CanOpen()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera");

        IntPtr hwnd = CreateWindowExW(0, "STATIC", "test", WS_OVERLAPPEDWINDOW,
            0, 0, 320, 240, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        try
        {
            {
                using var heldDevice = CameraDevice.Open(cameras[0]);
                using var sepecamPreview = new PreviewSession();
                Assert.True(sepecamPreview.Start(cameras[0], hwnd, 320, 240));
                Thread.Sleep(300);
            }

            using var secondClient = new PreviewSession();
            bool ok = secondClient.Start(cameras[0], hwnd, 320, 240);
            _output.WriteLine($"after dispose, second client opened={ok}");
            Assert.True(ok, "Camera is still locked after SepeCam released its handles");
        }
        finally
        {
            DestroyWindow(hwnd);
        }
    }

    [SkippableFact]
    public void OpenDispose_Loop_DoesNotFail()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera");

        for (int i = 0; i < 50; i++)
        {
            using var dev = CameraDevice.Open(cameras[0]);
            Assert.NotNull(dev);
            Assert.True(dev!.TryGet(PropertyKind.VideoProcAmp,
                (int)VideoProcAmpProperty.Brightness, out _, out _),
                $"Get failed on iteration {i}");
        }
    }

    [SkippableFact]
    public void PreviewStartStop_Loop_DoesNotLockCamera()
    {
        var cameras = CameraEnumerator.Enumerate();
        Skip.If(cameras.Count == 0, "No camera");

        IntPtr hwnd = CreateWindowExW(0, "STATIC", "test", WS_OVERLAPPEDWINDOW,
            0, 0, 320, 240, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        Assert.NotEqual(IntPtr.Zero, hwnd);

        try
        {
            for (int i = 0; i < 6; i++)
            {
                using var session = new PreviewSession();
                bool started = session.Start(cameras[0], hwnd, 320, 240);
                _output.WriteLine($"iteration {i}: started={started}");
                Assert.True(started, $"Preview failed to start on iteration {i} — camera may be locked");
                Thread.Sleep(150);
            }

            using var final = new PreviewSession();
            Assert.True(final.Start(cameras[0], hwnd, 320, 240),
                "Preview cannot restart after the loop — camera is locked");
        }
        finally
        {
            DestroyWindow(hwnd);
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateWindowExW(int dwExStyle, string lpClassName,
        string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight,
        IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

    [DllImport("user32.dll")]
    private static extern bool DestroyWindow(IntPtr hwnd);
}
