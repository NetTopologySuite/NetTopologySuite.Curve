using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Test.Geometries
{
    public class CompoundCurveTest : CurvedGeometryTest
    {
        [Test]
        public void TestEmpty()
        {
            var geoms = Array.Empty<Geometry>();

            var cc1 = Factory.CreateCompoundCurve();
            var cc2 = Factory.CreateCompoundCurve(geoms);

            Assert.That(cc1.IsEmpty);
            Assert.That(cc2.IsEmpty);
        }

        [Test]
        public void TestQuestionMark()
        {
            // Arrange
            var ca = new Circle(0, 6, 2).GetCircularArc(110, 60, 270);
            var cs = Factory.CreateCircularString(new[] {ca.P0, ca.P1, ca.P2});
            var ls = Factory.CreateLineString(new[] {new Coordinate(0, 4), new Coordinate(0, 2)});

            // Act
            CompoundCurve cc = null;
            Assert.That(() => cc = Factory.CreateCompoundCurve(new Geometry[] {cs, ls}), Throws.Nothing);

            Assert.That(cc, Is.Not.Null);
            Assert.That(cc.IsCoordinate(cs.ControlPoints.GetCoordinate(0)));
            Assert.That(cc.IsCoordinate(cs.ControlPoints.GetCoordinate(1)));
            Assert.That(cc.IsCoordinate(cs.ControlPoints.GetCoordinate(2)));
            Assert.That(cc.IsCoordinate(ls.CoordinateSequence.GetCoordinate(0)));
            Assert.That(cc.IsCoordinate(ls.CoordinateSequence.GetCoordinate(1)));

            Assert.That(cc.Length, Is.EqualTo(cs.Length + ls.Length).Within(5E-7));
            Assert.That(cc.Flatten().Length, Is.EqualTo(cs.Length + ls.Length).Within(5E-7));
        }
    }
}
