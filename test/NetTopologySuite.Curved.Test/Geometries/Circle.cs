using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Test.Geometries
{
    /// <summary>
    /// Utility class for creating and testing <see cref="CircularArc"/>s
    /// </summary>
    internal class Circle
    {
        private const double Tolerance = 1E-10d;

        /// <summary>
        /// Creates a circle around <see cref="Coordinate"/>(0, 0) with the given <paramref name="radius"/>
        /// </summary>
        /// <param name="radius">The radius of the circle</param>
        public Circle(double radius)
            :this(0, 0, radius)
        {}

        /// <summary>
        /// Creates a circle around <see cref="Coordinate"/>(<paramref name="x"/>, <paramref name="y"/>)
        /// with the given <paramref name="radius"/>
        /// </summary>
        /// <param name="x">The x-ordinate value of the <see cref="Center"/></param>
        /// <param name="y">The y-ordinate value of the <see cref="Center"/></param>
        /// <param name="radius">The radius of the circle</param>
        public Circle(double x, double y, double radius)
            : this(new Coordinate(x, y), radius)
        {
        }

        /// <summary>
        /// Creates a circle around <paramref name="center"/> with the given <paramref name="radius"/>
        /// </summary>
        /// <param name="center">The  <see cref="Center"/></param>
        /// <param name="radius">The radius of the circle</param>
        public Circle(Coordinate center, double radius)
        {
            Center = center.Copy();
            Radius = radius;
        }


        /// <summary>
        /// Gets a value indicating the center of the circle
        /// </summary>
        public Coordinate Center { get; }

        /// <summary>
        /// Gets a value indicating the radius of the circle
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Get a circular arc off of this circle
        /// </summary>
        /// <param name="startAngle">The start angle [degrees]</param>
        /// <param name="endAngle">The end angle [degrees]</param>
        /// <returns>A circular arc</returns>
        public CircularArc GetCircularArc(double startAngle, double endAngle)
        {
            double midAngle = startAngle + (endAngle - startAngle) * 0.5;
            return GetCircularArc(startAngle, midAngle, endAngle);
        }

        /// <summary>
        /// Get a circular arc off of this circle
        /// </summary>
        /// <param name="startAngle">The start angle [degrees]</param>
        /// <param name="midAngle">The start angle [degrees]</param>
        /// <param name="endAngle">The end angle [degrees]</param>
        /// <returns>A circular arc</returns>
        public CircularArc GetCircularArc(double startAngle, double midAngle, double endAngle)
        {
            // Normalize arguments
            startAngle = AngleUtility.ToRadians(startAngle);
            midAngle = AngleUtility.ToRadians(midAngle);
            endAngle = AngleUtility.ToRadians(endAngle);

            double minAngle = startAngle <= endAngle ? startAngle : endAngle;
            double maxAngle = startAngle > endAngle ? startAngle : endAngle;

            // Check midAngle
            if (midAngle < minAngle || maxAngle < midAngle)
                throw new ArgumentOutOfRangeException(nameof(midAngle));

            // Full Circle
            if (Math.Abs(AngleUtility.Normalize(startAngle) - AngleUtility.Normalize(endAngle)) < 1E-10)
            {
                var p0 = GetPointAtAngle(0);
                return new CircularArc(p0, Center, p0.Copy());
            }

            // Arc
            var coordinates = new[]
            {
                GetPointAtAngle(startAngle),
                GetPointAtAngle(midAngle),
                GetPointAtAngle(endAngle)
            };

            var sequence = NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory.Create(coordinates);
            return new CircularArc(sequence, 0);
        }

        /// <summary>
        /// Get a point on this circle specified by <paramref name="angle"/>
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Coordinate GetPointAtAngle(double angle)
        {
            return new Coordinate(
                Center.X + Math.Cos(angle) * Radius,
                Center.Y + Math.Sin(angle) * Radius);
        }

        /// <summary>
        /// Predicate to test of a circular arc is part of this circle
        /// </summary>
        /// <param name="arc">The arc</param>
        /// <param name="reason">A reason, why it is not on the circle</param>
        /// <returns><c>true</c> if it is on the circle</returns>
        public bool IsOnCircle(CircularArc arc, out string reason)
        {
            return IsOnCircle(arc, Tolerance, out reason);
        }

        public bool IsOnCircle(CircularArc arc, double tolerance, out string reason)
        {
            reason = null;
            if (arc.Center.Distance(Center) > tolerance)
            {
                reason = "Arc's center does not match this circle's center.";
                return false;
            }

            if (arc.P0.Distance(Center) - Radius > tolerance)
            {
                reason = "CircularArc.P0's distance to this circle's center is bigger than its radius.";
                return false;
            }

            if (arc.P1.Distance(Center) - Radius > tolerance)
            {
                reason = "CircularArc.P0's distance to this circle's center is bigger than its radius.";
                return false;
            }

            if (arc.P2.Distance(Center) - Radius > tolerance)
            {
                reason = "CircularArc.P2's distance to this circle's center is bigger than its radius.";
                return false;
            }

            return true;
        }

        public bool IsOnCircle(IList<Coordinate> arc, double tolerance, out string reason)
        {
            reason = null;
            for (int i = 0; i < arc.Count; i++)
            {
                if (arc[i].Distance(Center) - Radius > tolerance)
                {
                    reason = $"Distance of flattened arc's coordinate {i}' to this circle's center is bigger than its radius.";
                    return false;
                }

            }

            return true;
        }
        public override string ToString()
        {
            return $"Circle[{Center}, radius={Radius}]";
        }
    }
}
