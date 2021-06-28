using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    [TestFixture(0d, 5E-4)]
    [TestFixture(0.001d, 5E-7)]
    public class MultiSurfaceImplTest : CurveGeometryImplTest
    {
        public MultiSurfaceImplTest(double arcSegmentLength, double lengthTolerance)
            : base(arcSegmentLength, lengthTolerance)
        {
        }

        protected override Geometry CreateGeometry()
        {
            return Instance.WKTReader.Read(
                "MULTISURFACE (((0 0, 10 0, 10 10, 0 10, 0 0), (1 1, 1 2, 2 1, 1 1)), CURVEPOLYGON (CIRCULARSTRING (0 5, 5 0, 0 -5, -5 0, 0 5), (-2 2, 2 2, 2 -2, -2 -2, -2 2)), EMPTY)");
        }

        public override void TestIsEmpty()
        {

            var mc1 = Factory.CreateMultiCurve();
            var mc2 = Factory.CreateMultiCurve(Array.Empty<Geometry>());
            var mc3 = Instance.WKTReader.Read("MULTISURFACE EMPTY");
            Assert.That(mc1.IsEmpty);
            Assert.That(mc2.IsEmpty);
            Assert.That(mc3.IsEmpty);
        }

        public override void TestIsSimple()
        {
            Assert.Inconclusive();
        }

        public override void TestIsValid()
        {
            Assert.Inconclusive();
        }
    }
}
