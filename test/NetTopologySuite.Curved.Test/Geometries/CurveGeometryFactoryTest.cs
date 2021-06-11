using System.Collections;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    [TestFixtureSource("Services")]
    public class CurveGeometryFactoryTest
    {
        private NtsGeometryServices _services;

        public CurveGeometryFactoryTest(NtsCurveGeometryServices services)
        {
            _services = services;
        }

        [Test]
        public void TestBuildGeometryWithCurves()
        {
            var curves = new Geometry[]
            {
                _services.WKTReader.Read("LINESTRING (10 10, 11 10)"),
                _services.WKTReader.Read("CIRCULARSTRING (10 10, 11 11, 12 10)"),
                _services.WKTReader.Read("COMPOUNDCURVE ((12 10, 11 10), CIRCULARSTRING(11 10, 10 9, 11 8))"),
            };

            var geom = _services.CreateGeometryFactory().BuildGeometry(curves);
            Assert.That(geom, Is.TypeOf<MultiCurve>());

        }

        [Test]
        public void TestBuildGeometryWithSurfaces()
        {
            var curves = new Geometry[]
            {
                _services.WKTReader.Read("POLYGON ((-2 2, 2 2, 2 -2, -2 -2, -2 2))"),
                _services.WKTReader.Read("CURVEPOLYGON (CIRCULARSTRING (-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))"),
            };

            var geom = _services.CreateGeometryFactory().BuildGeometry(curves);
            Assert.That(geom, Is.TypeOf<MultiSurface>());

        }
        public static IEnumerable<NtsCurveGeometryServices> Services
        {
            get
            {
                var pm = new PrecisionModel(10000);
                var cc = new CoordinateEqualityComparer();

                yield return new NtsCurveGeometryServices(CoordinateArraySequenceFactory.Instance, pm, 0, cc, 0d);
                yield return new NtsCurveGeometryServices(DotSpatialAffineCoordinateSequenceFactory.Instance, pm, 0, cc, 0d);
                yield return new NtsCurveGeometryServices(PackedCoordinateSequenceFactory.DoubleFactory, pm, 0, cc, 0d);
            }
        }
    }
}
