using UnitsNet;
using WPIMath.Interpolation;

namespace WPIMath.Kinematics
{
    public readonly struct DifferentialDriveWheelPositions(Length leftDistance, Length rightDistance) : IWheelPositions<DifferentialDriveWheelPositions>,
                                                                IInterpolatable<DifferentialDriveWheelPositions>
    {
        public Length LeftDistance { get; } = leftDistance;
        public Length RightDistance { get; } = rightDistance;

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
