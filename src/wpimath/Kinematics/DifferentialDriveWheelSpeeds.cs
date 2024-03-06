using System.Numerics;
using Google.Protobuf.Reflection;
using UnitsNet;
using UnitsNet.NumberExtensions.NumberToSpeed;
using WPIMath.Proto;
using WPIUtil.Serialization.Protobuf;
using WPIUtil.Serialization.Struct;

namespace WPIMath.Kinematics
{
    public struct DifferentialDriveWheelSpeeds : IAdditionOperators<DifferentialDriveWheelSpeeds, DifferentialDriveWheelSpeeds, DifferentialDriveWheelSpeeds>,
        ISubtractionOperators<DifferentialDriveWheelSpeeds, DifferentialDriveWheelSpeeds, DifferentialDriveWheelSpeeds>,
        IMultiplyOperators<DifferentialDriveWheelSpeeds, double, DifferentialDriveWheelSpeeds>,
        IDivisionOperators<DifferentialDriveWheelSpeeds, double, DifferentialDriveWheelSpeeds>,
        IUnaryNegationOperators<DifferentialDriveWheelSpeeds, DifferentialDriveWheelSpeeds>,
        IEquatable<DifferentialDriveWheelSpeeds>
    {
        public Speed Left { get; private set; }
        public Speed Right { get; private set; }

        public DifferentialDriveWheelSpeeds() : this(0.MetersPerSecond(), 0.MetersPerSecond())
        {

        }


        public DifferentialDriveWheelSpeeds(Speed leftSpeed, Speed rightSpeed)
        {
            Left = leftSpeed;
            Right = rightSpeed;
        }

        public static IProtobuf<DifferentialDriveWheelSpeeds, ProtobufDifferentialDriveWheelSpeeds> Proto { get; } = new DifferentialDriveWheelSpeedsProto();

        public static IStruct<DifferentialDriveWheelSpeeds> Struct { get; } = new DifferentialDriveWheelSpeedsStruct();

        /// <summary>
        /// Renormalizes the wheel speeds if any either side is above the specified maximum.
        ///
        /// <para>
        /// Sometimes, after inverse kinematics, the requested speed from one or more wheels may be
        /// above the max attainable speed for the driving motor on that wheel.To fix this issue, one can
        /// reduce all the wheel speeds to make sure that all requested module speeds are at-or-below the
        /// absolute threshold, while maintaining the ratio of speeds between wheels.
        /// </para>
        /// </summary>
        /// <param name="maxAttainableSpeed">The absolute max speed that a wheel can reach.</param>
        public void Desaturate(Speed maxAttainableSpeed)
        {
            var currentMaxSpeed = ((IEnumerable<Speed>)[Left.Abs(), Right.Abs()]).Max();

            if (currentMaxSpeed > maxAttainableSpeed)
            {
                Left = Left / currentMaxSpeed * maxAttainableSpeed;
                Right = Right / currentMaxSpeed * maxAttainableSpeed;
            }
        }

        public readonly bool Equals(DifferentialDriveWheelSpeeds other)
        {
            return Left.Equals(other.Left, Speed.FromMetersPerSecond(1E-9))
                && Right.Equals(other.Right, Speed.FromMetersPerSecond(1E-9));
        }

        public static DifferentialDriveWheelSpeeds operator +(DifferentialDriveWheelSpeeds left, DifferentialDriveWheelSpeeds right)
        {
            return new DifferentialDriveWheelSpeeds(left.Left + right.Left, left.Right + right.Right);
        }

        public static DifferentialDriveWheelSpeeds operator -(DifferentialDriveWheelSpeeds left, DifferentialDriveWheelSpeeds right)
        {
            return new DifferentialDriveWheelSpeeds(left.Left - right.Left, left.Right - right.Right);
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is DifferentialDriveWheelSpeeds speeds && Equals(speeds);
        }

        public static bool operator ==(DifferentialDriveWheelSpeeds left, DifferentialDriveWheelSpeeds right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DifferentialDriveWheelSpeeds left, DifferentialDriveWheelSpeeds right)
        {
            return !(left == right);
        }

        public static DifferentialDriveWheelSpeeds operator -(DifferentialDriveWheelSpeeds value)
        {
            return new(-value.Left, -value.Right);
        }

        public static DifferentialDriveWheelSpeeds operator /(DifferentialDriveWheelSpeeds value, double scalar)
        {
            return new(value.Left / scalar, value.Right / scalar);
        }

        public static DifferentialDriveWheelSpeeds operator *(DifferentialDriveWheelSpeeds value, double scalar)
        {
            return new(value.Left * scalar, value.Right * scalar);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Left, Right);
        }
    }

    public class DifferentialDriveWheelSpeedsProto : IProtobuf<DifferentialDriveWheelSpeeds, ProtobufDifferentialDriveWheelSpeeds>
    {
        public MessageDescriptor Descriptor => ProtobufDifferentialDriveWheelSpeeds.Descriptor;

        public ProtobufDifferentialDriveWheelSpeeds CreateMessage() => new();

        public void Pack(ProtobufDifferentialDriveWheelSpeeds msg, DifferentialDriveWheelSpeeds value)
        {
            msg.Left = value.Left.MetersPerSecond;
            msg.Right = value.Right.MetersPerSecond;
        }

        public DifferentialDriveWheelSpeeds Unpack(ProtobufDifferentialDriveWheelSpeeds msg)
        {
            return new DifferentialDriveWheelSpeeds(msg.Left.MetersPerSecond(), msg.Right.MetersPerSecond());
        }
    }

    public class DifferentialDriveWheelSpeedsStruct : IStruct<DifferentialDriveWheelSpeeds>
    {
        public string TypeString => "struct:DifferentialDriveWheelSpeeds";

        public int Size => sizeof(double) * 2;

        public string Schema => "double left;double right";

        public void Pack(ref StructPacker buffer, DifferentialDriveWheelSpeeds value)
        {
            buffer.WriteDouble(value.Left.MetersPerSecond);
            buffer.WriteDouble(value.Right.MetersPerSecond);
        }

        public DifferentialDriveWheelSpeeds Unpack(ref StructUnpacker buffer)
        {
            return new DifferentialDriveWheelSpeeds(buffer.ReadDouble().MetersPerSecond(), buffer.ReadDouble().MetersPerSecond());
        }
    }
}
