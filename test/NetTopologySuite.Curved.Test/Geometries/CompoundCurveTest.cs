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
    }
}
