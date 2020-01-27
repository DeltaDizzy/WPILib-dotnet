
using Hal.Natives;
using System;
using WPIUtil.NativeUtilities;

namespace Hal
{
    [NativeInterface(typeof(IDutyCycle))]
    public unsafe static class DutyCycle
    {
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS0649 // Field is never assigned to
#pragma warning disable IDE0044 // Add readonly modifier
        private static IDutyCycle lowLevel;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore CS0649 // Field is never assigned to
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

public static void FreeDutyCycle(int dutyCycleHandle)
{
lowLevel.HAL_FreeDutyCycle(dutyCycleHandle);
}

public static int GetDutyCycleFPGAIndex(int dutyCycleHandle)
{
return lowLevel.HAL_GetDutyCycleFPGAIndex(dutyCycleHandle);
}

public static int GetDutyCycleFrequency(int dutyCycleHandle)
{
return lowLevel.HAL_GetDutyCycleFrequency(dutyCycleHandle);
}

public static double GetDutyCycleOutput(int dutyCycleHandle)
{
return lowLevel.HAL_GetDutyCycleOutput(dutyCycleHandle);
}

public static int GetDutyCycleOutputRaw(int dutyCycleHandle)
{
return lowLevel.HAL_GetDutyCycleOutputRaw(dutyCycleHandle);
}

public static int GetDutyCycleOutputScaleFactor(int dutyCycleHandle)
{
return lowLevel.HAL_GetDutyCycleOutputScaleFactor(dutyCycleHandle);
}

public static int InitializeDutyCycle(int digitalSourceHandle, AnalogTriggerType triggerType)
{
return lowLevel.HAL_InitializeDutyCycle(digitalSourceHandle, triggerType);
}

public static void SetDutyCycleSimDevice(int handle, int device)
{
lowLevel.HAL_SetDutyCycleSimDevice(handle, device);
}

}
}
