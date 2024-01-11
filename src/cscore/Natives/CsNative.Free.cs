﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CsCore.Natives;

public static unsafe partial class CsNatives
{
    [LibraryImport("cscore", EntryPoint = "CS_FreeEnumeratedUsbCameras")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeEnumeratedUsbCameras(UsbCameraInfoMarshaller.NativeUsbCameraInfo* cameras, int count);

    [LibraryImport("cscore", EntryPoint = "CS_ReleaseEnumeratedSources")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void ReleaseEnumeratedSources(int* sources, int count);

    [LibraryImport("cscore", EntryPoint = "CS_ReleaseEnumeratedSinks")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void ReleaseEnumeratedSinks(int* sinks, int count);

    [LibraryImport("cscore", EntryPoint = "CS_FreeString")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeString(byte* str);

    [LibraryImport("cscore", EntryPoint = "CS_FreeEnumPropertyChoices")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeEnumPropertyChoices(byte** choices, int count);

    [LibraryImport("cscore", EntryPoint = "CS_FreeUsbCameraInfo")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeUsbCameraInfo(UsbCameraInfoMarshaller.NativeUsbCameraInfo* info);

    [LibraryImport("cscore", EntryPoint = "CS_FreeHttpCameraUrls")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeHttpCameraUrls(byte** choices, int count);

    [LibraryImport("cscore", EntryPoint = "CS_FreeEnumeratedProperties")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeEnumeratedProperties(int* properties, int count);

    [LibraryImport("cscore", EntryPoint = "CS_FreeEnumeratedVideoModes")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeEnumeratedVideoModes(VideoModeMarshaller.NativeVideoMode* modes, int count);

    [LibraryImport("cscore", EntryPoint = "CS_FreeNetworkInterfaces")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void FreeNetworkInterfaces(byte** interfaces, int count);
}
