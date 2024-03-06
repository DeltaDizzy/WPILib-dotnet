using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Kinematics;
using Xunit;

namespace WPIMath.Test.Kinematics
{
    public class DifferentialDriveWheelSpeedsTest
    {
        [Fact]
        public void testPlus()
        {
            var left = new DifferentialDriveWheelSpeeds(1.MetersPerSecond(), 0.5.MetersPerSecond());
            var right = new DifferentialDriveWheelSpeeds(2.MetersPerSecond(), 1.5.MetersPerSecond());

            var wheelSpeeds = left + right;

            Assert.Equal(3.0, wheelSpeeds.Left.MetersPerSecond);
            Assert.Equal(2.0, wheelSpeeds.Right.MetersPerSecond);
        }

        [Fact]
        void testMinus()
        {
            var left = new DifferentialDriveWheelSpeeds(1.MetersPerSecond(), 0.5.MetersPerSecond());
            var right = new DifferentialDriveWheelSpeeds(2.MetersPerSecond(), 0.5.MetersPerSecond());

            var wheelSpeeds = left - right;

            Assert.Equal(-1.0, wheelSpeeds.Left.MetersPerSecond);
            Assert.Equal(0.0, wheelSpeeds.Right.MetersPerSecond);
        }

        [Fact]
        void testUnaryMinus()
        {
            var wheelSpeeds = -new DifferentialDriveWheelSpeeds(1.0.MetersPerSecond(), 0.5.MetersPerSecond());

            Assert.Equal(-1.0, wheelSpeeds.Left.MetersPerSecond);
            Assert.Equal(-0.5, wheelSpeeds.Right.MetersPerSecond);
        }

        [Fact]
        void testMultiplication()
        {
            var wheelSpeeds = new DifferentialDriveWheelSpeeds(1.0.MetersPerSecond(), 0.5.MetersPerSecond()) * 2.0;

            Assert.Equal(2.0, wheelSpeeds.Left.MetersPerSecond);
            Assert.Equal(1.0, wheelSpeeds.Right.MetersPerSecond);
        }

        [Fact]
        void testDivision()
        {
            var wheelSpeeds = new DifferentialDriveWheelSpeeds(1.0.MetersPerSecond(), 0.5.MetersPerSecond()) / 2.0;

            Assert.Equal(0.5, wheelSpeeds.Left.MetersPerSecond);
            Assert.Equal(0.25, wheelSpeeds.Right.MetersPerSecond);
        }
    }
}
