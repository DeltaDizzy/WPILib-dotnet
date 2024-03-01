using WPIMath.Geometry;

namespace WPIMath.Kinematics
{
    public interface IKinematics<TSpeeds, TPositions>
    {
        /// <summary>
        /// Performs forward kinematics to return the resulting chassis speed from the wheel speeds. This
        /// method is often used for odometry -- determining the robot's position on the field using data
        /// from the real-world speed of each wheel on the robot
        /// </summary>
        /// <param name="wheelSpeeds">The speeds of the wheels.</param>
        /// <returns>The chassis speed.</returns>
        ChassisSpeeds ToChassisSpeeds(TSpeeds wheelSpeeds);

        /// <summary>
        /// Performs inverse kinematics to return the wheel speeds from a desired chassis velocity. This
        /// method is often used to convert joystick values into wheel speeds.
        /// </summary>
        /// <param name="chassisSpeeds">The desired chassis speed.</param>
        /// <returns>The wheel speeds required to achieve the desired chassis speed.</returns>
        TSpeeds ToWheelSpeeds(ChassisSpeeds chassisSpeeds);

        /// <summary>
        /// Performs forward kinematics to return the resulting Twist2d from the given change in wheel
        /// positions.This method is often used for odometry -- determining the robot's position on the
        /// field using changes in the distance driven by each wheel on the robot.
        /// </summary>
        /// <param name="start">The starting distances driven by the wheels.</param>
        /// <param name="end">The ending distances driven by the wheels.</param>
        /// <returns>The resulting Twist2d.</returns>
        Twist2d ToTwist2d(TPositions start, TPositions end);
    }
}
