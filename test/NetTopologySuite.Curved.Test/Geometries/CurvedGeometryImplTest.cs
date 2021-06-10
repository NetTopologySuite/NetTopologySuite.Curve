using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    public abstract class CurvedGeometryImplTest
    {
        protected double LengthTolerance { get; }

        private readonly NtsGeometryServices _instance; 

        public CurvedGeometryImplTest(double arcSegmentLength, double lengthTolerance)
        {
            _instance = new NtsCurveGeometryServices(
                CoordinateArraySequenceFactory.Instance, new PrecisionModel(PrecisionModels.Floating), 0,
                new CoordinateEqualityComparer(), arcSegmentLength);

            LengthTolerance = lengthTolerance;
        }

        protected static LineSegment CreateDirectedSegment(Coordinate p0, double dx, double dy) =>
            new LineSegment(p0, new Coordinate(p0.X + dx, p0.Y + dy));

        protected CurveGeometryFactory Factory
        {
            get { return (CurveGeometryFactory) _instance.CreateGeometryFactory(); }
        }

        protected abstract Geometry CreateGeometry();

        protected void CheckEquals(string wkt0, string wkt1, bool expected)
        {
            var geom0 = _instance.WKTReader.Read(wkt0);
            var geom1 = _instance.WKTReader.Read(wkt1);

            Assert.That(geom0.Equals(geom1), Is.EqualTo(expected));
        }

        protected void CheckEqualExact(string wkt0, string wkt1, bool expected)
        {
            var geom0 = _instance.WKTReader.Read(wkt0);
            var geom1 = _instance.WKTReader.Read(wkt1);

            Assert.That(geom0.EqualsExact(geom1, LengthTolerance), Is.EqualTo(expected));
        }

        protected void CheckEqualsNormalized(string wkt0, string wkt1, bool expected)
        {
            var geom0 = _instance.WKTReader.Read(wkt0);
            var geom1 = _instance.WKTReader.Read(wkt1);

            Assert.That(geom0.EqualsNormalized(geom1), Is.EqualTo(expected));
        }

        public abstract void TestIsEmpty();

        public abstract void TestIsSimple();

        public abstract void TestIsValid();

        [Test]
        public void TestSerializeability()
        {
            var geom1 = CreateGeometry();
            TestContext.WriteLine(geom1.ToText());
            Geometry geom2 = null;

            var old = NtsGeometryServices.Instance;

            NtsGeometryServices.Instance = _instance;
            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, geom1);
                ms.Position = 0;
                Assert.That(() => geom2 = (Geometry)bf.Deserialize(ms), Throws.Nothing);
            }

            Assert.That(geom2, Is.Not.Null);
            Assert.That(geom2.EqualsExact(geom1));

            TestContext.WriteLine(geom2.ToText());

            NtsGeometryServices.Instance = old;
        }

        [Test]
        public void TestInteriorPoint()
        {
            var geom = CreateGeometry();
            Geometry interiorPoint = null;

            Assert.That(() => interiorPoint = geom.InteriorPoint, Throws.Nothing);
            Assert.That(interiorPoint, Is.Not.Null);
        }

        [Test]
        public void TestCentroid()
        {
            var geom = CreateGeometry();
            Geometry centroid = null;

            Assert.That(() => centroid = geom.Centroid, Throws.Nothing);
            Assert.That(centroid, Is.Not.Null);
        }

        [Test]
        public void TestConvexHull()
        {
            var geom = CreateGeometry();
            Geometry convexHull = null;

            Assert.That(() => convexHull = geom.ConvexHull(), Throws.Nothing);
            Assert.That(convexHull, Is.Not.Null);

            foreach (var coordinate in geom.Coordinates)
            {
                Assert.That(convexHull.Covers(
                    geom.Factory.CreatePoint(coordinate)), Is.True);
            }
        }
    }
}
