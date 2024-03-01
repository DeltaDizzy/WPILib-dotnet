using WPIMath.Geometry;

namespace WPIMath.Kinematics
{
    /// <summary>
    /// Class for odometry. Robot code should not use this directly- Instead, use the particular type for
    /// your drivetrain(e.g., <see cref="DifferentialDriveOdometry"/>). Odometry allows you to track the
    /// robot's position on the field over the course of a match using readings from encoders and a
    /// gyroscope.
    /// 
    /// <para>
    /// Teams can use odometry during the autonomous period for complex tasks like path following.
    /// Furthermore, odometry can be used for latency compensation when using computer-vision systems.
    /// </para>
    /// </summary>
    /// <typeparam name="TSpeeds">Wheel speeds type</typeparam>
    /// <typeparam name="TPositions">Wheel positions type</typeparam>
    public class Odometry<TSpeeds, TPositions> where TPositions : IWheelPositions<TPositions>
    {
        private IKinematics<TSpeeds, TPositions> Kinematics { get; }
        public Pose2d Pose { get; set; }
        private Rotation2d gyroOffset { get; set; }
        private Rotation2d previousAngle { get; set; }
        private TPositions previousWheelPositions { get; set; }

        /// <summary>
        /// Constructs an Odometry object.
        /// </summary>
        /// <param name="kinematics">The drivebase kinematics object.</param>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="wheelPositions">The current wheel encoder distance readings.</param>
        /// <param name="initialPose">The starting pose of the robot on the field.</param>
        public Odometry(IKinematics<TSpeeds, TPositions> kinematics,
                        Rotation2d gyroAngle, TPositions wheelPositions,
                        Pose2d initialPose)
        {
            Kinematics = kinematics;
            Pose = initialPose;
            gyroOffset = initialPose.Rotation - gyroAngle;
            previousAngle = initialPose.Rotation;
            previousWheelPositions = wheelPositions.Copy();
        }

        /// <summary>
        /// Resets the robot's position on the field.
        /// 
        /// <para>
        /// The gyroscope angle does not need to be reset here in the user's robot code. The library
        /// automatically takes care of offsetting the gyro angle.
        /// </para>
        /// </summary>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="wheelPositions">The current wheel encoder distance readings.</param>
        /// <param name="newPose">The new robot pose.</param>
        public virtual void ResetPosition(Rotation2d gyroAngle, TPositions wheelPositions, Pose2d newPose)
        {
            Pose = newPose;
            previousAngle = newPose.Rotation;
            gyroOffset = newPose.Rotation - gyroAngle;
            previousWheelPositions = wheelPositions.Copy();
        }

        /// <summary>
        /// Updates the robot's position on the field using forward kinematics and integration of the pose
        /// over time.This method takes in an angle parameter which is used instead of the angular rate
        /// that is calculated from forward kinematics, in addition to the current distance measurement at
        /// each wheel.
        /// </summary>
        /// <param name="gyroAngle">The angle reported by the gyroscope or IMU.</param>
        /// <param name="wheelPositions">The current wheel encoder distance readings.</param>
        /// <returns>The robot's new pose.</returns>
        public virtual Pose2d Update(Rotation2d gyroAngle, TPositions wheelPositions)
        {
            Rotation2d trueAngle = gyroAngle + gyroOffset;
            Twist2d kinematicsTwist = Kinematics.ToTwist2d(previousWheelPositions, wheelPositions);
            var realTwist = new Twist2d(kinematicsTwist.Dx, kinematicsTwist.Dy, (trueAngle - previousAngle).Angle);
            var newPose = Pose.Exp(realTwist);

            previousWheelPositions = wheelPositions.Copy();
            previousAngle = gyroAngle;
            Pose = new Pose2d(newPose.Translation, trueAngle);
            return Pose;
        }
    }
}
