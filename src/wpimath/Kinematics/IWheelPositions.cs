namespace WPIMath.Kinematics
{
    public interface IWheelPositions<T> where T : IWheelPositions<T>
    {
        /// <summary>
        /// Returns a copy of this instance.
        /// </summary>
        /// <returns>A copy.</returns>
        T Copy();
    }
}
