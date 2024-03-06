using UnitsNet.NumberExtensions.NumberToRotationalSpeed;
using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIUtil.Serialization.Struct;
using Xunit;

namespace WPIMath.Test.Kinematics.Struct
{
    public class ChassisSpeedsStructTest
    {
        readonly ChassisSpeeds data = new(1.MetersPerSecond(), 2.MetersPerSecond(), 3.RadiansPerSecond());

        [Fact]
        public void TestRoundtrip()
        {
            StructPacker packer = new(new byte[ChassisSpeeds.Struct.Size]);
            ChassisSpeeds.Struct.Pack(ref packer, data);

            StructUnpacker unpacker = new StructUnpacker(packer.Filled);
            ChassisSpeeds unpackedData = ChassisSpeeds.Struct.Unpack(ref unpacker);

            Assert.Equal(data, unpackedData);
        }
    }
}
