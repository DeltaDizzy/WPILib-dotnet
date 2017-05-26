﻿using System;
using HAL.Base;
using NetworkTables.Tables;
using WPILib.Interfaces;
using WPILib.LiveWindow;

namespace WPILib
{
    /// <summary>
    /// Handle operation of an analog accelerometer
    /// </summary>
    /// <remarks>The accelerometer reads acceleration directly through the sensor. Many sensors have
    /// <para/>multiple axis and can be treated as multiple devices.Each is calibrated by finding
    /// <para/>the center value over a period of time.</remarks>
    public class AnalogAccelerometer : SensorBase, IPIDSource, ILiveWindowSendable
    {
        private AnalogInput m_analogChannel;
        private readonly bool m_allocatedChannel;


        ///<inheritdoc/>
        public PIDSourceType PIDSourceType { get; set; } = PIDSourceType.Displacement;

        private void InitAccelerometer()
        {
            HAL.Base.HAL.Report(ResourceType.kResourceType_Accelerometer, (byte)m_analogChannel.Channel);
            LiveWindow.LiveWindow.AddSensor("Accelerometer", m_analogChannel.Channel, this);
        }

        /// <summary>
        /// Create a new instance of an accelerometer, declaring a new analog channel.
        /// </summary>
        /// <param name="channel">The channel the accelerometer is connected to. [0..3] on RIO, [4..7] on MXP</param>
        public AnalogAccelerometer(int channel)
        {
            m_allocatedChannel = true;
            m_analogChannel = new AnalogInput(channel);
            InitAccelerometer();
        }

        /// <summary>
        /// Creates a new instance of the Accelerometer from an existing <see cref="AnalogInput"/>
        /// </summary>
        /// <param name="channel">The existing <see cref="AnalogInput"/> the accelerometer is connected to.</param>
        public AnalogAccelerometer(AnalogInput channel)
        {
            m_allocatedChannel = false;
            if (channel == null)
                throw new ArgumentNullException(nameof(channel), "Analog Channel given was null");
            m_analogChannel = channel;
            InitAccelerometer();
        }

        /// <summary>
        /// Delete the analog components used for the accelerometer.
        /// </summary>
        public override void Dispose()
        {
            if (m_analogChannel != null && m_allocatedChannel)
            {
                m_analogChannel.Dispose();
            }
            m_analogChannel = null;
        }

        /// <summary>
        /// Returns the acceleration in Gs.
        /// </summary>
        /// <returns>The acceleration in Gs.</returns>
        public virtual double GetAcceleration() => (m_analogChannel.GetAverageVoltage() - Zero)/Sensitivity;

        /// <summary>
        /// Sets the accelerometer sensitivity.
        /// </summary>
        public double Sensitivity { get; set; } = 1.0;

        /// <summary>
        /// Sets the voltage that corresponds to 0G.
        /// </summary>
        public double Zero { get; set; } = 2.5;

        /// <summary>
        /// Get the result to use in PIDController
        /// </summary>
        /// <returns>The result to use in PIDController</returns>
        public virtual double PidGet() => GetAcceleration();

        /// <summary>
        /// Initialize a table for this sendable object.
        /// </summary>
        /// <param name="subtable">The table to put the values in.</param>
        public void InitTable(ITable subtable)
        {
            Table = subtable;
            UpdateTable();
        }

        /// <summary>
        /// Returns the table that is currently associated with the sendable
        /// </summary>
        public ITable Table { get; private set; }

        /// <summary>
        /// Returns the string representation of the named data type that will be used by the smart dashboard for this sendable
        /// </summary>
        public string SmartDashboardType => "Accelerometer";

        /// <summary>
        /// Update the table for this sendable object with the latest
        /// values.
        /// </summary>
        public void UpdateTable()
        {
            Table?.PutNumber("Value", GetAcceleration());
        }

        /// <summary>
        /// Start having this sendable object automatically respond to
        /// value changes reflect the value on the table.
        /// </summary>
        public void StartLiveWindowMode() { }

        /// <summary>
        /// Stop having this sendable object automatically respond to value changes.
        /// </summary>
        public void StopLiveWindowMode() { }
    }
}
