using System;
using System.Xml;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    public class CircularArcTest
    {

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

            Assert.That(ca.Center, Is.EqualTo(c));
            Assert.That(ca.Radius, Is.EqualTo(radius));
            Assert.That(ca.Angle, Is.EqualTo(angle));
            if (double.IsNaN(angle))
            {
                double dx = p2X - p0X;
                double dy = p2Y - p0Y;
                Assert.That(ca.Length, Is.EqualTo(Math.Sqrt(dx*dx+dy*dy)).Within(1e-7));
            }
            else
                Assert.That(ca.Length, Is.EqualTo(angle * radius).Within(1e-7));
        }
    }
}
