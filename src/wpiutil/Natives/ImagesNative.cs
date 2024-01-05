﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace WPIUtil.Natives;

public static partial class ImagesNative
{
    [LibraryImport("wpiutil", EntryPoint = "WPI_GetResource_wpilib_128_png")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    public static unsafe partial byte* GetResourceWpilib128Png(out nuint len);
}
