using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Geometry;

namespace WPIMath.Kinematics
{
    public class DifferentialDriveKinematics(Length trackwidth) : IKinematics<DifferentialDriveWheelSpeeds, DifferentialDriveWheelPositions>
    {
        public Length Trackwidth { get; init; } = trackwidth;

        public ChassisSpeeds ToChassisSpeeds(DifferentialDriveWheelSpeeds wheelSpeeds)
        {
            return new ChassisSpeeds(
                (wheelSpeeds.Left + wheelSpeeds.Right) / 2,
                0.MetersPerSecond(),
                RotationalSpeed.FromRadiansPerSecond((wheelSpeeds.Right.MetersPerSecond - wheelSpeeds.Left.MetersPerSecond) / Trackwidth.Meters));
        }

        public Twist2d ToTwist2d(DifferentialDriveWheelPositions start, DifferentialDriveWheelPositions end)
        {
            var twistLeft = end.LeftDistance - start.LeftDistance;
            var twistRight = end.RightDistance - start.RightDistance;
            return ToTwist2d(twistLeft, twistRight);
        }

        public Twist2d ToTwist2d(Length leftDistance, Length rightDistance)
        {
            return new Twist2d(
                (leftDistance + rightDistance) / 2,
                Length.FromMeters(0),
                Angle.FromRadians((rightDistance - leftDistance) / Trackwidth)
            );
        }

        public DifferentialDriveWheelSpeeds ToWheelSpeeds(ChassisSpeeds chassisSpeeds)
        {
            return new DifferentialDriveWheelSpeeds(
                Speed.FromMetersPerSecond(chassisSpeeds.Vx.MetersPerSecond - Trackwidth.Meters / 2 * chassisSpeeds.Omega.RadiansPerSecond),
                Speed.FromMetersPerSecond(chassisSpeeds.Vx.MetersPerSecond + Trackwidth.Meters / 2 * chassisSpeeds.Omega.RadiansPerSecond)
                );
        }
    }
}
