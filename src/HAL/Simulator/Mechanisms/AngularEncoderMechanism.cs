﻿using System;
using HAL.Simulator.Inputs;
using HAL.Simulator.Outputs;

namespace HAL.Simulator.Mechanisms
{
    /// <summary>
    /// This class is used to create a simulated arm that is driven by an encoder.
    /// </summary>
    public class AngularEncoderMechanism : FeedbackMechanismBase
    {
        /// <summary>
        /// Gets the current radians of the system offset by the homing location
        /// </summary>
        public double AdjustedRadians { get; protected set; }

        /// <summary>
        /// Gets the current rotations of the system offset by the homing location
        /// </summary>
        public double AdjustedRotations => AdjustedRadians / (Math.PI * 2);

        private double m_offset;

        /// <summary>
        /// Creates a new arm which is driven by an encoder
        /// </summary>
        /// <param name="input">The motor driving the system</param>
        /// <param name="output">The encoder giving feedback to the system</param>
        /// <param name="encoderCPR">The CPR of the encoder. If not a 1:1 ratio on the axle, scale this beforehand</param>
        /// <param name="model">The transmission model to use</param>
        /// <param name="startRotations">The location in rotations you want the system to start at.</param>
        /// <param name="invertInput">Inverts the motor input</param>
        public AngularEncoderMechanism(ISimSpeedController input, SimEncoder output, double encoderCPR, DCMotor model,
            double startRotations, bool invertInput)
        {
            m_input = input;
            //m_output = output;
            m_model = model;
            m_invert = invertInput;

            m_scaler = encoderCPR / (Math.PI * 2);

            CurrentRadians = 0;

            m_offset = startRotations * (Math.PI * 2);

            AdjustedRadians = m_offset;

            m_maxRadians = double.MaxValue;
            m_minRadians = double.MinValue;
        }

        /// <summary>
        /// Updates the system, checking for home and using the offset to get adjusted radians
        /// </summary>
        /// <param name="seconds">The seconds passed in the current iteration</param>
        public override void Update(double seconds)
        {
            base.Update(seconds);
            AdjustedRadians = CurrentRadians + m_offset;
            m_checkHome?.Invoke();
        }

        private Action m_checkHome = null;

        /// <summary>
        /// Sets the homing location for the system
        /// </summary>
        /// <param name="homeInput">The input to use for checking home</param>
        /// <param name="rising">Check on rising edge</param>
        /// <param name="radians">The radian to home at, in relation to starting radians</param>
        /// <param name="threshold">The threshold allowed to be considered in range</param>
        public void SetHomeLocation(SimDigitalInput homeInput, bool rising, double radians, double threshold)
        {
            homeInput.Set(!rising);

            m_checkHome = () =>
            {
                if (AdjustedRadians < radians + threshold && AdjustedRadians > radians - threshold)
                {
                    homeInput.Set(rising);
                }
                else
                {
                    homeInput.Set(!rising);
                }
            };
            /*
            Action<dynamic, dynamic> handler = null;
                handler = (k, v) =>
                {
                    m_offset = AdjustedRadians;
                    CurrentRadians = 0;
                    ((SimEncoder) m_output).EncoderData.Cancel(k, handler);
                    ((SimEncoder) m_output).EncoderData.Reset = false;
                };
                ((SimEncoder) m_output).EncoderData.Register("Reset", handler);
            

    */
        }
    }
}
