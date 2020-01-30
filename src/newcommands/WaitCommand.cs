﻿using System;
using System.Collections.Generic;
using System.Text;
using WPILib;
using WPILib.SmartDashboard;

namespace WPILib2.Commands
{
    public class WaitCommand : CommandBase
    {
        protected Timer m_timer = new Timer();
        private readonly TimeSpan m_duration;

        public WaitCommand(TimeSpan time)
        {
            m_duration = time;
            SendableRegistry.Instance.SetName(this, $"{Name}: {time.TotalSeconds} seconds");
        }

        public void Initialize()
        {
            m_timer.Reset();
            m_timer.Start();
        }

        public bool IsFinished => m_timer.HasPeriodPassed(m_duration);

        public bool RunsWhenDisabled => true;
    }
}
