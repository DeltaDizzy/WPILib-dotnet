using UnitsNet;
using UnitsNet.NumberExtensions.NumberToLength;
using WPIMath.Geometry;

namespace WPIMath.Kinematics
{
    public class DifferentialDriveOdometry : Odometry<DifferentialDriveWheelSpeeds, DifferentialDriveWheelPositions>
    {
        // we only need forward kinematics, which doesnt require a kinematics object, so lets default it
        /// <summary>
        /// Creates a new DifferentialDriveOdometry.
        /// </summary>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="wheelPositions">The distances of the left and right drive encoders.</param>
        /// <param name="initialPose">The robot starting pose on the field.</param>
        public DifferentialDriveOdometry(Rotation2d gyroAngle, DifferentialDriveWheelPositions wheelPositions, Pose2d initialPose) : base(new DifferentialDriveKinematics(1.Meters()), gyroAngle, wheelPositions, initialPose) { }

        /// <summary>
        /// Creates a new DifferentialDriveOdometry.
        /// <para>
        /// The starting pose is assumed as (0,0) facing +X (0 degrees).
        /// </para>
        /// </summary>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="wheelPositions">The distances of the left and right drive encoders.</param>
        public DifferentialDriveOdometry(Rotation2d gyroAngle, DifferentialDriveWheelPositions wheelPositions) : this(gyroAngle, wheelPositions, new Pose2d()) { }

        public override void ResetPosition(Rotation2d gyroAngle, DifferentialDriveWheelPositions wheelPositions, Pose2d newPose) => base.ResetPosition(gyroAngle, wheelPositions, newPose);

        /// <summary>
        /// Resets the robot's position on the field.
        /// 
        /// <para>
        /// The gyroscope angle does not need to be reset here in the user's robot code. The library
        /// automatically takes care of offsetting the gyro angle.
        /// </para>
        /// </summary>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="leftDistance">The distance reading of the left side drive encoder.</param>
        /// <param name="rightDistance">The distance reading of the right side drive encoder.</param>
        /// <param name="newPose">The new robot pose.</param>
        public void ResetPosition(Rotation2d gyroAngle, Length leftDistance, Length rightDistance, Pose2d newPose) => base.ResetPosition(gyroAngle, new DifferentialDriveWheelPositions(leftDistance, rightDistance), newPose);

        public override Pose2d Update(Rotation2d gyroAngle, DifferentialDriveWheelPositions wheelPositions) => base.Update(gyroAngle, wheelPositions);

        /// <summary>
        /// Updates the robot's position on the field using forward kinematics and integration of the pose
        /// over time.This method takes in an angle parameter which is used instead of the angular rate
        /// that is calculated from forward kinematics, in addition to the current distance measurement at
        /// each wheel.
        /// </summary>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="leftDistance">The distance reading of the left side drive encoder.</param>
        /// <param name="rightDistance">The distance reading of the right side drive encoder.</param>
        /// <returns>The robot's new pose.</returns>
        public Pose2d Update(Rotation2d gyroAngle, Length leftDistance, Length rightDistance) => Update(gyroAngle, new DifferentialDriveWheelPositions(leftDistance, rightDistance));
    }
}
