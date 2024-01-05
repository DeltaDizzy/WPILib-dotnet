﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WPIUtil.Natives;

public static partial class TimestampNative
{
    [LibraryImport("wpiutil", EntryPoint = "WPI_Impl_ShutdownNowRio")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial void ShutdownNowRio();

    [LibraryImport("wpiutil", EntryPoint = "WPI_NowDefault")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ulong NowDefault();

    [LibraryImport("wpiutil", EntryPoint = "WPI_SetNowImpl")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial void SetNowImpl(delegate* unmanaged[Cdecl]<ulong> callback);

    [LibraryImport("wpiutil", EntryPoint = "WPI_Now")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ulong Now();

    [LibraryImport("wpiutil", EntryPoint = "WPI_GetSystemTime")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static partial ulong GetSystemTime();
}
