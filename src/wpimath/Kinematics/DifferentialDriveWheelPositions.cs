using UnitsNet;
using UnitsNet.NumberExtensions.NumberToLength;
using WPIMath.Interpolation;

namespace WPIMath.Kinematics
{
    public readonly struct DifferentialDriveWheelPositions : IWheelPositions<DifferentialDriveWheelPositions>,
                                                                IInterpolatable<DifferentialDriveWheelPositions>
    {
        public Length LeftDistance { get; }
        public Length RightDistance { get; }

        public DifferentialDriveWheelPositions() : this(0.Meters(), 0.Meters())
        {

        }

        public DifferentialDriveWheelPositions(Length leftDistance, Length rightDistance) 
        {
            LeftDistance = leftDistance;
            RightDistance = rightDistance;
        }

        public DifferentialDriveWheelPositions Copy()
        {
            return new(LeftDistance, RightDistance);
        }

        public DifferentialDriveWheelPositions Interpolate(DifferentialDriveWheelPositions endValue, double t)
        {
            return new(MathExtras.Lerp(LeftDistance, endValue.LeftDistance, t), MathExtras.Lerp(RightDistance, endValue.RightDistance, t));
        }
    }
}
