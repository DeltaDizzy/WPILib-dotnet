using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpeed;

namespace WPIMath.Kinematics
{
    public struct DifferentialDriveWheelSpeeds
    {
        public Speed Left { get; private set; }
        public Speed Right { get; private set; }

        public DifferentialDriveWheelSpeeds() : this(0.MetersPerSecond(), 0.MetersPerSecond()) {

        }

        public DifferentialDriveWheelSpeeds(Speed leftSpeed, Speed rightSpeed) {
            Left = leftSpeed;
            Right = rightSpeed;
        }

        /// <summary>
        /// Renormalizes the wheel speeds if any either side is above the specified maximum.
        ///
        /// <para>
        /// Sometimes, after inverse kinematics, the requested speed from one or more wheels may be
        /// above the max attainable speed for the driving motor on that wheel.To fix this issue, one can
        /// reduce all the wheel speeds to make sure that all requested module speeds are at-or-below the
        /// absolute threshold, while maintaining the ratio of speeds between wheels.
        /// </para>
        /// </summary>
        /// <param name="maxAttainableSpeed">The absolute max speed that a wheel can reach.</param>
        public void Desaturate(Speed maxAttainableSpeed)
        {
            var currentMaxSpeed = ((IEnumerable<Speed>)[Left.Abs(), Right.Abs()]).Max();

            if (currentMaxSpeed > maxAttainableSpeed)
            {
                Left = Left / currentMaxSpeed * maxAttainableSpeed;
                Right = Right / currentMaxSpeed * maxAttainableSpeed;
            }
        }
    }
}
