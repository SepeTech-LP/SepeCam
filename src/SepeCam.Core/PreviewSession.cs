using System.Runtime.InteropServices;
using SepeCam.Core.Native;

namespace SepeCam.Core;

public sealed class PreviewSession : IDisposable
{
    private const int WS_CHILD = 0x40000000;
    private const int WS_CLIPCHILDREN = 0x02000000;

    private IGraphBuilder? _graph;
    private ICaptureGraphBuilder2? _capture;
    private IMediaControl? _control;
    private IBaseFilter? _source;
    private IVideoWindow? _videoWindow;
    private IntPtr _hostHwnd;
    private bool _running;

    public bool Start(CameraInfo info, IntPtr hostHwnd, int width, int height)
    {
        Stop();
        try
        {
            var graphType = Type.GetTypeFromCLSID(Guids.CLSID_FilterGraph, true)!;
            _graph = (IGraphBuilder)Activator.CreateInstance(graphType)!;

            var capType = Type.GetTypeFromCLSID(Guids.CLSID_CaptureGraphBuilder2, true)!;
            _capture = (ICaptureGraphBuilder2)Activator.CreateInstance(capType)!;
            _capture.SetFiltergraph(_graph);

            _source = CameraEnumerator.CreateFilter(info.MonikerDisplayName);
            if (_source is null) return false;

            _graph.AddFilter(_source, "Source");

            var pinCat = Guids.PIN_CATEGORY_PREVIEW;
            var mediaType = Guids.MEDIATYPE_Video;
            int hr = _capture.RenderStream(ref pinCat, ref mediaType, _source, null, null);
            if (hr < 0)
            {
                pinCat = Guids.PIN_CATEGORY_CAPTURE;
                hr = _capture.RenderStream(ref pinCat, ref mediaType, _source, null, null);
                if (hr < 0) return false;
            }

            _videoWindow = _graph as IVideoWindow;
            _control = _graph as IMediaControl;
            if (_videoWindow is null || _control is null) return false;

            _hostHwnd = hostHwnd;
            _videoWindow.put_Owner(hostHwnd);
            _videoWindow.put_WindowStyle(WS_CHILD | WS_CLIPCHILDREN);
            _videoWindow.SetWindowPosition(0, 0, width, height);
            _videoWindow.put_Visible(true);

            _control.Run();
            _running = true;
            return true;
        }
        catch
        {
            Stop();
            return false;
        }
    }

    public void Resize(int width, int height)
    {
        if (_videoWindow is null) return;
        try { _videoWindow.SetWindowPosition(0, 0, width, height); } catch { }
    }

    public void Stop()
    {
        if (!_running && _graph is null) return;
        try { _control?.Stop(); } catch { }
        try
        {
            if (_videoWindow is not null)
            {
                _videoWindow.put_Visible(false);
                _videoWindow.put_Owner(IntPtr.Zero);
            }
        }
        catch { }

        if (_source is not null) { try { Marshal.ReleaseComObject(_source); } catch { } _source = null; }
        if (_capture is not null) { try { Marshal.ReleaseComObject(_capture); } catch { } _capture = null; }
        if (_graph is not null) { try { Marshal.ReleaseComObject(_graph); } catch { } _graph = null; }
        _videoWindow = null;
        _control = null;
        _running = false;
    }

    public void Dispose() => Stop();
}
