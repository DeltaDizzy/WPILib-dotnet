using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Kinematics;
using WPIUtil.Serialization.Struct;
using Xunit;

namespace WPIMath.Test.Kinematics.Struct
{
    public class DifferentialDriveWheelSpeedsStructTest
    {
        readonly DifferentialDriveWheelSpeeds data = new(1.MetersPerSecond(), 2.MetersPerSecond());

        [Fact]
        public void TestRoundtrip()
        {
            StructPacker packer = new(new byte[DifferentialDriveWheelSpeeds.Struct.Size]);
            DifferentialDriveWheelSpeeds.Struct.Pack(ref packer, data);

            StructUnpacker unpacker = new StructUnpacker(packer.Filled);
            DifferentialDriveWheelSpeeds unpackedData = DifferentialDriveWheelSpeeds.Struct.Unpack(ref unpacker);

            Assert.Equal(data, unpackedData);
        }
    }
}
