using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A segment of a circle
    /// </summary>
    public sealed class CircularArc
    {
        /// <summary>
        /// Radius value that indicates that a CircularString is actually a line
        /// </summary>
        public const double CollinearRadius = double.PositiveInfinity;

        /// <summary>
        /// Angle value that indicates that a CircularString is actually a line
        /// </summary>
        public const double CollinearAngle = double.NaN;

        private static int _defaultQuadrantSegments = 12;

        /// <summary>
        /// Gets or sets a value indicating how many segments approximate the circle per segment.
        /// </summary>
        /// <remarks>The default value is <c>12</c></remarks>.
        public static int DefaultQuadrantSegments
        {
            get => _defaultQuadrantSegments;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be positive");

                _defaultQuadrantSegments = value;
            }
        }

        private readonly int _startOffset;
        private readonly CoordinateSequence _sequence;

        private WeakReference<CoordinateList> _linearized;
        
        private Coordinate _c;
        private double _radius;
        private Envelope _envelope;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="p0">The start-point of the arc</param>
        /// <param name="p1">A point on the arc defining the direction (mid-point)</param>
        /// <param name="p2">The end-point of the arc</param>
        public CircularArc(Coordinate p0, Coordinate p1, Coordinate p2)
            : this(NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory.Create(new[] {p0, p1, p2}), 0)
        {
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="sequence">A sequence containing start-, mid- and end-point</param>
        /// <param name="startOffset">An offset on the sequence defining the start</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="sequence"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="startOffset"/> &lt; <c>0</c> or &gt; <see cref="CoordinateSequence.Count"/> - <c>3</c></exception>
        public CircularArc(CoordinateSequence sequence, int startOffset)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            if (startOffset < 0 || startOffset > sequence.Count - 3)
                throw new ArgumentOutOfRangeException(nameof(startOffset));

            //if (sequence.GetCoordinate(startOffset + 0).Equals(sequence.GetCoordinate(startOffset + 1)) ||
            //    sequence.GetCoordinate(startOffset + 1).Equals(sequence.GetCoordinate(startOffset + 2)))
            //    throw new ArgumentException("Sequence does not define a circular arc", nameof(startOffset));

            _sequence = sequence;
            _startOffset = startOffset;
        }

        /// <summary>
        /// Gets a value indicating the start-point of this circle segment
        /// </summary>
        public Coordinate P0 { get => _sequence.GetCoordinate(_startOffset + 0); }

        /// <summary>
        /// Gets a value indicating the mid-point of this circle segment
        /// </summary>
        public Coordinate P1 { get => _sequence.GetCoordinate(_startOffset + 1); }

        /// <summary>
        /// Gets a value indicating the end-point of this circle segment.
        /// </summary>
        public Coordinate P2 { get => _sequence.GetCoordinate(_startOffset + 2); }
        
        /// <summary>
        /// Gets a value indicating the center of the center of this
        /// </summary>
        public Coordinate Center
        {
            get
            {
                if (_c == null)
                    _c = ComputeCenter();
                return _c.Copy();
            }
        }

        /// <summary>
        /// Gets a value indicating the radius of the circle that this segment is part of.
        /// </summary>
        /// <returns>The radius of a circle</returns>
        public double Radius
        {
            get
            {
                if (_c == null)
                    _c = ComputeCenter();
                return _radius;
            }
        }

        /// <summary>
        /// Gets a value indicating the length of the arc.
        /// </summary>
        public double Length
        {
            get => double.IsPositiveInfinity(Radius)
                ? P0.Distance(P2)
                : Math.Abs(Angle * Radius);
        }

        /// <summary>
        /// Gets a value indicating the opening angle of the circular arc
        /// </summary>
        public double Angle
        {
            get
            {
                if (double.IsPositiveInfinity(Radius))
                    return CollinearAngle;

                double a1 = AngleUtility.AngleBetweenOriented(P0, Center, P1);
                double a2 = AngleUtility.AngleBetweenOriented(P1, Center, P2);
                if (Math.Sign(a2) != Math.Sign(a1)) a2 += -Math.Sign(a2) * AngleUtility.PiTimes2;

                return a1 + a2;
            }
        }

        /// <summary>
        /// Gets a value indicating the extent of this <see cref="CircularArc"/>
        /// </summary>
        public Envelope Envelope
        {
            get => ComputeEnvelope();
        }

        /// <summary>
        /// Create a flattened version of this <see cref="CircularArc"/>
        /// </summary>
        /// <param name="arcStepLength">The maximum length of an arc segment</param>
        /// <param name="precisionModel">A precision model</param>
        /// <returns>A list of coordinates</returns>
        public CoordinateList Flatten(double arcStepLength = 0d, PrecisionModel precisionModel = null)
        {
            if (_linearized != null && _linearized.TryGetTarget(out var coordinateList))
                return coordinateList;

            if (precisionModel == null)
                precisionModel = NtsGeometryServices.Instance.DefaultPrecisionModel;

            // Get local instances of the coordinates
            var p0 = P0;
            var p1 = P1;
            var p2 = P2;

            // If this is arc is collinear, just return the 3 defining points
            if (_radius == double.PositiveInfinity)
            {
                var res = new CoordinateList {p0, p1, p2};
                _linearized = new WeakReference<CoordinateList>(res);
                return res;
            }

            // Compute angles
            var c = ComputeCenter();
            double angleP0 = AngleUtility.Angle(c, p0);
            double angleP1 = AngleUtility.Angle(c, p1);
            double angleP2 = AngleUtility.Angle(c, p2);

            // Check orientation and reorient if clockwise
            bool isClockwise = Orientation.Index(p0, p1, p2) == OrientationIndex.Clockwise;
            if (isClockwise)
            {
                var pTmp = p0;
                p0 = p2;
                p2 = pTmp;
                double angleTmp = angleP0;
                angleP0 = angleP2;
                angleP2 = angleTmp;
            }

            // Normalize angles in ascending order
            if (angleP1 < angleP0)
            {
                angleP1 += AngleUtility.PiTimes2;
                angleP2 += AngleUtility.PiTimes2;
            }
            else if (angleP2 < angleP0)
                angleP2 += AngleUtility.PiTimes2;

            if (arcStepLength == 0d)
                arcStepLength = AngleUtility.PiOver2 * Radius / DefaultQuadrantSegments;

            // Create buffer with p0
            var cl = new CoordinateList {p0};

            double angleStep = arcStepLength / Radius;
            double angle = Math.Ceiling(angleP0 / angleStep) * angleStep;

            // Add points from p0 to p1
            double angleDelta = angleP1 - angleP0;
            while (angle < angleP1)
            {
                var p = _sequence.CreateCoordinate();
                p.X = precisionModel.MakePrecise(c.X + _radius * Math.Cos(angle));
                p.Y = precisionModel.MakePrecise(c.Y + _radius * Math.Sin(angle));
                cl.Add(Interpolate(p, p0, p1, (angle - angleP0) / angleDelta), false);

                // Increase angle
                angle += angleStep;
            }

            // Add middle point
            cl.Add(p1, cl.Count == 1);

            // Add points from p1 to p2
            angleDelta = angleP2 - angleP1;
            while (angle < angleP2)
            {
                var p = _sequence.CreateCoordinate();
                p.X = precisionModel.MakePrecise(c.X + _radius * Math.Cos(angle));
                p.Y = precisionModel.MakePrecise(c.Y + _radius * Math.Sin(angle));
                cl.Add(Interpolate(p, p0, p1, (angle - angleP1) / angleDelta), false);

                // Increase angle
                angle += angleStep;
            }

            // Add end point
            cl.Add(p2, cl.Count == 2);

            if (isClockwise)
                cl.Reverse();

            _linearized = new WeakReference<CoordinateList>(cl);
            return cl;
        }

        /// <summary>
        /// Computes the center of the circle that this segment is part of.
        /// </summary>
        /// <returns>The center point of a circle</returns>
        private Coordinate ComputeCenter()
        {

            if (_c != null)
                return _c;

            var p0 = P0;
            var p1 = P1;
            var p2 = P2;

            var res = _sequence.CreateCoordinate();

            // If p0 and p2 are equal, we have a full circle
            if (p0.Equals2D(p2))
            {
                res.X = p0.X + (p1.X - p0.X) * 0.5d;
                res.Y = p0.Y + (p1.Y - p0.Y) * 0.5d;
                _radius = res.Distance(p0);
                return res;
            }

            // If p0, p1 and p2 are equal, we have a full circle
            if (Orientation.Index(p0, p1, p2) == OrientationIndex.Collinear)
            {
                res.X = p0.X + (p2.X - p0.X) * 0.5d;
                res.Y = p0.Y + (p2.Y - p0.Y) * 0.5d;
                _radius = CollinearRadius;
                return res;
            }

            var p0X = DD.ValueOf(p0.X);
            var p0Y = DD.ValueOf(p0.Y);
            var p1X = DD.ValueOf(p1.X);
            var p1Y = DD.ValueOf(p1.Y);
            var p2X = DD.ValueOf(p2.X);
            var p2Y = DD.ValueOf(p2.Y);

            var tmp = p1X * p1X + p1Y * p1Y;
            //                                      (sx - mx)  *  (my - ey)  -  (mx - ex)  *  (sy - my)
            var determinate = DD.ValueOf(1d) / ((p0X - p1X) * (p1Y - p2Y) - (p1X - p2X) * (p0Y - p1Y));
            var bc = (p0X * p0X + p0Y * p0Y - tmp) / DD.ValueOf(2d);
            var cd = (tmp - p2X * p2X - p2Y * p2Y) / DD.ValueOf(2d);

            res.X = ((bc * (p1Y - p2Y) - cd * (p0Y - p1Y)) * determinate).ToDoubleValue();
            res.Y = (((p0X - p1X) * cd - (p1X - p2X) * bc)  * determinate).ToDoubleValue();
            _radius = p0.Distance(res);

            return res;
        }

        private Envelope ComputeEnvelope()
        {
            if (_envelope != null)
                return _envelope.Copy();

            var p0 = P0;
            var p2 = P2;
            var res = new Envelope(p0);
            res.ExpandToInclude(p2);

            if (_radius == double.PositiveInfinity)
                return res;

            // Compute angles
            var c = ComputeCenter();
            double angleP0 = AngleUtility.Angle(c, p0);
            var p1 = _sequence.GetCoordinate(_startOffset + 1);
            double angleP1 = AngleUtility.Angle(c, p1);
            double angleP2 = AngleUtility.Angle(c, p2);

            // Check orientation and reorient if clockwise
            if (Orientation.Index(p0, p1, p2) == OrientationIndex.Clockwise)
            {
                //var pTmp = p0;
                //p0 = p2;
                //p2 = pTmp;
                double angleTmp = angleP0;
                angleP0 = angleP2;
                angleP2 = angleTmp;
            }

            // Normalize angles in ascending order
            if (angleP1 < angleP0)
            {
                //angleP1 += AngleUtility.PiTimes2;
                angleP2 += AngleUtility.PiTimes2;
            }
            else if (angleP2 < angleP0)
                angleP2 += AngleUtility.PiTimes2;

            // scan the circle at the PI/2 angles 
            double angle = (Math.Floor(angleP0 / AngleUtility.PiOver2) + 1) * AngleUtility.PiOver2;
            while (angle < angleP2)
            {
                double x = c.X + _radius * Math.Cos(angle);
                double y = c.Y + _radius * Math.Sin(angle);
                res.ExpandToInclude(x, y);
                angle += AngleUtility.PiOver2;
            }

            _envelope = res;
            return res.Copy();

        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return $"CircularArc[{P0}, {P1}, {P2}]";
        }

        private static Coordinate Interpolate(Coordinate p, Coordinate p0, Coordinate p1, double v)
        {
            switch (Coordinates.Dimension(p))
            {
                case 2:
                    break;
                case 3:
                    if (Coordinates.Measures(p) == 0)
                        p.Z = p0.Z + v * (p1.Z - p0.Z);
                    else
                        p.M = p0.M + v * (p1.M - p0.M);
                    break;
                case 4:
                    p.Z = p0.Z + v * (p1.Z - p0.Z);
                    p.M = p0.M + v * (p1.M - p0.M);
                    break;
            }

            return p;
        }


    }
}
