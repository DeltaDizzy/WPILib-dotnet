
using Hal.Natives;
using System;
using WPIUtil.NativeUtilities;

namespace Hal
{
    [NativeInterface(typeof(IMain))]
    public unsafe static class Main
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to
#pragma warning disable IDE0044 // Add readonly modifier
        private static IMain lowLevel;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Field is never assigned to
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

public static void Set(void* param, IntPtr mainFunc, IntPtr exitFunc)
{
lowLevel.HAL_SetMain(param, mainFunc, exitFunc);
}

public static bool HasMain()
{
return lowLevel.HAL_HasMain() != 0;
}

public static void RunMain()
{
lowLevel.HAL_RunMain();
}

public static void ExitMain()
{
lowLevel.HAL_ExitMain();
}

}
}
