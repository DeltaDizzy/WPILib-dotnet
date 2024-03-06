using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Kinematics;
using WPIMath.Proto;
using Xunit;

namespace WPIMath.Test.Kinematics.Proto
{
    public class DifferentialDriveWheelSpeedsProtoTest
    {
        readonly DifferentialDriveWheelSpeeds data = new(1.MetersPerSecond(), 2.MetersPerSecond());

        [Fact]
        public void TestRoundtrip()
        {
            ProtobufDifferentialDriveWheelSpeeds proto = DifferentialDriveWheelSpeeds.Proto.CreateMessage();
            DifferentialDriveWheelSpeeds.Proto.Pack(proto, data);
            DifferentialDriveWheelSpeeds unpacked = DifferentialDriveWheelSpeeds.Proto.Unpack(proto);

            Assert.Equal(data.Left, unpacked.Left);
            Assert.Equal(data.Right, unpacked.Right);
        }
    }
}
