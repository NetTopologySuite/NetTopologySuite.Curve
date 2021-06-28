using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    public abstract class CurveGeometryImplTest
    {
        protected double LengthTolerance { get; }


        public CurveGeometryImplTest(double arcSegmentLength, double lengthTolerance)
        {
            Instance = new NtsCurveGeometryServices(
                CoordinateArraySequenceFactory.Instance, new PrecisionModel(PrecisionModels.Floating), 0,
                new CoordinateEqualityComparer(), arcSegmentLength);

            LengthTolerance = lengthTolerance;
        }

        protected NtsGeometryServices Instance { get; }


        protected static LineSegment CreateDirectedSegment(Coordinate p0, double dx, double dy) =>
            new LineSegment(p0, new Coordinate(p0.X + dx, p0.Y + dy));

        protected CurveGeometryFactory Factory
        {
            get { return (CurveGeometryFactory) Instance.CreateGeometryFactory(); }
        }

        protected abstract Geometry CreateGeometry();

        protected void CheckEquals(string wkt0, string wkt1, bool expected)
        {
            var geom0 = Instance.WKTReader.Read(wkt0);
            var geom1 = Instance.WKTReader.Read(wkt1);

            Assert.That(geom0.Equals(geom1), Is.EqualTo(expected));
        }

        protected void CheckEqualExact(string wkt0, string wkt1, bool expected)
        {
            var geom0 = Instance.WKTReader.Read(wkt0);
            var geom1 = Instance.WKTReader.Read(wkt1);

            Assert.That(geom0.EqualsExact(geom1, LengthTolerance), Is.EqualTo(expected));
        }

        protected void CheckEqualsNormalized(string wkt0, string wkt1, bool expected)
        {
            var geom0 = Instance.WKTReader.Read(wkt0);
            var geom1 = Instance.WKTReader.Read(wkt1);

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

            NtsGeometryServices.Instance = Instance;
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

        [Test]
        public void TestApplyCoordinateSequenceFilter()
        {
            var geom = CreateGeometry();
            var test = geom.Copy();
            var at = AffineTransformation.TranslationInstance(100, 0);

            Assert.That(() => test.Apply(new AffineTransformationFilter1(at)), Throws.Nothing);
            Assert.That(test.Coordinate.Distance(geom.Coordinate), Is.EqualTo(100d));

            at = AffineTransformation.TranslationInstance(-100, 0);
            test.Apply(new AffineTransformationFilter1(at));

            Assert.That(test.EqualsExact(geom), Is.True);
        }

        [Test]
        public void TestApplyEntireCoordinateSequenceFilter()
        {
            var geom = CreateGeometry();
            var test = geom.Copy();
            var at = AffineTransformation.TranslationInstance(0, 100);

            Assert.That(() => test.Apply(new AffineTransformationFilter1(at)), Throws.Nothing);
            Assert.That(test.Coordinate.Distance(geom.Coordinate), Is.EqualTo(100d));

            at = AffineTransformation.TranslationInstance(0, -100);
            test.Apply(new AffineTransformationFilter1(at));

            Assert.That(test.EqualsExact(geom), Is.True);
        }
    }
}
