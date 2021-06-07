using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    public class CircularArcTest
    {
        private const double AngleTolerance = 1E-5;
        private const double LengthTolerance = 1E-5;
        private const double DistanceTolerance = 1.4142135623730951d * LengthTolerance;

        [TestCase(0, 10d, 7.0710678118654755d, 7.0710678118654755d, 10d, 0d, 0d, 0d, 10d, -Math.PI / 2d)]
        [TestCase(10d, 0d, 0d, 10d, -10d, 0d, 0d, 0d, 10d, Math.PI)]
        [TestCase(10d, 0d, 0d, 10d, 10d, 0d, 5d, 5d, 7.0710678118654755d,  Math.PI * 2)]
        [TestCase(10d, 1d, 1d, 1d, -10d, 1d, 0, 1d, double.PositiveInfinity, double.NaN)]
        public void TestProperties(double p0X, double p0Y, double p1X, double p1Y, double p2X, double p2Y,
            double cx, double cy, double radius, double angle)
        {
            // Arrange
            var p0 = new Coordinate(p0X, p0Y);
            var p1 = new Coordinate(p1X, p1Y);
            var p2 = new Coordinate(p2X, p2Y);

            // Act
            var ca = new CircularArc(p0, p1, p2);

            // Assert
            var c = new Coordinate(cx, cy);

            Assert.That(ca.Center.Distance(c), Is.LessThan(1e-10));
            Assert.That(ca.Radius, Is.EqualTo(radius).Within(1e-10));
            Assert.That(ca.Angle, Is.EqualTo(angle).Within(1e-10));
            if (double.IsNaN(angle))
            {
                double dx = p2X - p0X;
                double dy = p2Y - p0Y;
                Assert.That(ca.Length, Is.EqualTo(Math.Sqrt(dx*dx+dy*dy)).Within(1e-10));
            }
            else
                Assert.That(ca.Length, Is.EqualTo(Math.Abs(angle * radius)).Within(1e-7));
        }

        [TestCase(0, 10d, 7.0710678118654755d, 7.0710678118654755d, 10d, 0d, 0d, 10000d)]
        [TestCase(0, 10d, 7.0710678118654755d, 7.0710678118654755d, 10d, 0d, 0.25d)]
        public void TestFlatten(double p0X, double p0Y, double p1X, double p1Y, double p2X, double p2Y, double arcSegmentLength, double? scale = null)
        {
            // Arrange
            var pm = scale.HasValue ? new PrecisionModel(scale.Value) : new PrecisionModel();
            var p0 = new Coordinate(p0X, p0Y);
            pm.MakePrecise(p0);
            var p1 = new Coordinate(p1X, p1Y);
            pm.MakePrecise(p1);
            var p2 = new Coordinate(p2X, p2Y);
            pm.MakePrecise(p2);

            // Act
            var ca = new CircularArc(p0, p1, p2);

            var flattened = ca.Flatten(arcSegmentLength);
            var geom = NtsGeometryServices.Instance.CreateGeometryFactory(pm)
                .CreateLineString(flattened.ToCoordinateArray());

            TestContext.WriteLine($"{ca}\n{geom}");

            Assert.That(geom.Length, Is.EqualTo(ca.Length).Within(ca.Length * 0.02));
            Assert.That(geom.IsCoordinate(ca.P0));
            Assert.That(geom.IsCoordinate(ca.P1));
            Assert.That(geom.IsCoordinate(ca.P2));
        }

        [Test]
        public void TestLine()
        {
            
            var ca = new CircularArc(new Coordinate(0, 0), new Coordinate(0, 10), new Coordinate(0, 20));
            Assert.That(ca.Radius, Is.EqualTo(CircularArc.CollinearRadius));
            Assert.That(ca.Angle, Is.EqualTo(CircularArc.CollinearAngle));
            Assert.That(ca.Center, Is.EqualTo(new LineSegment(ca.P0, ca.P2).MidPoint));

            var flattened = CheckFlattened(ca, points: new[] {ca.P0, ca.P1, ca.P2}, length:20d,
                lengthTolerance: 0d, distanceTolerance: 0d);

            CheckEnvelopesMatch(ca.Envelope, GetEnvelope(flattened));
        }

        [Test]
        public void TestIdenticalPoints()
        {
            var center = new Coordinate(1, 1);
            var ca = new CircularArc(new Coordinate(1, 1), new Coordinate(1, 1), new Coordinate(1, 1));
            Assert.That(ca.Radius, Is.EqualTo(0d));
            Assert.That(ca.Angle, Is.EqualTo(0d));
            Assert.That(ca.Center, Is.EqualTo(center));

            var flattened = CheckFlattened(ca, length: 0d, points: new[] {ca.P0, ca.P1, ca.P2}, lengthTolerance: 0d);
            CheckEnvelopesMatch(ca.Envelope, GetEnvelope(flattened));
        }

        [TestCase(50, 30, 1)]
        [TestCase(50, 30, 0.5)]
        [TestCase(50, 30, 0.1)]
        [TestCase(100, 45, 0.05)]
        public void TestTiny(double radius, double angle, double opening)
        {
            var circle = new Circle(11, 12, radius);
            var arc1 = circle.GetCircularArc(angle, angle + opening);
            double length = AngleUtility.NormalizePositive(AngleUtility.ToRadians(opening)) * radius;


            Assert.That(arc1.Center.Distance(circle.Center), Is.LessThanOrEqualTo(25 * DistanceTolerance));
            Assert.That(arc1.Radius, Is.EqualTo(circle.Radius).Within(25 * LengthTolerance));
            Assert.That(arc1.Angle, Is.EqualTo(AngleUtility.ToRadians(opening)).Within(AngleTolerance));
            Assert.That(arc1.Length, Is.EqualTo(length).Within(LengthTolerance));
            Assert.That(circle.IsOnCircle(arc1, DistanceTolerance, out string reason), reason);

            var flattened = CheckFlattened(arc1, length / 10);
            Assert.That(circle.IsOnCircle(flattened, DistanceTolerance, out reason), reason);

            var arc2 = circle.GetCircularArc(angle + opening, angle);
            Assert.That(arc2.Center.Distance(circle.Center), Is.LessThanOrEqualTo(25 * DistanceTolerance));
            Assert.That(arc2.Radius, Is.EqualTo(circle.Radius).Within(25 * LengthTolerance));
            Assert.That(arc2.Angle, Is.EqualTo(AngleUtility.ToRadians(-opening)).Within(AngleTolerance));
            Assert.That(arc1.Length, Is.EqualTo(length).Within(LengthTolerance));
            Assert.That(circle.IsOnCircle(arc2, DistanceTolerance, out reason), reason);

            flattened = CheckFlattened(arc1, length / 10);
            Assert.That(circle.IsOnCircle(flattened, DistanceTolerance, out reason), reason);
        }


        private static CoordinateList CheckFlattened(CircularArc actual, double arcStepLength = 0d, PrecisionModel precisionModel = null,
            int? numPoints = null, double? length = null, Coordinate[] points = null,
            double distanceTolerance = DistanceTolerance, double lengthTolerance = LengthTolerance)
        {
            var res = actual.Flatten(arcStepLength, precisionModel);

            if (points != null && !numPoints.HasValue)
                numPoints = points.Length;

            if (points != null && points.Length != numPoints)
                throw new ArgumentException(nameof(numPoints));

            if (numPoints.HasValue)
                Assert.That(res.Count, Is.EqualTo(numPoints.Value), "Number of points");

            if (points != null)
            {
                for (int i = 0; i < numPoints.Value; i++)
                    Assert.That(res[i].Distance(points[i]), Is.EqualTo(0).Within(distanceTolerance));
            }

            if (length.HasValue)
                Assert.That(GetLength(res), Is.EqualTo(length.Value).Within(lengthTolerance));

            return res;
        }

        private static void CheckEnvelopesMatch(Envelope actual, Envelope expected, double tolerance = DistanceTolerance)
        {
            if (expected.IsNull)
                Assert.That(actual.IsNull, "actual.IsNull");

            Assert.That(actual.MinX, Is.EqualTo(expected.MinX).Within(tolerance), "actual.MinX");
            Assert.That(actual.MaxX, Is.EqualTo(expected.MaxX).Within(tolerance), "actual.MaxX");
            Assert.That(actual.MinY, Is.EqualTo(expected.MinY).Within(tolerance), "actual.MinY");
            Assert.That(actual.MaxY, Is.EqualTo(expected.MaxY).Within(tolerance), "actual.MaxY");
        }

        private static Envelope GetEnvelope(CoordinateList flattened)
        {
            var res = new Envelope();
            if (flattened == null || flattened.Count == 0)
                return res;

            for (int i = 0; i < flattened.Count; i++)
                res.ExpandToInclude(flattened[i]);

            return res;
        }

        private static double GetLength(CoordinateList flattened)
        {
            double res = 0d;
            if (flattened == null || flattened.Count <= 1)
                return res;

            for (int i = 1; i < flattened.Count; i++)
                res += flattened[i - 1].Distance(flattened[i]);

            return res;
        }

    }
}
