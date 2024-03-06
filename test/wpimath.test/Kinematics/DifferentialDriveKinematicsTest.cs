using UnitsNet;
using UnitsNet.NumberExtensions.NumberToRotationalSpeed;
using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Kinematics;
using Xunit;

namespace WPIMath.Test.Kinematics;

public class DifferentialDriveKinematicsTest
{
    readonly DifferentialDriveKinematics kinematics = new(Length.FromMeters(0.381 * 2));
    readonly double epsilon = 1E-9;

    [Fact]
    public void TestInverseKinematicsAtZero()
    {
        ChassisSpeeds chassisSpeeds = new();
        DifferentialDriveWheelSpeeds wheelSpeeds = kinematics.ToWheelSpeeds(chassisSpeeds);

        Assert.Equal(0.0, wheelSpeeds.Left.MetersPerSecond, epsilon);
        Assert.Equal(0.0, wheelSpeeds.Right.MetersPerSecond, epsilon);
    }

    [Fact]
    public void TestForwardKinematicsAtZero()
    {
        DifferentialDriveWheelSpeeds wheelSpeeds = new();
        ChassisSpeeds chassisSpeeds = kinematics.ToChassisSpeeds(wheelSpeeds);

        Assert.Equal(0.0, chassisSpeeds.Vx.MetersPerSecond, epsilon);
        Assert.Equal(0.0, chassisSpeeds.Vy.MetersPerSecond, epsilon);
        Assert.Equal(0.0, chassisSpeeds.Omega.RadiansPerSecond, epsilon);
    }

    [Fact]
    public void TestInverseKinematicsForStraightLine()
    {
        ChassisSpeeds chassisSpeeds = new(3.MetersPerSecond(), 3.MetersPerSecond(), 0.RadiansPerSecond());
        var wheelSpeeds = kinematics.ToWheelSpeeds(chassisSpeeds);

        Assert.Equal(3.0, wheelSpeeds.Left.MetersPerSecond, epsilon);
        Assert.Equal(3.0, wheelSpeeds.Right.MetersPerSecond, epsilon);
    }

    [Fact]
    public void TestForwardKinematicsForStraightLine()
    {
        DifferentialDriveWheelSpeeds wheelSpeeds = new(3.MetersPerSecond(), 3.MetersPerSecond());
        ChassisSpeeds chassisSpeeds = kinematics.ToChassisSpeeds(wheelSpeeds);

        Assert.Equal(3.0, chassisSpeeds.Vx.MetersPerSecond, epsilon);
        Assert.Equal(0.0, chassisSpeeds.Vy.MetersPerSecond, epsilon);
        Assert.Equal(0.0, chassisSpeeds.Omega.RadiansPerSecond, epsilon);
    }

    [Fact]
    public void TestInverseKinematicsForRotateInPlace()
    {
        ChassisSpeeds chassisSpeeds = new(0.MetersPerSecond(), 0.MetersPerSecond(), Math.PI.RadiansPerSecond());
        var wheelSpeeds = kinematics.ToWheelSpeeds(chassisSpeeds);

        Assert.Equal(-0.381 * Math.PI, wheelSpeeds.Left.MetersPerSecond, epsilon);
        Assert.Equal(0.381 * Math.PI, wheelSpeeds.Right.MetersPerSecond, epsilon);
    }

    [Fact]
    public void TestForwardKinematicsForRotateInPlace()
    {
        DifferentialDriveWheelSpeeds wheelSpeeds = new((0.381 * Math.PI).MetersPerSecond(), (-0.381 * Math.PI).MetersPerSecond());
        ChassisSpeeds chassisSpeeds = kinematics.ToChassisSpeeds(wheelSpeeds);

        Assert.Equal(0.0, chassisSpeeds.Vx.MetersPerSecond, epsilon);
        Assert.Equal(0.0, chassisSpeeds.Vy.MetersPerSecond, epsilon);
        Assert.Equal(-Math.PI, chassisSpeeds.Omega.RadiansPerSecond, epsilon);
    }
}
