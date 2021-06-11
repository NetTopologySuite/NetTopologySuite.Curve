using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Test.IO
{
    public class WKBReadWriteTest
    {
        private readonly NtsCurveGeometryServices _instance;

        public WKBReadWriteTest()
        {
            _instance = new NtsCurveGeometryServices(
                CoordinateArraySequenceFactory.Instance, new PrecisionModel(10000), 0, new CoordinateEqualityComparer(), 0);
            _instance.WKTWriter.OutputOrdinates = Ordinates.AllOrdinates;
        }

        /*
           The following WKT definitions were taken and adapted from
           * https://docs.microsoft.com/en-us/sql/relational-databases/spatial/circularstring
           * https://docs.microsoft.com/en-us/sql/relational-databases/spatial/compoundcurve
           * https://docs.microsoft.com/en-us/sql/relational-databases/spatial/curvepolygon
         */
        // CircularString
        [TestCase("CIRCULARSTRING EMPTY")]
        [TestCase("CIRCULARSTRING (2 0, 1 1, 0 0)")]
        [TestCase("CIRCULARSTRING (2 1, 1 2, 0 1, 1 0, 2 1)")]
        [TestCase("CIRCULARSTRING (0 0, 1 2.1082, 3 6.3246, 0 7, -3 6.3246, -1 2.1082, 0 0)")]
        [TestCase("CIRCULARSTRING (-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653)")]
        [TestCase("CIRCULARSTRING (0 0, 1 2, 2 4)")]
        //
        // CompoundCurve
        [TestCase("COMPOUNDCURVE EMPTY")]
        [TestCase("COMPOUNDCURVE ((2 2, 0 0), CIRCULARSTRING (0 0, 1 2.1082, 3 6.3246, 0 7, -3 6.3246, -1 2.1082, 0 0))")]
        [TestCase("COMPOUNDCURVE (CIRCULARSTRING (1 0, 0 1, -1 0), (-1 0, 2 0))")]
        [TestCase("COMPOUNDCURVE (CIRCULARSTRING (-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))")]
        [TestCase("COMPOUNDCURVE ((1 1, 1 3), (1 3, 3 3), (3 3, 3 1), (3 1, 1 1))")]
        [TestCase("COMPOUNDCURVE ((1 1, 1 3, 3 3, 3 1, 1 1))")]
        [TestCase("COMPOUNDCURVE (CIRCULARSTRING (0 2, 2 0, 4 2), CIRCULARSTRING (4 2, 2 4, 0 2))")]
        [TestCase("COMPOUNDCURVE ((3 5, 3 3), CIRCULARSTRING (3 3, 5 1, 7 3), (7 3, 7 5), CIRCULARSTRING (7 5, 5 7, 3 5))")]
        [TestCase("COMPOUNDCURVE ZM(CIRCULARSTRING (7 5 4 2, 5 7 4 2, 3 5 4 2), (3 5 4 2, 8 7 4 2))")]
        [TestCase("COMPOUNDCURVE (CIRCULARSTRING EMPTY)", "System.ArgumentException: Contains null or empty geometry! (Parameter 'linealGeometries')")]
        [TestCase("COMPOUNDCURVE (CIRCULARSTRING (1 0, 0 1, -1 0), (1 0, 2 0))", "System.ArgumentException: Geometries are not in a sequence (Parameter 'linealGeometries')")]
        //
        // CurvePolygon
        [TestCase("CURVEPOLYGON EMPTY")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (2 4, 4 2, 6 4, 4 6, 2 4))")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (-122.358 47.653, -122.348 47.649, -122.348 47.658, -122.358 47.658, -122.358 47.653))")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (0 4, 4 0, 8 4, 4 8, 0 4), CIRCULARSTRING (2 4, 4 2, 6 4, 4 6, 2 4))")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (0 5, 5 0, 0 -5, -5 0, 0 5), (-2 2, 2 2, 2 -2, -2 -2, -2 2))")]
        [TestCase("CURVEPOLYGON (CIRCULARSTRING (0 5, 5 0, 0 -5, -5 0, 0 5), (0 5, 5 0, 0 -5, -5 0, 0 5))")]
        [TestCase("CURVEPOLYGON ((0 5, 0 0, 0 0, 0 0))", "123")]
        //[TestCase("CURVEPOLYGON ((0 0, 0 0, 0 0))", "123")]
        //
        // MultiCurve
        [TestCase("MULTICURVE EMPTY")]
        [TestCase("MULTICURVE ((2 0, 1 1, 0 0), CIRCULARSTRING (0 0, 1 2.1082, 3 6.3246, 0 7, -3 6.3246, -1 2.1082, 0 0), EMPTY, COMPOUNDCURVE (CIRCULARSTRING (0 2, 2 0, 4 2), CIRCULARSTRING (4 2, 2 4, 0 2)))")]
        //
        // MultiSurface
        [TestCase("MULTISURFACE EMPTY")]
        [TestCase("MULTISURFACE (((0 0, 10 0, 10 10, 0 10, 0 0), (1 1, 1 2, 2 1, 1 1)), CURVEPOLYGON (CIRCULARSTRING (0 5, 5 0, 0 -5, -5 0, 0 5), (-2 2, 2 2, 2 -2, -2 -2, -2 2)))")]
        public void Test(string wkt, string exceptionText = null)
        {
            Geometry geom = null;

            if (!string.IsNullOrWhiteSpace(exceptionText))
            {
                Assert.That(() => geom = _instance.WKTReader.Read(wkt), Throws.Exception);
                return;
            }

            Assert.That(() => geom = _instance.WKTReader.Read(wkt), Throws.Nothing);
            Assert.That(geom, Is.Not.Null);

            byte[] wkb = null;
            Assert.That(() => wkb = _instance.WKBWriter.Write(geom), Throws.Nothing);
            Assert.That(wkb, Is.Not.Null);

            Geometry geom2 = null;
            Assert.That(() => geom2 = _instance.WKBReader.Read(wkb), Throws.Nothing);
            Assert.That(geom2, Is.Not.Null);

            Assert.That(geom2, Is.EqualTo(geom));
        }
    }
}
