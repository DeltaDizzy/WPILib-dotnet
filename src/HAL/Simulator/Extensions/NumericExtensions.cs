﻿using System;
using HAL.Simulator.Data;

namespace HAL.Simulator.Extensions
{
    /// <summary>
    /// Extension methods that are used to easily convert numbers between units.
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">Degrees</param>
        /// <returns>Radians</returns>
        public static double ToRadians(this double degrees)
        {
            return Math.PI / 180 * degrees;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">Radians</param>
        /// <returns>Degrees</returns>
        public static double ToDegrees(this double radians)
        {
            return 180/Math.PI*radians;
        }

        /// <summary>
        /// Meters to Inches.
        /// </summary>
        /// <param name="meters">Meters</param>
        /// <returns>Inches</returns>
        public static double ToInches(this double meters)
        {
            return meters * 39.3701;
        }

        /// <summary>
        /// Inches to Meters.
        /// </summary>
        /// <param name="inches">Inches</param>
        /// <returns>Meters</returns>
        public static double ToMeters(this double inches)
        {
            return inches * 0.0254;
        }

        /// <summary>
        /// Converts Rotations Per Minute to Radians Per Second.
        /// </summary>
        /// <param name="rpms">Rotations Per Minute</param>
        /// <returns>Radians Per Second</returns>
        public static double RpmsToRadiansPerSecond(this double rpms)
        {
            double radiansPerMinute = rpms * 2 * Math.PI;
            return radiansPerMinute / 60.0; 
        }

        /// <summary>
        /// Converts Radians Per Second to Rotations Per Minute
        /// </summary>
        /// <param name="radiansPerSecond">Radians Per Second</param>
        /// <returns>Rotations Per Minute</returns>
        public static double RadiansPerSecondToRpms(this double radiansPerSecond)
        {
            double radiansPerMinute = radiansPerSecond * 60.0;
            return radiansPerMinute / (2 * Math.PI);
        }
    }
}
