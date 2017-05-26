﻿using System;
using static HAL.Base.HALDIO;
using static WPILib.Utility;
using static HAL.Base.HAL;
using HAL.Base;

namespace WPILib
{
    /// <summary>
    /// Class to enable glitch filtering on a set of digital inputs.
    /// </summary>
    public class DigitalGlitchFilter : SensorBase
    {
        private int m_channelIndex = -1;

        private static readonly Resource s_allocated = new Resource(3);

        /// <summary>
        /// Creates a new <see cref="DigitalGlitchFilter"/>
        /// </summary>
        public DigitalGlitchFilter()
        {
            m_channelIndex = s_allocated.Allocate("Could not allocate a new DigitalGlitchFilter");
            Report(ResourceType.kResourceType_DigitalFilter, (byte)m_channelIndex, 0);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (m_channelIndex != -1)
            {
                s_allocated.Deallocate(m_channelIndex);
                m_channelIndex = -1;
            }
            base.Dispose();
        }

        private static void SetFilter(DigitalSource input, int channelIndex)
        {
            if (input != null)
            {
                int status = 0;
                HAL_SetFilterSelect(input.PortHandleForRouting, channelIndex, ref status);
                CheckStatus(status);
                int selected = HAL_GetFilterSelect(input.PortHandleForRouting, ref status);
                CheckStatus(status);
                if (selected != channelIndex)
                    throw new InvalidOperationException($"SetFilterSelect for {channelIndex} failed -> {selected}");
            }
        }

        /// <summary>
        /// Assigns the <see cref="DigitalSource"/> to this glitch filter
        /// </summary>
        /// <param name="input">The <see cref="DigitalSource"/> to add.</param>
        public void Add(DigitalSource input)
        {
            SetFilter(input, m_channelIndex + 1);
        }

        /// <summary>
        /// Assigns the <see cref="Encoder"/> to this glitch filter
        /// </summary>
        /// <param name="encoder">The <see cref="Encoder"/> to add.</param>
        public void Add(Encoder encoder)
        {
            Add(encoder.ASource);
            Add(encoder.BSource);
        }


        /// <summary>
        /// Assigns the <see cref="Counter"/> to this glitch filter
        /// </summary>
        /// <param name="counter">The <see cref="Counter"/> to add.</param>
        public void Add(Counter counter)
        {
            Add(counter.m_upSource);
            Add(counter.m_downSource);
        }

        /// <summary>
        /// Removes this filter from the given digital input.
        /// </summary>
        /// <param name="input">The <see cref="DigitalSource"/> to stop filtering.</param>
        public void Remove(DigitalSource input)
        {
            SetFilter(input, 0);
        }

        /// <summary>
        /// Removes the <see cref="Encoder"/> from this glitch filter
        /// </summary>
        /// <param name="encoder">The <see cref="Encoder"/> to remove.</param>
        public void Remove(Encoder encoder)
        {
            Remove(encoder.ASource);
            Remove(encoder.BSource);
        }

        /// <summary>
        /// Removes the <see cref="Counter"/> from this glitch filter
        /// </summary>
        /// <param name="counter">The <see cref="Counter"/> to remove.</param>
        public void Remove(Counter counter)
        {
            Remove(counter.m_upSource);
            Remove(counter.m_downSource);
        }

        /// <summary>
        /// Sets the number of FPGA cycles that the input must hold steady to pass
        /// through this glitch filter
        /// </summary>
        /// <param name="fpgaCycles">The number of FPGA cycles.</param>
        public void SetPeriodCycles(uint fpgaCycles)
        {
            int status = 0;
            HAL_SetFilterPeriod(m_channelIndex, fpgaCycles, ref status);
            CheckStatus(status);
        }

        /// <summary>
        /// Sets the number of nanoseconds that the input must hold steady to pass
        /// through this glitch filter.
        /// </summary>
        /// <param name="nanoSeconds"></param>
        public void SetPeriodNanoSeconds(ulong nanoSeconds)
        {
            uint fpgaCycles = (uint) (nanoSeconds * (ulong)SystemClockTicksPerMicrosecond / 4 / 1000);
            SetPeriodCycles(fpgaCycles);
        }

        /// <summary>
        /// Gets the number of FPGA cycles that the input must hold stead to pass
        /// through this glitch filter.
        /// </summary>
        /// <returns>The number of FPGA cycles.</returns>
        public long GetPeriodCycles()
        {
            int status = 0;
            long retVal = HAL_GetFilterPeriod(m_channelIndex, ref status);
            CheckStatus(status);
            return retVal;
        }

        /// <summary>
        /// Gets the number of nanoseconds that the input must hold steady to pass
        /// through this glitch filter
        /// </summary>
        /// <returns>The number of nanoseconds.</returns>
        public long GetPeriodNanoSeconds()
        {
            long fpgaCycles = GetPeriodCycles();

            return fpgaCycles * 1000L / (SystemClockTicksPerMicrosecond /4);
        }
    }
}
