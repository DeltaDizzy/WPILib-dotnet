﻿using System;
using HAL.Simulator.Extensions;
using HAL.Simulator.Inputs;
using HAL.Simulator.Outputs;

namespace HAL.Simulator.Mechanisms
{
    /// <summary>
    /// Mechanism for simulating a shooter wheel.
    /// </summary>
    /// <remarks>
    /// This class can be used to simulate a shooter wheel. It has been tested mainly using a Bang-Bang control scheme. 
    /// In order to use this you will need a model for your motor. In addition, you will need to know the inertia of your
    /// system, and the drag that your system has on it.
    /// <para/>
    /// The drag realistically can only be found experimentally. To find this, spin the wheel up to a known speed, and start
    /// graphing velocity over time. Velocity should be in Radians Per Second, however it can be in RPMs, which can be converted
    /// later. Once you are ready, turn off the motor while graphing. Stop graphing when the velocity hits zero. The acceleration is
    /// the slope of this graph, which should be fairly linear. Make sure this number is in Radians Per Second Squared, 
    /// and make sure to keep the negative. A good estimate for this number should be around -80 Radians Per Second Squared.
    /// <para/>
    /// The system inertia can be found using one of two methods. The first is experimentally. The second is to calculate the inertial manually.
    /// For reference, this should be around 0.003 if using an unweighted 6 inch wheel.
    /// </remarks>
    public class ShooterWheelMechanism
    {
        /// <summary>
        /// The motor driving the system.
        /// </summary>
        protected readonly ISimSpeedController m_input;
        /// <summary>
        /// The feedback sensor for the system.
        /// </summary>
        protected readonly IServoFeedback m_output;
        /// <summary>
        /// The model of the motor and transmission for the system.
        /// </summary>
        protected readonly DCMotor m_model;

        /// <summary>
        /// True if the input is inverted for your code.
        /// </summary>
        protected readonly bool m_invert;
        /// <summary>
        /// The minimum velocity allowed for your shooter wheel. (radians per second)
        /// </summary>
        protected readonly double m_minimumVelocity;

        /// <summary>
        /// The sensor CPR.
        /// </summary>
        protected readonly double m_cpr;

        /// <summary>
        /// Gets or sets the deacceleration constant.
        /// </summary>
        /// <value>
        /// The deacceleration constant.
        /// </value>
        public double DeaccelerationConstant { get; set; }

        /// <summary>
        /// Gets or sets the system inertia.
        /// </summary>
        /// <value>
        /// The system inertia.
        /// </value>
        public double SystemInertia { get; set; }

        /// <summary>
        /// Gets or sets the current radians per second.
        /// </summary>
        /// <value>
        /// The current radians per second.
        /// </value>
        public double CurrentRadiansPerSecond { get; protected set; }

        /// <summary>
        /// Gets the current rotations per minute.
        /// </summary>
        public double CurrentRotationsPerMinute => CurrentRadiansPerSecond.RadiansPerSecondToRpms();

        /// <summary>
        /// Gets or sets the deadzone.
        /// </summary>
        /// <value>
        /// The deadzone.
        /// </value>
        public double Deadzone { get; set; } = 0.001;


        /// <summary>
        /// Initializes a new instance of the <see cref="ShooterWheelMechanism"/> class.
        /// </summary>
        /// <param name="input">The speed controller being used.</param>
        /// <param name="output">The feedback output.</param>
        /// <param name="model">The motor model.</param>
        /// <param name="invertInput">True if the input is inverted.</param>
        /// <param name="minimumVelocity">The minimum velocity in Rotations Per Minute</param>
        /// <param name="deaccelConstant">The deaccel constant in Rotations Per Minute</param>
        /// <param name="systemInertia">The system inertia in kg*m^2.</param>
        /// <param name="cpr">The counts per rotation of the sensor.</param>
        /// <exception cref="ArgumentOutOfRangeException">Shooter Wheels do not support analog inputs</exception>
        public ShooterWheelMechanism(ISimSpeedController input, IServoFeedback output, DCMotor model,
            bool invertInput, double minimumVelocity, double deaccelConstant, double systemInertia, double cpr)
        {
            if (output is SimAnalogInput)
            {
                throw new ArgumentOutOfRangeException(nameof(output), "Shooter Wheels do not support analog inputs");
            }
            m_input = input;
            m_output = output;
            m_model = model;
            m_invert = invertInput;
            m_minimumVelocity = minimumVelocity.RpmsToRadiansPerSecond();
            DeaccelerationConstant = deaccelConstant.RpmsToRadiansPerSecond();
            SystemInertia = systemInertia;
            m_cpr = cpr;
        }

        /// <summary>
        /// Limits the specified PWM value.
        /// </summary>
        /// <param name="pwmValue">The PWM value.</param>
        /// <returns></returns>
        public double Limit(double pwmValue)
        {
            if (pwmValue < Deadzone && pwmValue > -Deadzone)
            {
                pwmValue = 0.0;
            }

            if (pwmValue > 1.0)
            {
                pwmValue = 1.0;
            }

            if (pwmValue < -1.0)
            {
                pwmValue = -1.0;
            }

            return pwmValue;
        }


        /// <summary>
        /// Updates the mechanism with the specified delta time
        /// </summary>
        /// <param name="seconds">The delta time in seconds.</param>
        public virtual void Update(double seconds)
        {
            double pwmValue = m_invert ? -m_input.Get() : m_input.Get();
            pwmValue = Limit(pwmValue);

            double percentFreeSpeed = CurrentRadiansPerSecond / m_model.MaxSpeed;
            double currentTorque = (1 - percentFreeSpeed) * m_model.MaxTorque;

            double alpha = currentTorque / SystemInertia;
            alpha *= pwmValue;
            alpha += DeaccelerationConstant;

            double delta = alpha * seconds;

            CurrentRadiansPerSecond += delta;

            if (CurrentRadiansPerSecond > m_model.MaxSpeed)
                CurrentRadiansPerSecond = m_model.MaxSpeed;
            else if (CurrentRadiansPerSecond < m_minimumVelocity)
                CurrentRadiansPerSecond = m_minimumVelocity;

            m_output.SetRate(CurrentRadiansPerSecond.RadiansPerSecondToRpms() / m_cpr);
        }
    }
}
