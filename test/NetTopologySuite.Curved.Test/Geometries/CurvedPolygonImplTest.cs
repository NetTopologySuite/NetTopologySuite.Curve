using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NetTopologySuite.Test.Geometries
{
    [TestFixture(0.001d, 5E-7)]
    public class CurvedPolygonImplTest : CurvedGeometryImplTest
    {

        public CurvedPolygonImplTest(double arcSegmentLength, double lengthTolerance)
            : base(arcSegmentLength, lengthTolerance)
        {
        }

        protected override Geometry CreateGeometry()
        {
            var circle = new Circle(0, 6, 2);
            var shell = Factory.CreateCompoundCurve(new Geometry[]
            {
                Factory.CreateLineString(new[] {new Coordinate(0, 0), new Coordinate(-2, 6)}),
                Factory.CreateCircularString(new[] {new Coordinate(-2, 6), new Coordinate(0, 8), new Coordinate(2, 6)}),
                Factory.CreateLineString(new[] {new Coordinate(2, 6), new Coordinate(0, 0)}),
            });

            var holes = new Geometry[]
            {
                Factory.CreateCircularString(new[]
                {
                    new Coordinate(1, 6), new Coordinate(0, 7), new Coordinate(-1, 6), new Coordinate(0, 5),
                    new Coordinate(1, 6)
                })
            };

            return Factory.CreateCurvedPolygon(shell, holes);
        }

        [Test]
        public override void TestIsEmpty()
        {
            var cp0 = Factory.CreateCurvedPolygon();
            var cp1 = Factory.CreateCurvedPolygon(null);

            Assert.That(cp0.IsEmpty, Is.True);
            Assert.That(cp1.IsEmpty, Is.True);
        }

        [Test]
        public override void TestIsSimple()
        {
            Assert.Inconclusive();
        }

        [Test]
        public override void TestIsValid()
        {
            Assert.Inconclusive();
        }
    }
}
