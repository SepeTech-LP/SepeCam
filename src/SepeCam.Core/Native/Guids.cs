using System;

namespace SepeCam.Core.Native;

internal static class Guids
{
    public static readonly Guid CLSID_SystemDeviceEnum = new("62BE5D10-60EB-11D0-BD3B-00A0C911CE86");
    public static readonly Guid CLSID_VideoInputDeviceCategory = new("860BB310-5D01-11D0-BD3B-00A0C911CE86");
    public static readonly Guid CLSID_FilterGraph = new("E436EBB3-524F-11CE-9F53-0020AF0BA770");
    public static readonly Guid CLSID_CaptureGraphBuilder2 = new("BF87B6E1-8C27-11D0-B3F0-00AA003761C5");

    public static readonly Guid IID_IBaseFilter = new("56A86895-0AD4-11CE-B03A-0020AF0BA770");
    public static readonly Guid IID_IPropertyBag = new("55272A00-42CB-11CE-8135-00AA004BB851");

    public static readonly Guid PIN_CATEGORY_PREVIEW = new("FB6C4282-0353-11D1-905F-0000C0CC16BA");
    public static readonly Guid PIN_CATEGORY_CAPTURE = new("FB6C4281-0353-11D1-905F-0000C0CC16BA");
    public static readonly Guid MEDIATYPE_Video = new("73646976-0000-0010-8000-00AA00389B71");
}
