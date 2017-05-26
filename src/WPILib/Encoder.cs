﻿using System;
using HAL.Base;
using NetworkTables.Tables;
using WPILib.Exceptions;
using WPILib.Interfaces;
using WPILib.LiveWindow;
using static HAL.Base.HAL;
using static HAL.Base.HALEncoder;
using static WPILib.Utility;

namespace WPILib
{
    /// <summary>
    /// Class to read quadrature encoders.
    /// </summary>
    /// <remarks>
    /// Quadrature encoders are devices that count shaft rotation and can sense direction. 
    /// The output of the QuadEncoder class is an integer that can count either up or down, 
    /// and can go negative for reverse direction counting. When creating Quad Encoders, a direction 
    /// is supplied that changes the sense of the output to make code more readable if the encoder 
    /// is mounter such that forwrd movement generates negative values. Quadrature encoders have 
    /// two digital outputs, a A channel and a B channel that are out of phase with each other to 
    /// allow the FPGA to do direction sensing.
    /// <para/>All encoders will immediately start counting - <see cref="Reset()"/> them if you need 
    /// them to be zeroed before use.
    /// </remarks>
    public class Encoder : SensorBase, ICounterBase, IPIDSource, ILiveWindowSendable
    {
        /// <summary>
        /// Encoder Indexing Type Enum
        /// </summary>
        public enum IndexingType
        {
            /// <summary>
            /// Reset indexing while the index pin is High
            /// </summary>
            ResetWhileHigh,
            /// <summary>
            /// Reset indexing while the index pin is low.
            /// </summary>
            ResetWhileLow,
            /// <summary>
            /// Reset indexing on the falling edge of the index pin.
            /// </summary>
            ResetOnFallingEdge,
            /// <summary>
            /// Reset indexing on the rising edge of the index pin.
            /// </summary>
            ResetOnRisingEdge,
        }

        /// <summary>
        /// The A Source
        /// </summary>
        protected internal DigitalSource ASource;
        /// <summary>
        /// The B Source
        /// </summary>
        protected internal DigitalSource BSource;
        /// <summary>
        /// The Index Source
        /// </summary>
        protected DigitalSource IndexSource = null;

        private int m_encoder;

        private bool m_allocatedA;
        private bool m_allocatedB;
        private bool m_allocatedI;
        private PIDSourceType m_pidSource;

        private void InitEncoder(bool reverseDirection, EncodingType encodingType)
        {
            int status = 0;
            m_encoder = HAL_InitializeEncoder(ASource.PortHandleForRouting,
                (HALAnalogTriggerType)ASource.AnalogTriggerTypeForRouting, BSource.PortHandleForRouting,
                (HALAnalogTriggerType)BSource.AnalogTriggerTypeForRouting, reverseDirection,
                (HALEncoderEncodingType)encodingType, ref status);
            CheckStatusForceThrow(status);

            m_pidSource = PIDSourceType.Displacement;

            LiveWindow.LiveWindow.AddSensor("Encoder", FPGAIndex, this);
            Report(ResourceType.kResourceType_Encoder, FPGAIndex, (byte)encodingType);
        }

        /// <summary>
        /// Construct an Encoder given A and B Channels.
        /// </summary>
        /// <remarks>The encoder will start counting immediately.</remarks>
        /// <param name="aChannel">The A channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="bChannel">The B channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="reverseDirection">True if to reverse the output, otherwise false</param>
        public Encoder(int aChannel, int bChannel, bool reverseDirection = false)
        {
            m_allocatedA = true;
            m_allocatedB = true;
            m_allocatedI = false;
            ASource = new DigitalInput(aChannel);
            BSource = new DigitalInput(bChannel);
            InitEncoder(reverseDirection, EncodingType.K4X);
        }

        /// <summary>
        /// Construct an Encoder given A and B Channels.
        /// </summary>
        /// <remarks>The encoder will start counting immediately.
        /// <para/>
        /// For encoding type, if 4X is selected, then an encoder FPGA object is used and the returned counts
        /// will be 4X the encoder spec'd value since all rising and falling edges are counted. If 1X or 2X
        /// are slected then a counter object will be used and the returned value will either exactly match the
        /// spec'd count or be double (2x) the spec'd count.
        /// </remarks>
        /// <param name="aChannel">The A channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="bChannel">The B channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="reverseDirection">True if to reverse the output, otherwise false</param>
        /// <param name="encodingType">Either 1X, 2X or 4X to indicate decoding scale.</param>
        public Encoder(int aChannel, int bChannel, bool reverseDirection, EncodingType encodingType)
        {
            m_allocatedA = true;
            m_allocatedB = true;
            m_allocatedI = false;
            ASource = new DigitalInput(aChannel);
            BSource = new DigitalInput(bChannel);
            InitEncoder(reverseDirection, encodingType);
        }

        /// <summary>
        /// Construct an Encoder given A and B Channels, and an Index pulse channel.
        /// </summary>
        /// <remarks>The encoder will start counting immediately.</remarks>
        /// <param name="aChannel">The A channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="bChannel">The B channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="indexChannel">The Index channel DIO channel. 0-9 are on-board, 10-25 are on the MXP port.</param>
        /// <param name="reverseDirection">True if to reverse the output, otherwise false</param>
        public Encoder(int aChannel, int bChannel,
            int indexChannel, bool reverseDirection = false)
        {
            m_allocatedA = true;
            m_allocatedB = true;
            m_allocatedI = true;
            ASource = new DigitalInput(aChannel);
            BSource = new DigitalInput(bChannel);
            IndexSource = new DigitalInput(indexChannel);
            InitEncoder(reverseDirection, EncodingType.K4X);
            SetIndexSource(indexChannel);
        }

        /// <summary>
        /// Construct an Encoder given precreated A and B Channels as <see cref="DigitalSource">DigitalSources</see>.
        /// </summary>
        /// <remarks>The encoder will start counting immediately.</remarks>
        /// <param name="aSource">The A channel <see cref="DigitalSource"/></param>
        /// <param name="bSource">The B channel <see cref="DigitalSource"/></param>
        /// <param name="reverseDirection">True if to reverse the output, otherwise false</param>
        public Encoder(DigitalSource aSource, DigitalSource bSource, bool reverseDirection = false)
        {
            m_allocatedA = false;
            m_allocatedB = false;
            m_allocatedI = false;
            if (aSource == null)
                throw new ArgumentNullException(nameof(aSource), "Digital Source A was null");
            ASource = aSource;
            if (bSource == null)
                throw new ArgumentNullException(nameof(bSource), "Digital Source B was null");
            BSource = bSource;
            InitEncoder(reverseDirection, EncodingType.K4X);
        }

        /// <summary>
        /// Construct an Encoder given precreated A and B Channels as <see cref="DigitalSource">DigitalSources</see>.
        /// </summary>
        /// <remarks>The encoder will start counting immediately.
        /// <para/>
        /// For encoding type, if 4X is selected, then an encoder FPGA object is used and the returned counts
        /// will be 4X the encoder spec'd value since all rising and falling edges are counted. If 1X or 2X
        /// are slected then a counter object will be used and the returned value will either exactly match the
        /// spec'd count or be double (2x) the spec'd count.
        /// </remarks>
        /// <param name="aSource">The A channel <see cref="DigitalSource"/></param>
        /// <param name="bSource">The B channel <see cref="DigitalSource"/></param>
        /// <param name="reverseDirection">True if to reverse the output, otherwise false</param>
        /// <param name="encodingType">Either 1X, 2X or 4X to indicate decoding scale.</param>
        public Encoder(DigitalSource aSource, DigitalSource bSource,
            bool reverseDirection, EncodingType encodingType)
        {
            m_allocatedA = false;
            m_allocatedB = false;
            m_allocatedI = false;
            if (aSource == null)
                throw new ArgumentNullException(nameof(aSource), "Digital Source A was null");
            ASource = aSource;
            if (bSource == null)
                throw new ArgumentNullException(nameof(bSource), "Digital Source B was null");
            ASource = aSource;
            BSource = bSource;
            InitEncoder(reverseDirection, encodingType);
        }

        /// <summary>
        /// Construct an Encoder given precreated A, B, and Index Channels as <see cref="DigitalSource">DigitalSources</see>.
        /// </summary>
        /// <remarks>The encoder will start counting immediately.</remarks>
        /// <param name="aSource">The A channel <see cref="DigitalSource"/></param>
        /// <param name="bSource">The B channel <see cref="DigitalSource"/></param>
        /// <param name="indexSource">The Index channel <see cref="DigitalSource"/></param>
        /// <param name="reverseDirection">True if to reverse the output, otherwise false</param>
        public Encoder(DigitalSource aSource, DigitalSource bSource,
            DigitalSource indexSource, bool reverseDirection = false)
        {
            m_allocatedA = false;
            m_allocatedB = false;
            m_allocatedI = false;
            if (aSource == null)
                throw new ArgumentNullException(nameof(aSource), "Digital Source A was null");
            ASource = aSource;
            if (bSource == null)
                throw new ArgumentNullException(nameof(bSource), "Digital Source B was null");
            ASource = aSource;
            BSource = bSource;
            IndexSource = indexSource;
            InitEncoder(reverseDirection, EncodingType.K4X);
            SetIndexSource(indexSource);
        }

        /// <summary>
        /// Gets the encoder's FPGA Index.
        /// </summary>
        public int FPGAIndex
        {
            get
            {
                int status = 0;
                int val = HAL_GetEncoderFPGAIndex(m_encoder, ref status);
                CheckStatus(status);
                return val;
            }
        }


        /// <summary>
        /// Gets the encoder's Encoding Scale, which is used to divide raw edge counts to spec'd counts.
        /// </summary>
        public int EncodingScale
        {
            get
            {
                int status = 0;
                int val = HAL_GetEncoderEncodingScale(m_encoder, ref status);
                CheckStatus(status);
                return val;
            }
        }


        /// <inheritdoc/>
        public override void Dispose()
        {
            if (ASource != null && m_allocatedA)
            {
                ASource.Dispose();
                m_allocatedA = false;
            }
            if (BSource != null && m_allocatedB)
            {
                BSource.Dispose();
                m_allocatedB = false;
            }
            if (IndexSource != null && m_allocatedI)
            {
                IndexSource.Dispose();
                m_allocatedI = false;
            }

            ASource = null;
            BSource = null;
            IndexSource = null;
            int status = 0;
            HAL_FreeEncoder(m_encoder, ref status);
            CheckStatus(status);
        }

        /// <summary>
        /// Gets the raw value from the encoder.
        /// </summary>
        /// <remarks>The value is the actual count, not scaled by the scale factor.</remarks>
        /// <returns>The raw count from the encoder</returns>
        public virtual int GetRaw()
        {
            int status = 0;
            int value = HAL_GetEncoderRaw(m_encoder, ref status);
            CheckStatus(status);
            return value;
        }

        /// <inheritdoc/>
        public virtual int Get()
        {
            int status = 0;
            int value = HAL_GetEncoder(m_encoder, ref status);
            CheckStatus(status);
            return value;
        }

        /// <inheritdoc/>
        public virtual void Reset()
        {
            int status = 0;
            HAL_ResetEncoder(m_encoder, ref status);
            CheckStatus(status);
        }

        /// <inheritdoc/>
        public virtual double GetPeriod()
        {
            int status = 0;
            double measuredPeriod = HAL_GetEncoderPeriod(m_encoder, ref status);
            CheckStatus(status);
            return measuredPeriod;
        }


        /// <inheritdoc/>
        public double MaxPeriod
        {
            set
            {
                int status = 0;
                HAL_SetEncoderMaxPeriod(m_encoder, value, ref status);
                CheckStatus(status);
            }
        }

        /// <inheritdoc/>
        public bool GetStopped()
        {
            int status = 0;
            bool value = HAL_GetEncoderStopped(m_encoder, ref status);
            CheckStatus(status);
            return value;
        }

        /// <inheritdoc/>
        public bool GetDirection()
        {
            int status = 0;
            bool value = HAL_GetEncoderDirection(m_encoder, ref status);
            CheckStatus(status);
            return value;
        }

        private double DecodingScaleFactor
        {
            get
            {
                int status = 0;
                double value = HAL_GetEncoderDecodingScaleFactor(m_encoder, ref status);
                CheckStatus(status);
                return value;
            }
        }

        /// <summary>
        /// Gets the distance the robot has driven since the last reset.
        /// </summary>
        /// <returns>Distance driven since the last reset scaled by the <see cref="DistancePerPulse"/></returns>
        public virtual double GetDistance()
        {
            int status = 0;
            double value = HAL_GetEncoderDistance(m_encoder, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Gets the current rate of the encoder in distance per second.
        /// </summary>
        /// <returns>The current rate of the encoder scaled by the <see cref="DistancePerPulse"/></returns>
        public virtual double GetRate()
        {
            int status = 0;
            double value = HAL_GetEncoderRate(m_encoder, ref status);
            CheckStatus(status);
            return value;
        }

        /// <summary>
        /// Sets the minimum rate of the device before the hardware reports it stopped.
        /// </summary>
        public double MinRate
        {
            set
            {
                int status = 0;
                HAL_SetEncoderMinRate(m_encoder, value, ref status);
                CheckStatus(status);
            }
        }

        /// <summary>
        /// Sets the distance per pulse for this encoder.
        /// </summary>
        /// <remarks>
        /// This sets the multiplier used to determine the distance driven based on the count value 
        /// from the encoder. Do not include the decoding type in the scale. The library arleady compensates 
        /// for the decoding type. Set this value based on the encoders rated Pulses Per Revolution and factor 
        /// in gearing reductions following the encoder shaft.
        /// </remarks>
        public double DistancePerPulse
        {
            set
            {
                int status = 0;
                HAL_SetEncoderDistancePerPulse(m_encoder, value, ref status);
                CheckStatus(status);
            }
        }

        /// <summary>
        /// Sets the direction sensing for this encoder.
        /// </summary>
        /// <param name="direction">True if direction should be reversed, otherwise false.</param>
        public void SetReverseDirection(bool direction)
        {
            int status = 0;
            HAL_SetEncoderReverseDirection(m_encoder, direction, ref status);
            CheckStatus(status);
        }

        /// <summary>
        /// Gets or Sets the number of samples to average when caluclating the period.
        /// </summary>
        public int SamplesToAverage
        {
            set
            {
                int status = 0;
                HAL_SetEncoderSamplesToAverage(m_encoder, value, ref status);
                CheckStatus(status);
            }
            get
            {
                int status = 0;
                int val = HAL_GetEncoderSamplesToAverage(m_encoder, ref status);
                CheckStatus(status);
                return val;
            }
        }

        /// <inheritdoc/>
        public PIDSourceType PIDSourceType
        {
            get
            {
                return m_pidSource;
            }
            set
            {
                BoundaryException.AssertWithinBounds((int)value, 0, 1);
                m_pidSource = value;
            }
        }

        /// <inheritdoc/>
        public virtual double PidGet()
        {
            switch (m_pidSource)
            {
                case PIDSourceType.Displacement:
                    return GetDistance();
                case PIDSourceType.Rate:
                    return GetRate();
                default:
                    return 0.0;
            }
        }

        /// <summary>
        /// Sets the index source for the encoder. Resets based on the <see cref="IndexingType"/> passed.
        /// </summary>
        /// <param name="channel">The DIO channel to set as the encoder index.</param>
        /// <param name="type">The state that will cause the encoder to reset.</param>
        public void SetIndexSource(int channel, IndexingType type = IndexingType.ResetOnRisingEdge)
        {
            if (m_allocatedI) 
                throw new AllocationException("Digital Input for Indexing already allocated");

            IndexSource = new DigitalInput(channel);
            m_allocatedI = true;
            SetIndexSource(IndexSource, type);
        }

        /// <summary>
        /// Sets the index source for the encoder. Resets based on the <see cref="IndexingType"/> passed.
        /// </summary>
        /// <param name="source">The <see cref="DigitalSource"/> to set as the encoder index.</param>
        /// <param name="type">The state that will cause the encoder to reset.</param>
        public void SetIndexSource(DigitalSource source, IndexingType type = IndexingType.ResetOnRisingEdge)
        {
            int status = 0;

            HAL_SetEncoderIndexSource(m_encoder, source.PortHandleForRouting,
                (HALAnalogTriggerType) source.AnalogTriggerTypeForRouting, (HALEncoderIndexingType) type, ref status);
            CheckStatus(status);
        }

        ///<inheritdoc />
        public void InitTable(ITable subtable)
        {
            Table = subtable;
            UpdateTable();
        }

        ///<inheritdoc />
        public ITable Table { get; private set; }

        ///<inheritdoc />
        public string SmartDashboardType => "Encoder";
        ///<inheritdoc />
        public void UpdateTable()
        {
            if (Table != null)
            {
                Table.PutNumber("Speed", GetRate());
                Table.PutNumber("Distance", GetDistance());
                int status = 0;
                Table.PutNumber("Distance per Tick", HAL_GetEncoderDistancePerPulse(m_encoder, ref status));
            }
        }

        ///<inheritdoc />
        public void StartLiveWindowMode()
        {
        }
        ///<inheritdoc />
        public void StopLiveWindowMode()
        {
        }
    }
}
