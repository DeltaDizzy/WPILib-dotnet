﻿using System;
using NetworkTables;
using NetworkTables.Tables;
using WPILib.CAN;
using WPILib.Interfaces;
using WPILib.LiveWindow;
using static HAL.Base.HALCAN;
using static HAL.Base.HALCAN.Constants;

namespace WPILib
{
    /// <summary>
    /// This class represents a CAN Jaguar Motor Controller.
    /// </summary>
    public class CANJaguar : IMotorSafety, ICANSpeedController, IPIDInterface, ITableListener, ILiveWindowSendable, IDisposable
    {
        /// <summary>
        /// The Max Message send size allowed.
        /// </summary>
        public const int MaxMessageDataSize = 8;

        /// <summary>
        /// The internal control rate (in Hz)
        /// </summary>
        public const int ControllerRate = 1000;

        /// <summary>
        /// The approximate bus voltage of the power system.
        /// </summary>
        public const double ApproxBusVoltage = 12.0;

        private MotorSafetyHelper m_safetyHelper;
        private static readonly Resource s_allocated = new Resource(63);

        private const int FullMessageIdMask = CAN_MSGID_API_M | CAN_MSGID_MFR_M | CAN_MSGID_DTYPE_M;
        private const int SendMessagePeriod = 20;

        /// <summary>
        /// The source mode for the CAN Jaguar
        /// </summary>
        public enum SourceMode
        {
            /// <summary>
            /// Use an encoder as the source.
            /// </summary>
            Encoder,
            /// <summary>
            /// Use a quadrature encoder as the source.
            /// </summary>
            QuadEncoder,
            /// <summary>
            /// Use a potentiometer as the source.
            /// </summary>
            Potentiometer,
            /// <summary>
            /// Use no source.
            /// </summary>
            None,
        }

        ///<inheritdoc/>
        public bool Inverted { get; set; }

        /// <summary>
        /// Creates a new <see cref="CANJaguar"/> with a specific device number.
        /// </summary>
        /// <param name="deviceNumber">The CAN Id of the Jaguar.</param>
        public CANJaguar(int deviceNumber)
        {
            s_allocated.Allocate(deviceNumber - 1, "CANJaguar device " + deviceNumber + " is already allocated");

            m_deviceNumber = (byte)deviceNumber;
            m_controlMode = ControlMode.PercentVbus;

            m_safetyHelper = new MotorSafetyHelper(this);

            bool receivedFirmwareVersion = false;

            byte[] data = new byte[8];

            RequestMessage(CAN_IS_FRAME_REMOTE | CAN_MSGID_API_FIRMVER);
            RequestMessage(LM_API_HWVER);

            for (int i = 0; i < ReceiveStatusAttempts; i++)
            {
                Timer.Delay(0.001);
                SetupPeriodicStatus();
                UpdatePeriodicStatus();

                if (!receivedFirmwareVersion)
                {
                    try
                    {
                        GetMessage(CAN_MSGID_API_FIRMVER, CAN_MSGID_FULL_M, data);
                        m_firmwareVersion = UnpackInt32(data);
                        receivedFirmwareVersion = true;
                    }
                    catch (CANMessageNotFoundException)
                    {

                    }
                }

                if (m_receivedStatusMessage0 &&
                    m_receivedStatusMessage1 &&
                    m_receivedStatusMessage2 &&
                    receivedFirmwareVersion)
                {
                    break;
                }
            }

            if (!m_receivedStatusMessage0 ||
                !m_receivedStatusMessage1 ||
                !m_receivedStatusMessage2 ||
                !receivedFirmwareVersion)
            {
                Dispose();
                throw new CANMessageNotFoundException();
            }

            try
            {
                GetMessage(LM_API_HWVER, CAN_MSGID_FULL_M, data);
                m_hardwareVersion = data[0];
            }
            catch (CANMessageNotFoundException)
            {
                m_hardwareVersion = 0;
            }

            if (m_firmwareVersion >= 3330 || m_firmwareVersion < 108)
            {
                if (m_firmwareVersion < 3330)
                {
                    DriverStation.ReportError("Jag " + m_deviceNumber + " firmware " + m_firmwareVersion + " is too old (must be at least version 108 of the FIRST approved firmware)", false);
                }
                else
                {
                    DriverStation.ReportError("Jag" + m_deviceNumber + " firmware " + m_firmwareVersion + " is not FIRST approved (must be at least version 108 of the FIRST approved firmware)", false);
                }
                return;
            }


        }

        /// <inheritdoc/>
        public void Dispose()
        {
            s_allocated.Deallocate(m_deviceNumber - 1);
            m_safetyHelper = null;
            int status = 0;

            int messageId;

            switch (m_controlMode)
            {
                case ControlMode.PercentVbus:
                    messageId = (int)m_controlMode | LM_API_VOLT_T_SET;
                    break;
                case ControlMode.Speed:
                    messageId = (int)m_controlMode | LM_API_SPD_T_SET;
                    break;
                case ControlMode.Position:
                    messageId = (int)m_controlMode | LM_API_POS_T_SET;
                    break;
                case ControlMode.Current:
                    messageId = (int)m_controlMode | LM_API_ICTRL_T_SET;
                    break;
                case ControlMode.Voltage:
                    messageId = (int)m_controlMode | LM_API_VCOMP_T_SET;
                    break;
                default:
                    return;

            }

            FRC_NetworkCommunication_CANSessionMux_sendMessage((uint)messageId, null, 0,
                CAN_SEND_PERIOD_STOP_REPEATING, ref status);
        }

        /// <summary>
        /// Gets the current device number.
        /// </summary>
        public int DeviceNumber => m_deviceNumber;


        readonly byte m_deviceNumber;
        double m_value = 0.0f;

        // Parameters/configuration
        ControlMode m_controlMode;
        int m_speedReference = LM_REF_NONE;
        int m_positionReference = LM_REF_NONE;
        double m_p = 0.0;
        double m_i = 0.0;
        double m_d = 0.0;
        NeutralMode m_neutralMode = NeutralMode.Jumper;
        short m_encoderCodesPerRev = 0;
        short m_potentiometerTurns = 0;
        readonly LimitMode m_limitMode = LimitMode.SwitchInputsOnly;
        double m_forwardLimit = 0.0;
        double m_reverseLimit = 0.0;
        double m_maxOutputVoltage = ApproxBusVoltage;
        readonly double m_voltageRampRate = 0.0;
        float m_faultTime = 0.0f;

        // Which parameters have been verified since they were last set?
        bool m_controlModeVerified = true;
        bool m_speedRefVerified = true;
        bool m_posRefVerified = true;
        bool m_pVerified = true;
        bool m_iVerified = true;
        bool m_dVerified = true;
        bool m_neutralModeVerified = true;
        bool m_encoderCodesPerRevVerified = true;
        bool m_potentiometerTurnsVerified = true;
        bool m_forwardLimitVerified = true;
        bool m_reverseLimitVerified = true;
        bool m_limitModeVerified = true;
        bool m_maxOutputVoltageVerified = true;
        bool m_voltageRampRateVerified = true;
        bool m_faultTimeVerified = true;

        // Status data
        double m_busVoltage = 0.0f;
        double m_outputVoltage = 0.0f;
        double m_outputCurrent = 0.0f;
        double m_temperature = 0.0f;
        double m_position = 0.0;
        double m_speed = 0.0;
        byte m_limits = 0;
        short m_faults = 0;
        readonly int m_firmwareVersion = 0;
        readonly byte m_hardwareVersion = 0;

        // Which periodic status messages have we received at least once?
        bool m_receivedStatusMessage0 = false;
        bool m_receivedStatusMessage1 = false;
        bool m_receivedStatusMessage2 = false;

        private const int ReceiveStatusAttempts = 50;

        bool m_controlEnabled = true;
        bool m_stopped = false;

        /// <inheritdoc/>
        public double Expiration
        {
            set { m_safetyHelper.Expiration = value; }
            get { return m_safetyHelper.Expiration; }
        }

        /// <inheritdoc/>
        public bool Alive => m_safetyHelper.Alive;

        /// <inheritdoc/>
        public void StopMotor()
        {
            DisableControl();
        }

        /// <inheritdoc/>
        public bool SafetyEnabled
        {
            set { m_safetyHelper.SafetyEnabled = value; }
            get { return m_safetyHelper.SafetyEnabled; }
        }

        /// <inheritdoc/>
        public string Description => "CANJaguar ID " + m_deviceNumber;

        /// <inheritdoc/>
        public void PidWrite(double value)
        {
            if (m_controlMode == ControlMode.PercentVbus)
            {
                Set(value);
            }
            else
            {
                throw new InvalidOperationException("PID only supported in PercentVbus mode");
            }
        }

        private void SetSpeedReference(int reference)
        {
            SendMessage(LM_API_SPD_REF, new[] { (byte)reference }, 1);
            m_speedReference = reference;
            m_speedRefVerified = false;
        }

        /// <summary>
        /// Enables the closed loop controller.
        /// </summary>
        /// <remarks>
        /// Start actually controlling the output based on teh feedback. If starting a 
        /// position controller with an encoder reference, use the encoderInitialPosition
        /// parameter to initialize the encoder state.
        /// </remarks>
        /// <param name="encoderInitialPosition">Encoder position to set if position is an encoder reference. 
        /// Otherwise ignored.</param>
        public void EnableControl(double encoderInitialPosition)
        {
            switch (m_controlMode)
            {
                case ControlMode.PercentVbus:
                    SendMessage(LM_API_VOLT_T_EN, new byte[0], 0);
                    break;

                case ControlMode.Speed:
                    SendMessage(LM_API_SPD_T_EN, new byte[0], 0);
                    break;

                case ControlMode.Position:
                    byte[] data = new byte[8];
                    int dataSize = PackFXP16_16(data, encoderInitialPosition);
                    SendMessage(LM_API_POS_T_EN, data, dataSize);
                    break;

                case ControlMode.Current:
                    SendMessage(LM_API_ICTRL_T_EN, new byte[0], 0);
                    break;

                case ControlMode.Voltage:
                    SendMessage(LM_API_VCOMP_T_EN, new byte[0], 0);
                    break;
            }

            m_controlEnabled = true;
        }

        /// <summary>
        /// Enableds the closed loop controller.
        /// </summary>
        /// <remarks>Starts actually controlling the output based on the feedback.
        /// This is the same as calling <see cref="EnableControl(double)"/> with a 0.0
        /// parameter.</remarks>
        public void EnableControl()
        {
            EnableControl(0.0);
        }

        /// <summary>
        /// Disables the closed loop controller.
        /// </summary>
        public void DisableControl()
        {
            // Disable all control modes.
            SendMessage(LM_API_VOLT_DIS, new byte[0], 0);
            SendMessage(LM_API_SPD_DIS, new byte[0], 0);
            SendMessage(LM_API_POS_DIS, new byte[0], 0);
            SendMessage(LM_API_ICTRL_DIS, new byte[0], 0);
            SendMessage(LM_API_VCOMP_DIS, new byte[0], 0);

            // Stop all periodic setpoints
            SendMessage(LM_API_VOLT_T_SET, new byte[0], 0, CAN_SEND_PERIOD_STOP_REPEATING);
            SendMessage(LM_API_SPD_T_SET, new byte[0], 0, CAN_SEND_PERIOD_STOP_REPEATING);
            SendMessage(LM_API_POS_T_SET, new byte[0], 0, CAN_SEND_PERIOD_STOP_REPEATING);
            SendMessage(LM_API_ICTRL_T_SET, new byte[0], 0, CAN_SEND_PERIOD_STOP_REPEATING);
            SendMessage(LM_API_VCOMP_T_SET, new byte[0], 0, CAN_SEND_PERIOD_STOP_REPEATING);

            m_controlEnabled = false;
        }

        /// <inheritdoc/>
        public void Set(double value)
        {
            Set(value, 0);
        }

        /// <inheritdoc/>
        public virtual double Get()
        {
            return m_value;
        }

        /// <inheritdoc/>
        public double Setpoint
        {
            get
            {
                return Get();
            }
            set
            {
                Set(value);
            }
        }

        /// <inheritdoc/>
        public double GetError()
        {
            return Get() - GetPosition();
        }

        /// <inheritdoc/>
        public virtual void Set(double value, byte syncGroup)
        {
            byte[] data = new byte[8];

            m_safetyHelper?.Feed();
            if (m_stopped)
            {
                EnableControl();
                m_stopped = false;
            }

            if (m_controlEnabled)
            {
                int messageId;
                byte dataSize;
                switch (m_controlMode)
                {
                    case ControlMode.PercentVbus:
                        messageId = LM_API_VOLT_T_SET;
                        dataSize = PackPercentage(data, Inverted ? -value : value);
                        break;
                    case ControlMode.Speed:
                        messageId = LM_API_SPD_T_SET;
                        dataSize = PackFXP16_16(data, Inverted ? -value : value);
                        break;
                    case ControlMode.Position:
                        messageId = LM_API_POS_T_SET;
                        dataSize = PackFXP16_16(data, value);
                        break;
                    case ControlMode.Current:
                        messageId = LM_API_ICTRL_T_SET;
                        dataSize = PackFXP8_8(data, value);
                        break;
                    case ControlMode.Voltage:
                        messageId = LM_API_VCOMP_T_SET;
                        dataSize = PackFXP8_8(data, Inverted ? -value : value);
                        break;
                    default:
                        return;

                }

                if (syncGroup != 0)
                {
                    data[dataSize++] = syncGroup;
                }

                SendMessage(messageId, data, dataSize, SendMessagePeriod);
            }
            m_value = value;
            Verify();
        }

        /// <summary>
        /// Check all unverified params an make sure they are equal to their local
        /// cached versions.
        /// </summary>
        /// <remarks>If a value isn't available, it gets requested. If a value doesn't match
        /// up, it gets set again.</remarks>
        protected void Verify()
        {
            byte[] data = new byte[8];

            try
            {
                GetMessage(LM_API_STATUS_POWER, CAN_MSGID_FULL_M, data);
                bool powerCycled = data[0] != 0;

                if (powerCycled)
                {
                    data[0] = 1;
                    SendMessage(LM_API_STATUS_POWER, data, 1);

                    m_controlModeVerified = false;
                    m_speedRefVerified = false;
                    m_posRefVerified = false;
                    m_neutralModeVerified = false;
                    m_encoderCodesPerRevVerified = false;
                    m_potentiometerTurnsVerified = false;
                    m_forwardLimitVerified = false;
                    m_reverseLimitVerified = false;
                    m_limitModeVerified = false;
                    m_maxOutputVoltageVerified = false;
                    m_faultTimeVerified = false;

                    if (m_controlMode == ControlMode.PercentVbus || m_controlMode == ControlMode.Voltage)
                    {
                        m_voltageRampRateVerified = false;
                    }
                    else
                    {
                        m_pVerified = false;
                        m_iVerified = false;
                        m_dVerified = false;
                    }

                    m_receivedStatusMessage0 = false;
                    m_receivedStatusMessage1 = false;
                    m_receivedStatusMessage2 = false;

                    int[] messages = {
                        LM_API_SPD_REF, LM_API_POS_REF,
                        LM_API_SPD_PC, LM_API_POS_PC,
                        LM_API_ICTRL_PC, LM_API_SPD_IC,
                        LM_API_POS_IC, LM_API_ICTRL_IC,
                        LM_API_SPD_DC, LM_API_POS_DC,
                        LM_API_ICTRL_DC, LM_API_CFG_ENC_LINES,
                        LM_API_CFG_POT_TURNS, LM_API_CFG_BRAKE_COAST,
                        LM_API_CFG_LIMIT_MODE, LM_API_CFG_LIMIT_REV,
                        LM_API_CFG_MAX_VOUT, LM_API_VOLT_SET_RAMP,
                        LM_API_VCOMP_COMP_RAMP, LM_API_CFG_FAULT_TIME,
                        LM_API_CFG_LIMIT_FWD
                    };

                    foreach (int message in messages)
                    {
                        try
                        {
                            GetMessage(message, CAN_MSGID_FULL_M, data);
                        }
                        catch (CANMessageNotFoundException)
                        {

                        }
                    }
                }
            }
            catch (CANMessageNotFoundException)
            {
                RequestMessage(LM_API_STATUS_POWER);
            }

            if (!m_controlModeVerified && m_controlEnabled)
            {
                try
                {
                    GetMessage(LM_API_STATUS_CMODE, CAN_MSGID_FULL_M, data);
                    ControlMode mode = (ControlMode)data[0];

                    if (m_controlMode == mode)
                    {
                        m_controlModeVerified = true;
                    }
                    else
                    {
                        EnableControl();
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    RequestMessage(LM_API_STATUS_CMODE);
                }
            }

            if (!m_speedRefVerified)
            {
                try
                {
                    GetMessage(LM_API_SPD_REF, CAN_MSGID_FULL_M, data);

                    int speedRef = data[0];

                    if (m_speedReference == speedRef)
                    {
                        m_speedRefVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        SetSpeedReference(m_speedReference);
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_SPD_REF);
                }
            }

            if (!m_posRefVerified)
            {
                try
                {
                    GetMessage(LM_API_POS_REF, CAN_MSGID_FULL_M, data);

                    int posRef = data[0];

                    if (m_positionReference == posRef)
                    {
                        m_posRefVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        SetPositionReference(m_positionReference);
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_POS_REF);
                }
            }

            if (!m_pVerified)
            {
                int message = 0;

                switch (m_controlMode)
                {
                    case ControlMode.Speed:
                        message = LM_API_SPD_PC;
                        break;

                    case ControlMode.Position:
                        message = LM_API_POS_PC;
                        break;

                    case ControlMode.Current:
                        message = LM_API_ICTRL_PC;
                        break;

                    default:
                        break;
                }

                try
                {
                    GetMessage(message, CAN_MSGID_FULL_M, data);

                    double p = UnpackFXP16_16(data);

                    if (FXP16_EQ(m_p, p))
                    {
                        m_pVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        P = m_p;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(message);
                }
            }

            if (!m_iVerified)
            {
                int message = 0;

                switch (m_controlMode)
                {
                    case ControlMode.Speed:
                        message = LM_API_SPD_IC;
                        break;

                    case ControlMode.Position:
                        message = LM_API_POS_IC;
                        break;

                    case ControlMode.Current:
                        message = LM_API_ICTRL_IC;
                        break;

                    default:
                        break;
                }

                try
                {
                    GetMessage(message, CAN_MSGID_FULL_M, data);

                    double i = UnpackFXP16_16(data);

                    if (FXP16_EQ(m_i, i))
                    {
                        m_iVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        I = m_i;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(message);
                }
            }

            if (!m_dVerified)
            {
                int message = 0;

                switch (m_controlMode)
                {
                    case ControlMode.Speed:
                        message = LM_API_SPD_DC;
                        break;

                    case ControlMode.Position:
                        message = LM_API_POS_DC;
                        break;

                    case ControlMode.Current:
                        message = LM_API_ICTRL_DC;
                        break;

                    default:
                        break;
                }

                try
                {
                    GetMessage(message, CAN_MSGID_FULL_M, data);

                    double d = UnpackFXP16_16(data);

                    if (FXP16_EQ(m_d, d))
                    {
                        m_dVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        D = m_d;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(message);
                }
            }

            if (!m_neutralModeVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_BRAKE_COAST, CAN_MSGID_FULL_M, data);

                    NeutralMode mode = (NeutralMode)data[0];

                    if (mode == m_neutralMode)
                    {
                        m_neutralModeVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        NeutralMode = m_neutralMode;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_BRAKE_COAST);
                }
            }

            if (!m_encoderCodesPerRevVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_ENC_LINES, CAN_MSGID_FULL_M, data);

                    short codes = UnpackInt16(data);

                    if (codes == m_encoderCodesPerRev)
                    {
                        m_encoderCodesPerRevVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        EncoderCodesPerRev = m_encoderCodesPerRev;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_ENC_LINES);
                }
            }

            if (!m_potentiometerTurnsVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_POT_TURNS, CAN_MSGID_FULL_M, data);

                    short turns = UnpackInt16(data);

                    if (turns == m_potentiometerTurns)
                    {
                        m_potentiometerTurnsVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        PotentiometerTurns = m_potentiometerTurns;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_POT_TURNS);
                }
            }

            if (!m_limitModeVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_LIMIT_MODE, CAN_MSGID_FULL_M, data);

                    LimitMode mode = (LimitMode)data[0];

                    if (mode == m_limitMode)
                    {
                        m_limitModeVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        LimitMode = m_limitMode;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_LIMIT_MODE);
                }
            }

            if (!m_forwardLimitVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_LIMIT_FWD, CAN_MSGID_FULL_M, data);

                    double limit = UnpackFXP16_16(data);

                    if (FXP16_EQ(limit, m_forwardLimit))
                    {
                        m_forwardLimitVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        ForwardLimit = m_forwardLimit;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_LIMIT_FWD);
                }
            }

            if (!m_reverseLimitVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_LIMIT_REV, CAN_MSGID_FULL_M, data);

                    double limit = UnpackFXP16_16(data);

                    if (FXP16_EQ(limit, m_reverseLimit))
                    {
                        m_reverseLimitVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        ReverseLimit = m_reverseLimit;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_LIMIT_REV);
                }
            }

            if (!m_maxOutputVoltageVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_MAX_VOUT, CAN_MSGID_FULL_M, data);

                    double voltage = UnpackFXP8_8(data);

                    // The returned max output voltage is sometimes slightly higher
                    // or lower than what was sent.  This should not trigger
                    // resending the message.
                    if (Math.Abs(voltage - m_maxOutputVoltage) < 0.1)
                    {
                        m_maxOutputVoltageVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        MaxOutputVoltage = m_maxOutputVoltage;
                    }

                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_MAX_VOUT);
                }
            }

            if (!m_voltageRampRateVerified)
            {
                if (m_controlMode == ControlMode.PercentVbus)
                {
                    try
                    {
                        GetMessage(LM_API_VOLT_SET_RAMP, CAN_MSGID_FULL_M, data);

                        double rate = UnpackPercentage(data);

                        if (FXP16_EQ(rate, m_voltageRampRate))
                        {
                            m_voltageRampRateVerified = true;
                        }
                        else
                        {
                            // It's wrong - set it again
                            VoltageRampRate = (m_voltageRampRate);
                        }

                    }
                    catch (CANMessageNotFoundException)
                    {
                        // Verification is needed but not available - request it again.
                        RequestMessage(LM_API_VOLT_SET_RAMP);
                    }
                }
            }
            else if (m_controlMode == ControlMode.Voltage)
            {
                try
                {
                    GetMessage(LM_API_VCOMP_COMP_RAMP, CAN_MSGID_FULL_M, data);

                    double rate = UnpackFXP8_8(data);

                    if (FXP8_EQ(rate, m_voltageRampRate))
                    {
                        m_voltageRampRateVerified = true;
                    }
                    else
                    {
                        // It's wrong - set it again
                        VoltageRampRate = (m_voltageRampRate);
                    }

                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_VCOMP_COMP_RAMP);
                }
            }

            if (!m_faultTimeVerified)
            {
                try
                {
                    GetMessage(LM_API_CFG_FAULT_TIME, CAN_MSGID_FULL_M, data);

                    int faultTime = UnpackInt16(data);

                    if ((int)(m_faultTime * 1000.0) == faultTime)
                    {
                        m_faultTimeVerified = true;
                    }
                    else
                    {
                        //It's wrong - set it again
                        FaultTime = m_faultTime;
                    }
                }
                catch (CANMessageNotFoundException)
                {
                    // Verification is needed but not available - request it again.
                    RequestMessage(LM_API_CFG_FAULT_TIME);
                }
            }

            if (!m_receivedStatusMessage0 ||
                    !m_receivedStatusMessage1 ||
                    !m_receivedStatusMessage2)
            {
                // If the periodic status messages haven't been verified as received,
                // request periodic status messages again and attempt to unpack any
                // available ones.
                SetupPeriodicStatus();
                //The properties are called just to update their periodic status.
                var tmp = GetTemperature();
                var tmp2 = GetPosition();
                var tmp3 = GetFaults();
            }
        }

        private static void SendMessageHelper(int messageId, byte[] data, int dataSize, int period)
        {
            int[] kTrustedMessages = {
                LM_API_VOLT_T_EN, LM_API_VOLT_T_SET, LM_API_SPD_T_EN, LM_API_SPD_T_SET,
                LM_API_VCOMP_T_EN, LM_API_VCOMP_T_SET, LM_API_POS_T_EN, LM_API_POS_T_SET,
                LM_API_ICTRL_T_EN, LM_API_ICTRL_T_SET
            };
            int status = 0;

            for (byte i = 0; i < kTrustedMessages.Length; i++)
            {
                if ((FullMessageIdMask & messageId) == kTrustedMessages[i])
                {
                    if (dataSize > MaxMessageDataSize - 2)
                    {
                        throw new Exception("CAN message has too much data.");
                    }

                    byte[] trustedData = new byte[dataSize + 2];
                    trustedData[0] = 0;
                    trustedData[1] = 0;
                    for (byte j = 0; j < dataSize; j++)
                    {
                        trustedData[j + 2] = data[j];
                    }

                    FRC_NetworkCommunication_CANSessionMux_sendMessage((uint)messageId, trustedData, (byte)(dataSize + 2), period, ref status);
                    if (status < 0)
                    {
                        CANExceptionFactory.CheckStatus(status, messageId);
                    }

                    return;
                }
            }

            FRC_NetworkCommunication_CANSessionMux_sendMessage((uint)messageId, data, (byte)dataSize, period, ref status);

            if (status < 0)
            {
                CANExceptionFactory.CheckStatus(status, messageId);
            }


        }

        /// <summary>
        /// Send a message to the Jaguar periodically.
        /// </summary>
        /// <param name="messageId">The messageId to be used on the CAN bus (device number is added internally).</param>
        /// <param name="data">The up to 8 bytes of data to be sent with the message.</param>
        /// <param name="dataSize">Specify how much of the data in the data buffer to send.</param>
        /// <param name="period">The period in ms to send the message.</param>
        protected void SendMessage(int messageId, byte[] data, int dataSize, int period)
        {
            SendMessageHelper(messageId | m_deviceNumber, data, dataSize, period);
        }

        /// <summary>
        /// Send a message to the Jaguar non periodically.
        /// </summary>
        /// <param name="messageId">The messageId to be used on the CAN bus (device number is added internally).</param>
        /// <param name="data">The up to 8 bytes of data to be sent with the message.</param>
        /// <param name="dataSize">Specify how much of the data in the data buffer to send.</param>
        protected void SendMessage(int messageId, byte[] data, int dataSize)
        {
            SendMessage(messageId, data, dataSize, CAN_SEND_PERIOD_NO_REPEAT);
        }

        /// <summary>
        /// Request a message from the Jaguar periodically, without waiting for it to arrive.
        /// </summary>
        /// <param name="messageId">The message to request</param>
        /// <param name="period">The period to automatically request data at.</param>
        protected void RequestMessage(int messageId, int period)
        {
            SendMessageHelper(messageId | m_deviceNumber, null, 0, period);
        }

        /// <summary>
        /// Request a message from the Jaguar once, without waiting for it to arrive.
        /// </summary>
        /// <param name="messageId">The message to request.</param>
        protected void RequestMessage(int messageId)
        {
            RequestMessage(messageId, CAN_SEND_PERIOD_NO_REPEAT);
        }

        /// <summary>
        /// Get a previously requested message.
        /// </summary>
        /// <remarks>
        /// Jaguar always generates a message with the same messageId when replying.
        /// </remarks>
        /// <param name="messageId">The messageId to read from the CAN bus (device number is added internally).</param>
        /// <param name="messageMask">The mask of data to receive.</param>
        /// <param name="data">The up to 8 bytes of data that was received with the message.</param>
        protected void GetMessage(int messageId, int messageMask, byte[] data)
        {
            uint messageIdu = (uint)messageId;
            messageIdu |= m_deviceNumber;
            messageIdu &= CAN_MSGID_FULL_M;
            byte dataSize = 0;
            uint timeStamp = 0;
            int status = 0;

            FRC_NetworkCommunication_CANSessionMux_receiveMessage(ref messageIdu, (uint)messageMask, data, ref dataSize, ref timeStamp, ref status);

            if (status < 0)
            {
                CANExceptionFactory.CheckStatus(status, messageId);
            }

        }

        /// <summary>
        /// Enables periodic status updates from the Jaguar.
        /// </summary>
        protected void SetupPeriodicStatus()
        {
            byte[] data = new byte[8];

            byte[] kMessage0Data = {
            LM_PSTAT_VOLTBUS_B0, LM_PSTAT_VOLTBUS_B1,
            LM_PSTAT_VOLTOUT_B0, LM_PSTAT_VOLTOUT_B1,
            LM_PSTAT_CURRENT_B0, LM_PSTAT_CURRENT_B1,
            LM_PSTAT_TEMP_B0, LM_PSTAT_TEMP_B1
            };

            byte[] kMessage1Data = {
                LM_PSTAT_POS_B0, LM_PSTAT_POS_B1, LM_PSTAT_POS_B2, LM_PSTAT_POS_B3,
                LM_PSTAT_SPD_B0, LM_PSTAT_SPD_B1, LM_PSTAT_SPD_B2, LM_PSTAT_SPD_B3
            };

            byte[] kMessage2Data = {
                LM_PSTAT_LIMIT_CLR,
                LM_PSTAT_FAULT,
                LM_PSTAT_END,
                0,
                0,
                0,
                0,
                0,
            };

            int dataSize = PackInt16(data, SendMessagePeriod);
            SendMessage(LM_API_PSTAT_PER_EN_S0, data, dataSize);
            SendMessage(LM_API_PSTAT_PER_EN_S1, data, dataSize);
            SendMessage(LM_API_PSTAT_PER_EN_S2, data, dataSize);

            dataSize = 8;
            SendMessage(LM_API_PSTAT_CFG_S0, kMessage0Data, dataSize);
            SendMessage(LM_API_PSTAT_CFG_S1, kMessage1Data, dataSize);
            SendMessage(LM_API_PSTAT_CFG_S2, kMessage2Data, dataSize);
        }

        /// <summary>
        /// Check for new periodic status updates and unpack them into local variables.
        /// </summary>
        protected void UpdatePeriodicStatus()
        {
            byte[] data = new byte[8];

            // Check if a new bus voltage/output voltage/current/temperature message
            // has arrived and unpack the values into the cached member variables
            try
            {
                GetMessage(LM_API_PSTAT_DATA_S0, CAN_MSGID_FULL_M, data);

                m_busVoltage = UnpackFXP8_8(new[] { data[0], data[1] });
                m_outputVoltage = UnpackPercentage(new[] { data[2], data[3] }) * m_busVoltage;
                m_outputCurrent = UnpackFXP8_8(new[] { data[4], data[5] });
                m_temperature = UnpackFXP8_8(new[] { data[6], data[7] });

                m_receivedStatusMessage0 = true;
            }
            catch (CANMessageNotFoundException) { }

            // Check if a new position/speed message has arrived and do the same
            try
            {
                GetMessage(LM_API_PSTAT_DATA_S1, CAN_MSGID_FULL_M, data);

                m_position = UnpackFXP16_16(new[] { data[0], data[1], data[2], data[3] });
                m_speed = UnpackFXP16_16(new[] { data[4], data[5], data[6], data[7] });

                m_receivedStatusMessage1 = true;
            }
            catch (CANMessageNotFoundException) { }

            // Check if a new limits/faults message has arrived and do the same
            try
            {
                GetMessage(LM_API_PSTAT_DATA_S2, CAN_MSGID_FULL_M, data);
                m_limits = data[0];
                m_faults = data[1];

                m_receivedStatusMessage2 = true;
            }
            catch (CANMessageNotFoundException) { }
        }

        /// <summary>
        /// Update all the motors that have pending sets in the syncGroup.
        /// </summary>
        /// <param name="syncGroup">A bitmask of groups to generate synchronous output.</param>
        public static void UpdateSyncGroup(byte syncGroup)
        {
            byte[] data = new byte[8];

            data[0] = syncGroup;

            SendMessageHelper(CAN_MSGID_API_SYNC, data, 1, CAN_SEND_PERIOD_NO_REPEAT);
        }

        private static void Swap16(int x, byte[] buffer)
        {
        }

        private static void Swap32(int x, byte[] buffer)
        {
        }

        private static byte PackPercentage(byte[] buffer, double value)
        {
            if (value < -1.0) value = -1.0;
            if (value > 1.0) value = 1.0;
            short intValue = (short)(value * 32767.0);
            Swap16(intValue, buffer);
            return 2;
        }

        private static byte PackFXP8_8(byte[] buffer, double value)
        {
            short intValue = (short)(value * 256.0);
            Swap16(intValue, buffer);
            return 2;
        }

        private static byte PackFXP16_16(byte[] buffer, double value)
        {
            int intValue = (int)(value * 65536.0);
            Swap32(intValue, buffer);
            return 4;
        }

        private static byte PackInt16(byte[] buffer, short value)
        {
            Swap16(value, buffer);
            return 2;
        }

        private static byte PackInt32(byte[] buffer, int value)
        {
            Swap32(value, buffer);
            return 4;
        }

        private static short Unpack16(byte[] buffer, int offset)
        {
            return (short)((buffer[offset] & 0xFF) | (short)((buffer[offset + 1] << 8)) & 0xFF00);
        }

        private static int Unpack32(byte[] buffer, int offset)
        {
            return (buffer[offset] & 0xFF) | ((buffer[offset + 1] << 8) & 0xFF00) |
                ((buffer[offset + 2] << 16) & 0xFF0000) | (int)((buffer[offset + 3] << 24) & 0xFF000000);
        }

        private static double UnpackPercentage(byte[] buffer)
        {
            return Unpack16(buffer, 0) / 32767.0;
        }

        private static double UnpackFXP8_8(byte[] buffer)
        {
            return Unpack16(buffer, 0) / 256.0;
        }

        private static double UnpackFXP16_16(byte[] buffer)
        {
            return Unpack32(buffer, 0) / 65536.0;
        }

        private static short UnpackInt16(byte[] buffer)
        {
            return Unpack16(buffer, 0);
        }

        private static int UnpackInt32(byte[] buffer)
        {
            return Unpack32(buffer, 0);
        }

        /* Compare floats for equality as fixed point numbers */
        /// <summary>
        /// Compare floats for equality as fixed point numbers
        /// </summary>
        /// <param name="a">A to check</param>
        /// <param name="b">B to check</param>
        /// <returns>True if they are equal</returns>
        public bool FXP8_EQ(double a, double b)
        {
            return (int)(a * 256.0) == (int)(b * 256.0);
        }

        /* Compare floats for equality as fixed point numbers */
        /// <summary>
        /// Compare floats for equality as fixed point numbers
        /// </summary>
        /// <param name="a">A to check</param>
        /// <param name="b">B to check</param>
        /// <returns>True if they are equal</returns>
        public bool FXP16_EQ(double a, double b)
        {
            return (int)(a * 65536.0) == (int)(b * 65536.0);
        }

        /// <inheritdoc/>
        public void Disable() => DisableControl();
        /// <inheritdoc/>
        public bool Enabled => m_controlEnabled;
        /// <inheritdoc/>
        public void InitTable(ITable subtable)
        {
            Table = subtable;
            UpdateTable();
        }

        /// <inheritdoc/>
        public ITable Table { get; private set; } = null;
        /// <inheritdoc/>
        public string SmartDashboardType => "CANSpeedController";
        /// <inheritdoc/>
        public void UpdateTable()
        {
            if (Table != null)
            {
                Table.PutString("~TYPE~", SmartDashboardType);
                Table.PutString("Type", nameof(CANJaguar)); // "CANTalon", "CANJaguar" 	
                Table.PutNumber("Mode", (int)MotorControlMode);
                if (MotorControlMode.IsPID())
                {

                    // CANJaguar throws an exception if you try to get its PID constants 
                    // when it's not in a PID-compatible mode 	
                    Table.PutNumber("p", P);
                    Table.PutNumber("i", I);
                    Table.PutNumber("d", D);
                    Table.PutNumber("f", F);
                }

                Table.PutBoolean("Enabled", Enabled);
                Table.PutNumber("Value", Get());
            }
        }
        /// <inheritdoc/>
        public void StartLiveWindowMode()
        {
            Set(0.0);
            Table.AddTableListener(this, true);
        }

        /// <inheritdoc/>
        public void ValueChanged(ITable source, string key, Value value, NotifyFlags flags)
        {
            switch (key)
            {
                case "Enabled":
                    if (value.GetBoolean())
                        Enable();
                    else
                        Disable();
                    break;
                case "Value":
                    Set(value.GetDouble());
                    break;
                case "Mode":
                    MotorControlMode = (ControlMode)(int)(value.GetDouble());
                    break;
            }
            if (MotorControlMode.IsPID())
            {
                switch (key)
                {
                    case "p":
                        P = value.GetDouble();
                        break;
                    case "i":
                        I = value.GetDouble();
                        break;
                    case "d":
                        D = value.GetDouble();
                        break;
                    case "f":
                        F = value.GetDouble();
                        break;
                }
            }
        }
        /// <inheritdoc/>
        public void StopLiveWindowMode()
        {
            Set(0.0);
            Table.RemoveTableListener(this);
        }
        /// <inheritdoc/>
        public void Reset()
        {
            Set(m_value);
            DisableControl();
        }
        /// <inheritdoc/>
        public void Enable()
        {
            EnableControl();
        }

        private void SetPositionReference(int reference)
        {
            SendMessage(LM_API_POS_REF, new[] { (byte)reference }, 1);

            m_positionReference = reference;
            m_posRefVerified = false;
        }

        /// <inheritdoc/>
        public ControlMode MotorControlMode
        {
            get { return m_controlMode; }
            set
            {
                ChangeControlMode(value);
            }
        }

        /// <inheritdoc/>
        public double P
        {
            set
            {
                byte[] data = new byte[8];
                byte dataSize = PackFXP16_16(data, value);

                switch (m_controlMode)
                {
                    case ControlMode.Speed:
                        SendMessage(LM_API_SPD_PC, data, dataSize);
                        break;

                    case ControlMode.Position:
                        SendMessage(LM_API_POS_PC, data, dataSize);
                        break;

                    case ControlMode.Current:
                        SendMessage(LM_API_ICTRL_PC, data, dataSize);
                        break;

                    default:
                        throw new InvalidOperationException(
                            "PID constants only apply in Speed, Position, and Current mode");
                }

                m_p = value;
                m_pVerified = false;
            }
            get
            {
                if (m_controlMode == (ControlMode.PercentVbus) || m_controlMode == (ControlMode.Voltage))
                {
                    throw new InvalidOperationException("PID does not apply in Percent or Voltage control modes");
                }
                return m_p;
            }
        }

        /// <inheritdoc/>
        public double I
        {
            set
            {
                byte[] data = new byte[8];
                byte dataSize = PackFXP16_16(data, value);

                switch (m_controlMode)
                {
                    case ControlMode.Speed:
                        SendMessage(LM_API_SPD_IC, data, dataSize);
                        break;

                    case ControlMode.Position:
                        SendMessage(LM_API_POS_IC, data, dataSize);
                        break;

                    case ControlMode.Current:
                        SendMessage(LM_API_ICTRL_IC, data, dataSize);
                        break;

                    default:
                        throw new InvalidOperationException(
                            "PID constants only apply in Speed, Position, and Current mode");
                }

                m_i = value;
                m_iVerified = false;
            }
            get
            {
                if (m_controlMode == (ControlMode.PercentVbus) || m_controlMode == (ControlMode.Voltage))
                {
                    throw new InvalidOperationException("PID does not apply in Percent or Voltage control modes");
                }
                return m_i;
            }
        }

        /// <inheritdoc/>
        public double D
        {
            set
            {
                byte[] data = new byte[8];
                byte dataSize = PackFXP16_16(data, value);

                switch (m_controlMode)
                {
                    case ControlMode.Speed:
                        SendMessage(LM_API_SPD_DC, data, dataSize);
                        break;

                    case ControlMode.Position:
                        SendMessage(LM_API_POS_DC, data, dataSize);
                        break;

                    case ControlMode.Current:
                        SendMessage(LM_API_ICTRL_DC, data, dataSize);
                        break;

                    default:
                        throw new InvalidOperationException(
                            "PID constants only apply in Speed, Position, and Current mode");
                }

                m_d = value;
                m_dVerified = false;
            }
            get
            {
                if (m_controlMode == (ControlMode.PercentVbus) || m_controlMode == (ControlMode.Voltage))
                {
                    throw new InvalidOperationException("PID does not apply in Percent or Voltage control modes");
                }
                return m_d;
            }
        }

        /// <summary>
        /// Not implemented on CANJaguar.
        /// </summary>
        public double F
        {
            get { return 0.0; }
            set { }
        }

        /// <inheritdoc/>
        public void SetPID(double p, double i, double d)
        {
            P = p;
            I = i;
            D = d;
        }

        /// <summary>
        /// Enable control of the motor voltage with the specified <see cref="SourceMode"/> and number of 
        /// codes per revolution.
        /// </summary>
        /// <remarks>
        /// After calling this you must call <see cref="EnableControl()"/> or <see cref="EnableControl(double)"/>
        /// to enable the device.
        /// </remarks>
        /// <param name="mode">The <see cref="SourceMode"/> to set the controller to.</param>
        /// <param name="codesPerRev">The number of codes per revolution on the encoder or potentiometer.</param>
        public void SetPercentMode(SourceMode mode = SourceMode.None, int codesPerRev = 0)
        {
            ChangeControlMode(ControlMode.PercentVbus);
            switch (mode)
            {
                case SourceMode.Encoder:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_ENCODER);
                    EncoderCodesPerRev = codesPerRev;
                    break;
                case SourceMode.QuadEncoder:
                    SetPositionReference(LM_REF_ENCODER);
                    SetSpeedReference(LM_REF_QUAD_ENCODER);
                    EncoderCodesPerRev = codesPerRev;
                    break;
                case SourceMode.Potentiometer:
                    SetPositionReference(LM_REF_POT);
                    SetSpeedReference(LM_REF_NONE);
                    PotentiometerTurns = 1;
                    break;
                default:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_NONE);
                    break;
            }
        }

        /// <summary>
        /// Enable controlling the motor current with a PID loop based on the specifed source.
        /// </summary>
        /// <remarks>
        /// After calling this you must call <see cref="EnableControl()"/> or <see cref="EnableControl(double)"/>
        /// to enable the device.
        /// </remarks>
        /// <param name="p">The proportional gain of the Jaguar's PID controller</param>
        /// <param name="i">The integral gain of the Jaguar's PID controller</param>
        /// <param name="d">The derivative gain of the Jaguar's PID controller</param>
        /// <param name="mode">The <see cref="SourceMode"/> to set the controller to.</param>
        /// <param name="codesPerRev">The number of codes per revolution on the encoder or potentiometer.</param>
        public void SetCurrentMode(double p, double i, double d, SourceMode mode = SourceMode.None, int codesPerRev = 0)
        {
            ChangeControlMode(ControlMode.Current);
            switch (mode)
            {
                case SourceMode.Encoder:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_NONE);
                    EncoderCodesPerRev = codesPerRev;
                    SetPID(p, i, d);
                    break;
                case SourceMode.QuadEncoder:
                    SetPositionReference(LM_REF_ENCODER);
                    SetSpeedReference(LM_REF_QUAD_ENCODER);
                    EncoderCodesPerRev = codesPerRev;
                    SetPID(p, i, d);
                    break;
                case SourceMode.Potentiometer:
                    SetPositionReference(LM_REF_POT);
                    SetSpeedReference(LM_REF_NONE);
                    PotentiometerTurns = 1;
                    SetPID(p, i, d);
                    break;
                default:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_NONE);
                    SetPID(p, i, d);
                    break;
            }
        }

        /// <summary>
        /// Enable controlling the motor speed with a PID loop based on the specifed source.
        /// </summary>
        /// <remarks>
        /// After calling this you must call <see cref="EnableControl()"/> or <see cref="EnableControl(double)"/>
        /// to enable the device.
        /// </remarks>
        /// <param name="p">The proportional gain of the Jaguar's PID controller</param>
        /// <param name="i">The integral gain of the Jaguar's PID controller</param>
        /// <param name="d">The derivative gain of the Jaguar's PID controller</param>
        /// <param name="mode">The <see cref="SourceMode"/> to set the controller to.</param>
        /// <param name="codesPerRev">The number of codes per revolution on the encoder or potentiometer.</param>
        public void SetSpeedMode(double p, double i, double d, SourceMode mode = SourceMode.None, int codesPerRev = 0)
        {
            ChangeControlMode(ControlMode.Speed);
            switch (mode)
            {
                case SourceMode.Encoder:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_NONE);
                    EncoderCodesPerRev = codesPerRev;
                    SetPID(p, i, d);
                    break;
                case SourceMode.QuadEncoder:
                    SetPositionReference(LM_REF_ENCODER);
                    SetSpeedReference(LM_REF_QUAD_ENCODER);
                    EncoderCodesPerRev = codesPerRev;
                    SetPID(p, i, d);
                    break;
                case SourceMode.Potentiometer:
                    SetPositionReference(LM_REF_POT);
                    SetSpeedReference(LM_REF_NONE);
                    PotentiometerTurns = 1;
                    SetPID(p, i, d);
                    break;
                default:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_NONE);
                    SetPID(p, i, d);
                    break;
            }
        }

        /// <summary>
        /// Enable controlling the motor voltage with feedback from the specifed mode.
        /// </summary>
        /// <remarks>
        /// After calling this you must call <see cref="EnableControl()"/> or <see cref="EnableControl(double)"/>
        /// to enable the device.
        /// </remarks>
        /// <param name="mode">The <see cref="SourceMode"/> to set the controller to.</param>
        /// <param name="codesPerRev">The number of codes per revolution on the encoder or potentiometer.</param>
        public void SetVoltageMode(SourceMode mode = SourceMode.None, int codesPerRev = 0)
        {
            ChangeControlMode(ControlMode.Voltage);
            switch (mode)
            {
                case SourceMode.Encoder:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_ENCODER);
                    EncoderCodesPerRev = codesPerRev;
                    break;
                case SourceMode.QuadEncoder:
                    SetPositionReference(LM_REF_ENCODER);
                    SetSpeedReference(LM_REF_QUAD_ENCODER);
                    EncoderCodesPerRev = codesPerRev;
                    break;
                case SourceMode.Potentiometer:
                    SetPositionReference(LM_REF_POT);
                    SetSpeedReference(LM_REF_NONE);
                    PotentiometerTurns = 1;
                    break;
                default:
                    SetPositionReference(LM_REF_NONE);
                    SetSpeedReference(LM_REF_NONE);
                    break;
            }
        }

        private void ChangeControlMode(ControlMode controlMode)
        {
            // Disable the previous mode
            DisableControl();

            // Update the local mode
            m_controlMode = controlMode;
            m_controlModeVerified = false;
        }

        /// <inheritdoc/>
        public double GetBusVoltage()
        {
            UpdatePeriodicStatus();

            return m_busVoltage;
        }

        /// <inheritdoc/>
        public double GetOutputVoltage()
        {
            UpdatePeriodicStatus();

            return m_outputVoltage;
        }

        /// <inheritdoc/>
        public double GetOutputCurrent()
        {
            UpdatePeriodicStatus();

            return m_outputCurrent;
        }

        /// <inheritdoc/>
        public double GetTemperature()
        {
            UpdatePeriodicStatus();

            return m_temperature;
        }

        /// <inheritdoc/>
        public double GetPosition()
        {
            UpdatePeriodicStatus();

            return m_position;
        }

        /// <inheritdoc/>
        public double GetSpeed()
        {
            UpdatePeriodicStatus();

            return m_speed;
        }

        /// <inheritdoc/>
        public bool GetForwardLimitOk()
        {
            UpdatePeriodicStatus();

            return (m_limits & (byte)Limits.ForwardLimit) != 0;
        }

        /// <inheritdoc/>
        public bool GetReverseLimitOk()
        {
            UpdatePeriodicStatus();

            return (m_limits & (byte)Limits.ForwardLimit) != 0;
        }

        /// <inheritdoc/>
        public Faults GetFaults()
        {
            UpdatePeriodicStatus();

            return (Faults)m_faults;
        }

        /// <inheritdoc/>
        public double VoltageRampRate
        {
            set
            {
                byte[] data = new byte[8];
                int dataSize;
                int message;

                switch (m_controlMode)
                {
                    case ControlMode.PercentVbus:
                        dataSize = PackPercentage(data, value / (m_maxOutputVoltage * ControllerRate));
                        message = LM_API_VOLT_SET_RAMP;
                        break;
                    case ControlMode.Voltage:
                        dataSize = PackFXP8_8(data, value / ControllerRate);
                        message = LM_API_VCOMP_COMP_RAMP;
                        break;
                    default:
                        throw new InvalidOperationException(
                            "Voltage ramp rate only applies in Percentage and Voltage modes");
                }

                SendMessage(message, data, dataSize);
            }
        }

        /// <inheritdoc/>
        public uint FirmwareVersion => (uint)m_firmwareVersion;

        /// <summary>
        /// Get the version of the Jaguar hardware.
        /// </summary>
        /// <returns>1 for the grey jaguar, 2 for the black jaguar.</returns>
        public byte GetHardwareVersion()
        {
            return m_hardwareVersion;
        }

        /// <inheritdoc/>
        public NeutralMode NeutralMode
        {
            set
            {
                byte[] data = new byte[8];

                data[0] = (byte)value;

                SendMessage(LM_API_CFG_BRAKE_COAST, data, 1);

                m_neutralMode = value;
                m_neutralModeVerified = false;
            }
        }

        /// <inheritdoc/>
        public int EncoderCodesPerRev
        {
            set
            {
                byte[] data = new byte[8];

                int dataSize = PackInt16(data, (short)value);
                SendMessage(LM_API_CFG_ENC_LINES, data, dataSize);

                m_encoderCodesPerRev = (short)value;
                m_encoderCodesPerRevVerified = false;
            }
        }

        /// <inheritdoc/>
        public int PotentiometerTurns
        {
            set
            {
                byte[] data = new byte[8];

                int dataSize = PackInt16(data, (short)value);
                SendMessage(LM_API_CFG_POT_TURNS, data, dataSize);

                m_potentiometerTurns = (short)value;
                m_potentiometerTurnsVerified = false;
            }
        }

        /// <inheritdoc/>
        public void ConfigSoftPositionLimits(double forwardLimitPosition, double reverseLimitPosition)
        {
            LimitMode = LimitMode.SoftPositionLimits;
            ForwardLimit = forwardLimitPosition;
            ReverseLimit = reverseLimitPosition;
        }

        /// <inheritdoc/>
        public void DisableSoftPositionLimits()
        {
            LimitMode = LimitMode.SwitchInputsOnly;
        }

        /// <inheritdoc/>
        public LimitMode LimitMode
        {
            set
            {
                byte[] data = new byte[8];
                data[0] = (byte)value;
                SendMessage(LM_API_CFG_LIMIT_MODE, data, 1);
            }
        }

        /// <inheritdoc/>
        public double ForwardLimit
        {
            set
            {
                byte[] data = new byte[8];

                int dataSize = PackFXP16_16(data, value);
                data[dataSize++] = 1;
                SendMessage(LM_API_CFG_LIMIT_FWD, data, dataSize);

                m_forwardLimit = value;
                m_forwardLimitVerified = false;
            }
        }

        /// <inheritdoc/>
        public double ReverseLimit
        {
            set
            {
                byte[] data = new byte[8];

                int dataSize = PackFXP16_16(data, value);
                data[dataSize++] = 1;
                SendMessage(LM_API_CFG_LIMIT_REV, data, dataSize);

                m_reverseLimit = value;
                m_reverseLimitVerified = false;
            }
        }

        /// <inheritdoc/>
        public double MaxOutputVoltage
        {
            set
            {
                byte[] data = new byte[8];

                int dataSize = PackFXP8_8(data, value);
                SendMessage(LM_API_CFG_MAX_VOUT, data, dataSize);

                m_maxOutputVoltage = value;
                m_maxOutputVoltageVerified = false;
            }
        }

        /// <inheritdoc/>
        public float FaultTime
        {
            set
            {
                byte[] data = new byte[8];

                if (value < 0.5f) value = 0.5f;
                else if (value > 3.0f) value = 3.0f;

                int dataSize = PackInt16(data, (short)(value * 1000.0));
                SendMessage(LM_API_CFG_FAULT_TIME, data, dataSize);

                m_faultTime = value;
                m_faultTimeVerified = false;
            }
        }
    }
}
