using System;
using System.Xml;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    public class CircularArcTest
    {

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
                Assert.That(ca.Length, Is.EqualTo(angle * radius).Within(1e-7));
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
    }
}
