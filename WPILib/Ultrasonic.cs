﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HAL_Base;
using NetworkTables.Tables;
using WPILib.Interfaces;
using WPILib.LiveWindows;

namespace WPILib
{
    /// <summary>
    /// Ultrasonic rangefinder class, for digital ultransonics. Not for the MaxBotix Sensors
    /// </summary>
    /// <remarks>
    /// For Maxbotix sensors, if somebody wants to write one and add it to WPILib.Extra please do and sumbit a pull request.
    /// <para/> The Ultrasonic rangefinder measures absolute distance based on the round-trip time 
    /// of a ping generated by the controller. These sensors use two transducers, a speaker and a 
    /// microphone both tuned to the ultrasonic range. A common ultrasonic sensor, the Daventech 
    /// SRF04 requires a short pulse to be generated on a digital channel. This causes the chirp to 
    /// be emmitted. A second line becomes high as the ping is transmitted and goes low when the echo 
    /// is received. The time that the line is high determines the round trip distance (time of flight).
    /// </remarks>
    public class Ultrasonic : SensorBase, IPIDSource, ILiveWindowSendable
    {
        /// <summary>
        /// The unit for measurements to return.
        /// </summary>
        public enum Unit
        {
            ///Inches
            Inches = 0,
            ///Millimeters
            Millimeters = 1
        }

        private const float PingTime = 10e-6f;
        private const double MaxUltrasonicTime = 0.1;
        private const double SpeedOfSoundInchesPerSec = 1130.0 * 12.0;
        private static readonly List<Ultrasonic> s_currentSensors = new List<Ultrasonic>();
        private static bool s_automaticRoundRobinEnabled = false;
        private static BackgroundWorker s_autoSensingWorker = null;
        private static byte s_instances;

        private DigitalInput m_echoChannel = null;
        private DigitalOutput m_pingChannel = null;
        private readonly bool m_allocatedChannels;
        private Counter m_counter = null;

        private readonly object m_syncRoot = new object();

        protected PIDSourceType m_pidSource = PIDSourceType.Displacement;

        private static void GetUltrasonicChecker(object sender, EventArgs args)
        {
            while (s_automaticRoundRobinEnabled)
            {
                foreach (var sensor in s_currentSensors)
                {
                    if (sensor.Enabled)
                        sensor.m_pingChannel.Pulse(PingTime);
                }
                Timer.Delay(.1);
            }
        }

        private void Initialize()
        {
            lock (m_syncRoot)
            {
                bool originalMode = s_automaticRoundRobinEnabled;
                SetAutomaticMode(false);
                m_counter = new Counter { MaxPeriod = 1.0 };
                m_counter.SetSemiPeriodMode(true);
                m_counter.Reset();
                Enabled = true;
                s_currentSensors.Add(this);
                SetAutomaticMode(originalMode);
                ++s_instances;
                HAL.Report(ResourceType.kResourceType_Ultrasonic, s_instances);
                LiveWindow.AddSensor("Ultrasonic", m_echoChannel.Channel, this);
            }
        }

        /// <summary>
        /// Create an instance of the <see cref="Ultrasonic"/> Sensor.
        /// </summary>
        /// <remarks>This is designed to superchannel the Deventech SRF04 and 
        /// Vex ultrasonic sensors.</remarks>
        /// <param name="pingChannel">The digital output channel that sends the pulse 
        /// to initiate the sensor sending the ping</param>
        /// <param name="echoChannel">The digital input channel the receives the echo.
        /// The lenght of time that the echo is high represents the round trip time of 
        /// the ping, and the distance/</param>
        /// <param name="units">The units returns in either <see cref="Unit.Inches">Inches</see>
        /// or <see cref="Unit.Millimeters">Millimeters</see>. Default is inches.</param>
        public Ultrasonic(int pingChannel, int echoChannel, Unit units = Unit.Inches)
        {
            m_pingChannel = new DigitalOutput(pingChannel);
            m_echoChannel = new DigitalInput(echoChannel);
            m_allocatedChannels = true;
            DistanceUnits = units;
            Initialize();
        }

        /// <summary>
        /// Create an instance of the <see cref="Ultrasonic"/> Sensor.
        /// </summary>
        /// <remarks>This is designed to superchannel the Deventech SRF04 and 
        /// Vex ultrasonic sensors.</remarks>
        /// <param name="pingChannel">The <see cref="DigitalOutput"/> channel that sends the pulse 
        /// to initiate the sensor sending the ping</param>
        /// <param name="echoChannel">The <see cref="DigitalInput"/> channel the receives the echo.
        /// The lenght of time that the echo is high represents the round trip time of 
        /// the ping, and the distance/</param>
        /// <param name="units">The units returns in either <see cref="Unit.Inches">Inches</see>
        /// or <see cref="Unit.Millimeters">Millimeters</see></param>
        public Ultrasonic(DigitalOutput pingChannel, DigitalInput echoChannel, Unit units = Unit.Inches)
        {
            if (pingChannel == null) throw new ArgumentNullException(nameof(pingChannel));
            if (echoChannel == null) throw new ArgumentNullException(nameof(echoChannel));
            m_pingChannel = pingChannel;
            m_echoChannel = echoChannel;
            m_allocatedChannels = false;
            DistanceUnits = units;
            Initialize();
        }

        /// <summary>
        /// Destructor for the ultrasonic sensor.
        /// </summary>
        /// <remarks>Delete the instance of the ultrasonic sensor by freeing the allocated digital channels. 
        /// If the system was in automatic mode (round robin), then it is stopped, then started again after 
        /// this sensor is removed (provided this wasn't the last sensor).
        /// </remarks>
        public override void Dispose()
        {
            base.Dispose();
            lock (m_syncRoot)
            {
                bool previousAutoMode = s_automaticRoundRobinEnabled;
                SetAutomaticMode(false);
                if (m_allocatedChannels)
                {
                    m_pingChannel?.Dispose();
                    m_echoChannel?.Dispose();
                }
                m_counter?.Dispose();
                m_pingChannel = null;
                m_echoChannel = null;
                m_counter = null;
                s_currentSensors.Remove(this);
                if (!s_currentSensors.Any()) return;
                SetAutomaticMode(previousAutoMode);
            }
        }

        /// <summary>
        /// Turn Automatic mode on/off.
        /// </summary>
        /// <remarks>When in Automatic mode, all sensors will fire in round robin,
        /// waiting a set time between each sensor.</remarks>
        /// <param name="enabling">Set to true to enable round robin scheduling.</param>
        public static void SetAutomaticMode(bool enabling)
        {
            if (enabling == s_automaticRoundRobinEnabled) return;
            s_automaticRoundRobinEnabled = enabling;
            if (enabling)
            {
                s_currentSensors.ForEach(s => s.m_counter.Reset());
                s_autoSensingWorker = new BackgroundWorker();
                s_autoSensingWorker.DoWork += GetUltrasonicChecker;
                s_autoSensingWorker.RunWorkerAsync();
            }
            else
            {
                while (s_autoSensingWorker.IsBusy)
                {
                    Timer.Delay(MaxUltrasonicTime * 1.5);
                }
                s_currentSensors.ForEach(s => s.m_counter.Reset());
            }
        }

        /// <summary>
        /// Get or Set whether automatic mode is enabled.
        /// </summary>
        public static bool AutomaticModeEnabled
        {
            get { return s_automaticRoundRobinEnabled; }
            set { SetAutomaticMode(value); }
        }

        /// <summary>
        /// Ping a single ultrasonic sensor.
        /// </summary>
        /// <remarks>This only work if automatic mode is disabled. A single ping is sent out, and the counter
        /// should count the semi-period when it comes in. The counter is reset to make the current
        /// value invalid.</remarks>
        public void Ping()
        {
            SetAutomaticMode(false);
            m_counter.Reset();
            m_pingChannel.Pulse(PingTime);
        }

        /// <summary>
        /// Check if there is a valid range measurement.
        /// </summary>
        /// <remarks>The ranges are accumulated in a counter that will increment on each edge of the
        /// echo (return) signal. If the count is not at least 2, then the range has not yet been
        /// measured, and is invalid.</remarks>
        /// <returns>If the range is valid.</returns>
        public bool IsRangeValid()
        {
            return m_counter.Get() > 1;
        }

        /// <summary>
        /// Get the range in inches from the ultrasonic sensor.
        /// </summary>
        /// <returns>Range in inches of the target returned from the ultrasonic sensor. 
        /// Returns 0 if there is no valid value.</returns>
        public double GetRangeInches()
        {
            if (IsRangeValid()) return m_counter.GetPeriod() * SpeedOfSoundInchesPerSec / 2;
            return 0;
        }

        /// <summary>
        /// Get the range in millimeters from the ultrasonic sensor.
        /// </summary>
        /// <returns>Range in millimeters of the target returned from the ultrasonic sensor. 
        /// Returns 0 if there is no valid value.</returns>
        public double GetRangeMM()
        {
            return GetRangeInches() * 25.4;
        }

        /// <summary>
        /// Get the result to use in PIDController
        /// </summary>
        /// <returns>The result to use in PIDController</returns>
        public double PidGet()
        {
            switch (DistanceUnits)
            {
                case Unit.Inches:
                    return GetRangeInches();
                case Unit.Millimeters:
                    return GetRangeMM();
                default:
                    return 0.0;
            }
        }

        /// <inheritdoc/>
        public PIDSourceType PIDSourceType
        {
            get { return m_pidSource; }
            set
            {
                if (value != PIDSourceType.Displacement)
                {
                    throw new ArgumentOutOfRangeException(nameof(value),
                        "Only displacement PID is allowed for ultrasonics.");
                }
                m_pidSource = value;
            }
        }

        /// <summary>
        /// Set the current Distance Unit that should be used for the <see cref="IPIDSource"/>
        /// base object.
        /// </summary>
        /// <param name="units">The <see cref="Unit">Distance Unit</see> to use.</param>
        [Obsolete("Use DistanceUnits property instead.")]
        public void SetDistanceUnits(Unit units)
        {
            DistanceUnits = units;
        }

        /// <summary>
        /// Get the current <see cref="Unit">Distance Unit</see> that is used for the
        /// <see cref="IPIDSource"/> base object.
        /// </summary>
        /// <returns>The type of <see cref="Unit">Distance Unit</see> that is being used.</returns>
        [Obsolete("Use DistanceUnits property instead.")]
        public Unit GetDistanceUnits() => DistanceUnits;

        /// <summary>
        /// Gets or Sets the current <see cref="Unit">Distance Unit</see> that is used for the 
        /// <see cref="IPIDSource"/> base object.
        /// </summary>
        public Unit DistanceUnits { get; set; }

        /// <summary>
        /// Gets if the ultrasonic is enabled
        /// </summary>
        /// <returns>true if enabled</returns>
        [Obsolete("Use Enabled property instead.")]
        public bool GetEnabled() { return Enabled; }

        /// <summary>
        /// Set if the ultrasonic is enabled.
        /// </summary>
        /// <param name="enabled">true if enabled</param>
        [Obsolete("Use Enabled property instead.")]
        public void SetEnabled(bool enabled) { Enabled = enabled; }

        /// <summary>
        /// Gets or Sets whether the ultrasonic is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <inheridoc/>
        public string SmartDashboardType => "Ultrasonic";

        /// <inheridoc/>
        public void InitTable(ITable subtable)
        {
            Table = subtable;
            UpdateTable();
        }

        /// <inheridoc/>
        public ITable Table { get; private set; }

        /// <inheridoc/>
        public void UpdateTable()
        {
            Table?.PutNumber("Value", GetRangeInches());
        }

        /// <inheridoc/>
        public void StartLiveWindowMode() { }

        /// <inheridoc/>
        public void StopLiveWindowMode() { }
    }
}
