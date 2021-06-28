using System;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    [TestFixture(0d, 5E-4)]
    [TestFixture(0.001d, 5E-7)]
    public class MultiCurveImplTest : CurveGeometryImplTest
    {
        public MultiCurveImplTest(double arcSegmentLength, double lengthTolerance)
            : base(arcSegmentLength, lengthTolerance)
        {
        }

        protected override Geometry CreateGeometry()
        {
            return Instance.WKTReader.Read(
                "MULTICURVE ((2 0, 1 1, 0 0), CIRCULARSTRING (0 0, 1 2.1082, 3 6.3246, 0 7, -3 6.3246, -1 2.1082, 0 0), " +
                "EMPTY, COMPOUNDCURVE (CIRCULARSTRING (0 2, 2 0, 4 2), CIRCULARSTRING (4 2, 2 4, 0 2)))");
        }

        public override void TestIsEmpty()
        {

            var mc1 = Factory.CreateMultiCurve();
            var mc2 = Factory.CreateMultiCurve(Array.Empty<Geometry>());
            var mc3 = Instance.WKTReader.Read("MULTICURVE EMPTY");
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
