﻿using System;
using System.Linq;
using HAL.Base;
using NetworkTables.Tables;
using WPILib.Exceptions;
using WPILib.Interfaces;
using WPILib.LiveWindow;
using static HAL.Base.HAL;
using static HAL.Base.HALAnalogInput;
using static HAL.Base.HALPorts;
using static WPILib.Utility;
using static HAL.Base.HALAnalogAccumulator;
using HALAnalogAccumulator = HAL.Base.HALAnalogAccumulator;

namespace WPILib
{
    /// <summary>
    /// Analog Channel class. Each channel is read from hardware as a 12-bit number representing 0v to 5v.
    /// </summary>
    /// <remarks>Connected to each analog channel is an averaging and oversampling engine.
    /// <para/> This engine accumulates the specified(by setAverageBits() and
    /// <para/> setOversampleBits() ) number of samples before returning a new value.This is
    /// <para/> not a sliding window average.The only difference between the oversampled
    /// <para/> samples and the averaged samples is that the oversampled samples are simply
    /// <para/> accumulated effectively increasing the resolution, while the averaged samples
    /// <para/> are divided by the number of samples to retain the resolution, but get more
    /// <para/> stable values.</remarks>
    public class AnalogInput : SensorBase, IPIDSource, ILiveWindowSendable
    {
        internal int m_halHandle;
        private static readonly int[] s_accumulatorChannels = { 0, 1 };
        private long m_accumulatorOffset;


        ///<inheritdoc/>
        public PIDSourceType PIDSourceType { get; set; } = PIDSourceType.Displacement;

        /// <summary>
        /// Construct an analog channel
        /// </summary>
        /// <param name="channel">The channel number to represent. 0-3 are on-board 4-7 are on the MXP port.</param>
        public AnalogInput(int channel)
        {
            Channel = channel;

            CheckAnalogInputChannel(channel);
            int status = 0;
            m_halHandle = HAL_InitializeAnalogInputPort(HAL_GetPort(channel), ref status);
            if (status != 0)
            {
                CheckStatusRange(status, 0, HAL_GetNumAnalogInputs(), channel);
                Channel = Int32.MaxValue;
                m_halHandle = HALInvalidHandle;
                return;
            }

            LiveWindow.LiveWindow.AddSensor("AnalogInput", channel, this);
            Report(ResourceType.kResourceType_AnalogChannel, (byte)channel);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            HAL_FreeAnalogInputPort(m_halHandle);
            m_halHandle = HALInvalidHandle;
            Channel = 0;
            m_accumulatorOffset = 0;
        }

        /// <summary>
        /// Get a sample straight from this channel. 
        /// </summary>
        /// <remarks>The sample is a 12-bit value
        /// <para/> representing the 0V to 5V range of the A/D converter.The units are in
        /// <para/> A/D converter codes.Use GetVoltage() to get the analog value in
        /// <para/> calibrated units.</remarks>
        /// <returns>A straight sample from this channel in 12 bit form.</returns>
        public virtual int GetValue()
        {
            int status = 0;
            int value = HAL_GetAnalogValue(m_halHandle, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Get a sample from the output of the oversample and average engine for this channel.
        /// </summary>
        /// <remarks>The sample is 12-bit + the bits configured in
        /// <para/> SetOversampleBits(). The value configured in setAverageBits() will cause
        /// <para/> this value to be averaged 2^bits number of samples.This is not a
        /// <para/> sliding window. The sample will not change until 2^(OversampleBits +
        /// <para/> AverageBits) samples have been acquired from this channel.Use
        /// <para/> getAverageVoltage() to get the analog value in calibrated units.</remarks>
        /// <returns> A sample from the oversample and average engine for this channel.</returns>
        public virtual int GetAverageValue()
        {
            int status = 0;
            int value = HAL_GetAnalogAverageValue(m_halHandle, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Get a scaled sample straight from this channel.
        /// </summary>
        /// <returns>The voltage on the Analog Input</returns>
        public virtual double GetVoltage()
        {
            int status = 0;
            double value = HAL_GetAnalogVoltage(m_halHandle, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Get a scaled sample from the output of the oversample and average engine
        /// for this channel.
        /// </summary>
        /// <returns>A scaled sample from the output of the oversample and average engine for this channel.</returns>
        public virtual double GetAverageVoltage()
        {
            int status = 0;
            double value = HAL_GetAnalogAverageVoltage(m_halHandle, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Gets the factory scaling least significant bit weight constant that was calibrated
        /// at manufacturing.
        /// </summary>
        /// <remarks>
        /// Volts = ((<see cref="LSBWeight"/> * 1e-9) * raw) - (<see cref="Offset"/> * 1e-9)
        /// </remarks>
        public int LSBWeight
        {
            get
            {
                int status = 0;
                int value = HAL_GetAnalogLSBWeight(m_halHandle, ref status);
                CheckStatus(status);
                return value;
            }
        }

        /// <summary>
        /// Gets the factory scaling offset constant that was calibrated at manufacturing.
        /// </summary>
        /// <remarks>
        /// Volts = ((<see cref="LSBWeight"/> * 1e-9) * raw) - (<see cref="Offset"/> * 1e-9)
        /// </remarks>
        public int Offset
        {
            get
            {
                int status = 0;
                int value = HAL_GetAnalogOffset(m_halHandle, ref status);
                CheckStatus(status);
                return value;
            }
        }

        /// <summary>
        /// Gets the channel of this Analog Input.
        /// </summary>
        public int Channel { get; private set; }

        /// <summary>
        /// Gets or sets the averaging bits of this analog input.
        /// </summary>
        /// <remarks>
        /// The actual number of averaged samples is 2^bits. The averaging is done
        /// automatically in the FPGA.
        /// </remarks>
        public int AverageBits
        {
            set
            {
                int status = 0;
                HAL_SetAnalogAverageBits(m_halHandle, value, ref status);
                CheckStatus(status);
            }
            get
            {
                int status = 0;
                int value = HAL_GetAnalogAverageBits(m_halHandle, ref status);
                CheckStatus(status);
                return value;
            }
        }

        /// <summary>
        /// Gets or Sets the number of oversample bits
        /// </summary>
        /// <remarks>
        /// The actual number of oversampled values is 2^bits. The oversampling is done
        /// automatically in the FPGA.
        /// </remarks>
        public int OversampleBits
        {
            set
            {
                int status = 0;
                HAL_SetAnalogOversampleBits(m_halHandle, value, ref status);
                CheckStatus(status);
            }
            get
            {
                int status = 0;
                int value = HAL_GetAnalogOversampleBits(m_halHandle, ref status);
                CheckStatus(status);
                return value;
            }
        }

        /// <summary>
        /// Initialize the accumulator.
        /// </summary>
        public void InitAccumulator()
        {
            if (!IsAccumulatorChannel)
            {
                throw new AllocationException("This is not an accumulator");
            }
            m_accumulatorOffset = 0;
            int status = 0;
            HAL_InitAccumulator(m_halHandle, ref status);
            CheckStatus(status);
        }

        /// <summary>
        /// Set an initial value for the accumulator. This will be added to all values returned to the user.
        /// </summary>
        public long AccumulatorInitialValue
        {
            set { m_accumulatorOffset = value; }
        }

        /// <summary>
        /// Reset the accumulator to its initial value.
        /// </summary>
        public void ResetAccumulator()
        {
            int status = 0;
            HAL_ResetAccumulator(m_halHandle, ref status);
            CheckStatus(status);
            double sampleTime = 1.0 / GlobalSampleRate;
            double overSamples = 1 << OversampleBits;
            double averageSamples = 1 << AverageBits;
            Timer.Delay(sampleTime * overSamples * averageSamples);
        }

        /// <summary>
        /// Set the center value of the accumulator.
        /// </summary>
        public int AccumulatorCenter
        {
            set
            {
                int status = 0;
                HAL_SetAccumulatorCenter(m_halHandle, value, ref status);
                CheckStatus(status);
            }
        }

        /// <summary>
        /// Set the accumulators deadband in 12 bit format.
        /// </summary>
        public int AccumulatorDeadband
        {
            set
            {
                int status = 0;
                HAL_SetAccumulatorDeadband(m_halHandle, value, ref status);
                CheckStatus(status);
            }
        }

        /// <summary>
        /// Read the accumulated value
        /// </summary>
        public long GetAccumulatorValue()
        {
            int status = 0;
            long value = HAL_GetAccumulatorValue(m_halHandle, ref status);
            CheckStatus(status);
            return value + m_accumulatorOffset;
        }

        /// <summary>
        /// Read the number of accumulated values
        /// </summary>
        public long GetAccumulatorCount()
        {
            int status = 0;
            long value = HAL_GetAccumulatorCount(m_halHandle, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Read the accumulated value and the number of accumulated values atomically
        /// </summary>
        /// <param name="value">The 64 bit accumulated output</param>
        /// <param name="count">The number of accumulation cycles</param>
        public void GetAccumulatorOutput(ref long value, ref long count)
        {
            if (!IsAccumulatorChannel)
                throw new ArgumentException($"Channel {Channel} is not an accumulator channel.");
            int status = 0;
            HAL_GetAccumulatorOutput(m_halHandle, ref value, ref count, ref status);
            CheckStatus(status);
            value += m_accumulatorOffset;
        }

        /// <summary>
        /// Is the channel attached to an accumulator.
        /// </summary>
        public bool IsAccumulatorChannel
        {
            get { return s_accumulatorChannels.Any(t => Channel == t); }
        }

        /// <summary>
        /// Gets or Sets the current global sample rage.
        /// </summary>
        public static double GlobalSampleRate
        {
            set
            {
                int status = 0;
                HAL_SetAnalogSampleRate(value, ref status);
            }
            get
            {
                int status = 0;
                double value = HAL_GetAnalogSampleRate(ref status);
                CheckStatus(status);
                return value;
            }
        }

        /// <summary>
        /// Get the result to use in PIDController
        /// </summary>
        /// <returns>The result to use in PIDController</returns>
        public virtual double PidGet() => GetAverageVoltage();

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
        public string SmartDashboardType => "Analog Input";

        /// <summary>
        /// Update the table for this sendable object with the latest
        /// values.
        /// </summary>
        public void UpdateTable()
        {
            Table?.PutNumber("Value", GetAverageVoltage());
        }

        /// <summary>
        /// Start having this sendable object automatically respond to
        /// value changes reflect the value on the table.
        /// </summary>
        public void StartLiveWindowMode()
        {
        }

        /// <summary>
        /// Stop having this sendable object automatically respond to value changes.
        /// </summary>
        public void StopLiveWindowMode()
        {
        }
    }
}
