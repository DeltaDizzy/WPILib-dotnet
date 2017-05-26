﻿using System;
using System.Threading;
using HAL.Simulator;
using HAL = HAL.Base.HAL;

namespace WPILib.Tests
{
    public abstract class TestBase
    {
        private static bool initialized = false;

        static TestBase()
        {
            if (!initialized)
            {
                    global::HAL.Base.HAL.Initialize();
                    global::HAL.Base.HALDriverStation.HAL_ObserveUserProgramStarting();

                ;
                LiveWindow.LiveWindow.SetEnabled(false);

                var instance = DriverStation.Instance;

                //DriverStationHelper.StartDSLoop();

                //DriverStationHelper.SetRobotMode(DriverStationHelper.RobotMode.Teleop);
                //DriverStationHelper.SetEnabledState(DriverStationHelper.EnabledState.Enabled);
                
                Thread.Sleep(500);
            }

        }

        public const int SystemClockTicksPerMicrosecond = 40;

        public const int DigitalChannels = 26;

        public const int AnalogInputChannels = 8;

        public const int AnalogOutputChannels = 2;

        public const int SolenoidChannels = 8;

        public const int SolenoidModules = 63;

        public const int PwmChannels = 20;

        public const int RelayChannels = 4;

        public const int PDPChannels = 16;

        public const int PDPModules = 63;

        public const int NumInterrupts = 8;

        public const int NumCounters = 8;

        public const int NumEncoders = 8;
    }
}
