﻿using System;

namespace WPILib.Geometry
{
    public readonly struct Transform2d : IEquatable<Transform2d>
    {
        public Translation2d Translation { get; }
        public Rotation2d Rotation { get; }

        public Transform2d(in Pose2d initial, in Pose2d final)
        {
            throw new NotImplementedException();
        }

        public Transform2d(in Translation2d translation, in Rotation2d rotation)
        {
            Translation = translation;
            Rotation = rotation;
        }

        public static Transform2d operator *(in Transform2d obj, double scalar)
        {
            return new Transform2d(obj.Translation * scalar, obj.Rotation * scalar);
        }

        public static bool operator ==(in Transform2d left, in Transform2d right)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(in Transform2d left, in Transform2d right)
        {
            return !(left == right);
        }

        public override bool Equals(object? other)
        {
            return other is Transform2d t && this == t;
        }


        public bool Equals(Transform2d other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Translation, Rotation);
        }
    }
}
