using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace SepeCam.Core.Native;

[ComImport, Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICreateDevEnum
{
    [PreserveSig]
    int CreateClassEnumerator([In] ref Guid clsidDeviceClass,
        out IEnumMoniker? ppEnumMoniker, [In] int dwFlags);
}

[ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IPropertyBag
{
    [PreserveSig]
    int Read([MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
        [In, Out, MarshalAs(UnmanagedType.Struct)] ref object pVar,
        IntPtr pErrorLog);

    [PreserveSig]
    int Write([MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
        [In, MarshalAs(UnmanagedType.Struct)] ref object pVar);
}

[ComImport, Guid("56A868B1-0AD4-11CE-B03A-0020AF0BA770"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMediaControl
{
    [PreserveSig] int _GetTypeInfoCount(out int count);
    [PreserveSig] int _GetTypeInfo(int index, int lcid, out IntPtr typeInfo);
    [PreserveSig] int _GetIDsOfNames(ref Guid riid, IntPtr names, int count, int lcid, IntPtr dispIds);
    [PreserveSig] int _Invoke(int dispId, ref Guid riid, int lcid, int flags, IntPtr dispParams, IntPtr result, IntPtr excepInfo, IntPtr argErr);

    [PreserveSig] int Run();
    [PreserveSig] int Pause();
    [PreserveSig] int Stop();
    [PreserveSig] int GetState(int msTimeout, out int filterState);
    [PreserveSig] int RenderFile([MarshalAs(UnmanagedType.BStr)] string strFilename);
    [PreserveSig] int AddSourceFilter([MarshalAs(UnmanagedType.BStr)] string strFilename,
        [MarshalAs(UnmanagedType.IDispatch)] out object? ppUnk);
    [PreserveSig] int get_FilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object? ppUnk);
    [PreserveSig] int get_RegFilterCollection([MarshalAs(UnmanagedType.IDispatch)] out object? ppUnk);
    [PreserveSig] int StopWhenReady();
}

[ComImport, Guid("56A86895-0AD4-11CE-B03A-0020AF0BA770"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IBaseFilter
{
    [PreserveSig] int GetClassID(out Guid pClassID);
    [PreserveSig] int Stop();
    [PreserveSig] int Pause();
    [PreserveSig] int Run(long tStart);
    [PreserveSig] int GetState(int dwMilliSecsTimeout, out int filtState);
    [PreserveSig] int SetSyncSource([In] IntPtr pClock);
    [PreserveSig] int GetSyncSource(out IntPtr pClock);
    [PreserveSig] int EnumPins(out IntPtr ppEnum);
    [PreserveSig] int FindPin([MarshalAs(UnmanagedType.LPWStr)] string Id, out IntPtr ppPin);
    [PreserveSig] int QueryFilterInfo(IntPtr pInfo);
    [PreserveSig] int JoinFilterGraph([In] IntPtr pGraph,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pName);
    [PreserveSig] int QueryVendorInfo([MarshalAs(UnmanagedType.LPWStr)] out string pVendorInfo);
}

[ComImport, Guid("56A868A9-0AD4-11CE-B03A-0020AF0BA770"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IGraphBuilder
{
    [PreserveSig] int AddFilter([In] IBaseFilter pFilter,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pName);
    [PreserveSig] int RemoveFilter([In] IBaseFilter pFilter);
    [PreserveSig] int EnumFilters(out IntPtr ppEnum);
    [PreserveSig] int FindFilterByName([In, MarshalAs(UnmanagedType.LPWStr)] string pName,
        out IBaseFilter? ppFilter);
    [PreserveSig] int ConnectDirect([In] IntPtr ppinOut, [In] IntPtr ppinIn,
        [In] IntPtr pmt);
    [PreserveSig] int Reconnect([In] IntPtr ppin);
    [PreserveSig] int Disconnect([In] IntPtr ppin);
    [PreserveSig] int SetDefaultSyncSource();
    [PreserveSig] int Connect([In] IntPtr ppinOut, [In] IntPtr ppinIn);
    [PreserveSig] int Render([In] IntPtr ppinOut);
    [PreserveSig] int RenderFile([In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
        [In, MarshalAs(UnmanagedType.LPWStr)] string? lpcwstrPlayList);
    [PreserveSig] int AddSourceFilter([In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
        [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
        out IBaseFilter? ppFilter);
    [PreserveSig] int SetLogFile(IntPtr hFile);
    [PreserveSig] int Abort();
    [PreserveSig] int ShouldOperationContinue();
}

[ComImport, Guid("93E5A4E0-2D50-11D2-ABFA-00A0C9C6E38D"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ICaptureGraphBuilder2
{
    [PreserveSig] int SetFiltergraph([In] IGraphBuilder pfg);
    [PreserveSig] int GetFiltergraph(out IGraphBuilder? ppfg);
    [PreserveSig] int SetOutputFileName([In] ref Guid pType,
        [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrFile,
        out IBaseFilter? ppbf, out IntPtr ppSink);
    [PreserveSig] int FindInterface([In] ref Guid pCategory, [In] ref Guid pType,
        [In] IBaseFilter pbf, [In] ref Guid riid,
        [MarshalAs(UnmanagedType.IUnknown)] out object? ppint);
    [PreserveSig] int RenderStream([In] ref Guid PinCategory, [In] ref Guid MediaType,
        [In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
        [In] IBaseFilter? pfCompressor, [In] IBaseFilter? pfRenderer);
    [PreserveSig] int ControlStream([In] ref Guid pCategory, [In] ref Guid pType,
        [In] IBaseFilter pFilter, [In] IntPtr pstart, [In] IntPtr pstop,
        [In] short wStartCookie, [In] short wStopCookie);
    [PreserveSig] int AllocCapFile([In, MarshalAs(UnmanagedType.LPWStr)] string lpstr,
        [In] long dwlSize);
    [PreserveSig] int CopyCaptureFile([In, MarshalAs(UnmanagedType.LPWStr)] string lpwstrOld,
        [In, MarshalAs(UnmanagedType.LPWStr)] string lpwstrNew, [In] int fAllowEscAbort,
        [In] IntPtr pCallback);
    [PreserveSig] int FindPin([In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
        [In] int pindir, [In] ref Guid pCategory, [In] ref Guid pType,
        [In, MarshalAs(UnmanagedType.Bool)] bool fUnconnected, [In] int num,
        out IntPtr ppPin);
}

[ComImport, Guid("C6E13370-30AC-11D0-A18C-00A0C9118956"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAMCameraControl
{
    [PreserveSig] int GetRange([In] CameraControlProperty Property,
        out int pMin, out int pMax, out int pSteppingDelta,
        out int pDefault, out CameraControlFlags pCapsFlags);
    [PreserveSig] int Set([In] CameraControlProperty Property,
        [In] int lValue, [In] CameraControlFlags Flags);
    [PreserveSig] int Get([In] CameraControlProperty Property,
        out int lValue, out CameraControlFlags Flags);
}

[ComImport, Guid("C6E13360-30AC-11D0-A18C-00A0C9118956"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAMVideoProcAmp
{
    [PreserveSig] int GetRange([In] VideoProcAmpProperty Property,
        out int pMin, out int pMax, out int pSteppingDelta,
        out int pDefault, out VideoProcAmpFlags pCapsFlags);
    [PreserveSig] int Set([In] VideoProcAmpProperty Property,
        [In] int lValue, [In] VideoProcAmpFlags Flags);
    [PreserveSig] int Get([In] VideoProcAmpProperty Property,
        out int lValue, out VideoProcAmpFlags Flags);
}

[ComImport, Guid("56A868B4-0AD4-11CE-B03A-0020AF0BA770"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IVideoWindow
{
    [PreserveSig] int _GetTypeInfoCount(out int count);
    [PreserveSig] int _GetTypeInfo(int index, int lcid, out IntPtr typeInfo);
    [PreserveSig] int _GetIDsOfNames(ref Guid riid, IntPtr names, int count, int lcid, IntPtr dispIds);
    [PreserveSig] int _Invoke(int dispId, ref Guid riid, int lcid, int flags, IntPtr dispParams, IntPtr result, IntPtr excepInfo, IntPtr argErr);

    [PreserveSig] int put_Caption([In, MarshalAs(UnmanagedType.BStr)] string caption);
    [PreserveSig] int get_Caption([Out, MarshalAs(UnmanagedType.BStr)] out string caption);
    [PreserveSig] int put_WindowStyle([In] int windowStyle);
    [PreserveSig] int get_WindowStyle(out int windowStyle);
    [PreserveSig] int put_WindowStyleEx([In] int windowStyleEx);
    [PreserveSig] int get_WindowStyleEx(out int windowStyleEx);
    [PreserveSig] int put_AutoShow([In, MarshalAs(UnmanagedType.Bool)] bool autoShow);
    [PreserveSig] int get_AutoShow([Out, MarshalAs(UnmanagedType.Bool)] out bool autoShow);
    [PreserveSig] int put_WindowState([In] int windowState);
    [PreserveSig] int get_WindowState(out int windowState);
    [PreserveSig] int put_BackgroundPalette([In, MarshalAs(UnmanagedType.Bool)] bool backgroundPalette);
    [PreserveSig] int get_BackgroundPalette([Out, MarshalAs(UnmanagedType.Bool)] out bool backgroundPalette);
    [PreserveSig] int put_Visible([In, MarshalAs(UnmanagedType.Bool)] bool visible);
    [PreserveSig] int get_Visible([Out, MarshalAs(UnmanagedType.Bool)] out bool visible);
    [PreserveSig] int put_Left([In] int left);
    [PreserveSig] int get_Left(out int left);
    [PreserveSig] int put_Width([In] int width);
    [PreserveSig] int get_Width(out int width);
    [PreserveSig] int put_Top([In] int top);
    [PreserveSig] int get_Top(out int top);
    [PreserveSig] int put_Height([In] int height);
    [PreserveSig] int get_Height(out int height);
    [PreserveSig] int put_Owner([In] IntPtr owner);
    [PreserveSig] int get_Owner(out IntPtr owner);
    [PreserveSig] int put_MessageDrain([In] IntPtr drain);
    [PreserveSig] int get_MessageDrain(out IntPtr drain);
    [PreserveSig] int get_BorderColor(out int color);
    [PreserveSig] int put_BorderColor([In] int color);
    [PreserveSig] int get_FullScreenMode([Out, MarshalAs(UnmanagedType.Bool)] out bool fullScreenMode);
    [PreserveSig] int put_FullScreenMode([In, MarshalAs(UnmanagedType.Bool)] bool fullScreenMode);
    [PreserveSig] int SetWindowForeground([In] int focus);
    [PreserveSig] int NotifyOwnerMessage([In] IntPtr hwnd, [In] int uMsg,
        [In] IntPtr wParam, [In] IntPtr lParam);
    [PreserveSig] int SetWindowPosition([In] int left, [In] int top,
        [In] int width, [In] int height);
    [PreserveSig] int GetWindowPosition(out int left, out int top,
        out int width, out int height);
    [PreserveSig] int GetMinIdealImageSize(out int width, out int height);
    [PreserveSig] int GetMaxIdealImageSize(out int width, out int height);
    [PreserveSig] int GetRestorePosition(out int left, out int top,
        out int width, out int height);
    [PreserveSig] int HideCursor([In, MarshalAs(UnmanagedType.Bool)] bool hideCursor);
    [PreserveSig] int IsCursorHidden([Out, MarshalAs(UnmanagedType.Bool)] out bool hideCursor);
}
