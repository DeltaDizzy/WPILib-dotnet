using UnitsNet.NumberExtensions.NumberToRotationalSpeed;
using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Proto;
using Xunit;

namespace WPIMath.Test.Kinematics.Proto
{
    public class ChassisSpeedsProtoTest
    {
        readonly ChassisSpeeds data = new(1.MetersPerSecond(), 2.MetersPerSecond(), 3.RadiansPerSecond());

        [Fact]
        public void TestRoundtrip()
        {
            ProtobufChassisSpeeds proto = ChassisSpeeds.Proto.CreateMessage();
            ChassisSpeeds.Proto.Pack(proto, data);
            ChassisSpeeds unpacked = ChassisSpeeds.Proto.Unpack(proto);

            Assert.Equal(data.Vx, unpacked.Vx);
            Assert.Equal(data.Vy, unpacked.Vy);
            Assert.Equal(data.Omega, unpacked.Omega);
        }
    }
}
