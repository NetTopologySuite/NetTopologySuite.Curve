using System;
using System.Collections;
using System.Linq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Test.Geometries
{
    [TestFixture(0d, 5E-4)]
    [TestFixture(0.001d, 5E-7)]
    public class CircularStringImplTest : CurvedGeometryImplTest
    {
        public CircularStringImplTest(double arcSegmentLength, double lengthTolerance)
            : base(arcSegmentLength, lengthTolerance)
        {

        }

        protected override Geometry CreateGeometry()
        {
            var pts = new[] {
                new Coordinate(-2, 0), new Coordinate(0, 2), new Coordinate(2, 0),
                new Coordinate(4, -2), new Coordinate(6, 0) };

            return Factory.CreateCircularString(pts);
        }

        [Test]
        public override void TestIsEmpty()
        {
            var pts = Array.Empty<Coordinate>();

            var cs1 = Factory.CreateCircularString();
            var cs2 = Factory.CreateCircularString(pts);

            Assert.That(cs1.IsEmpty);
            Assert.That(cs2.IsEmpty);
        }

        [Test]
        public override void TestIsSimple()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [Test]
        public override void TestIsValid()
        {
            Assert.Inconclusive("Not yet implemented");
        }

        [Test]
        public void TestOneArc()
        {
            var pts = new[] { new Coordinate(-2, 0), new Coordinate(0, 2), new Coordinate(2, 0) };

            CircularString cs = null;
            Assert.That(() => cs = Factory.CreateCircularString(pts), Throws.Nothing);
            Assert.That(cs, Is.Not.Null);

            Check(cs, pts, Math.PI * 2d);

            TestContext.WriteLine(cs.ToText());
        }

        [Test]
        public void TestTwoArcs()
        {
            var pts = new[] {
                new Coordinate(-2, 0), new Coordinate(0, 2), new Coordinate(2, 0),
                new Coordinate(4, -2), new Coordinate(6, 0) };

            CircularString cs = null;
            Assert.That(() => cs = Factory.CreateCircularString(pts), Throws.Nothing);
            Assert.That(cs, Is.Not.Null);

            Check(cs, pts, 2d * Math.PI * 2d);

            TestContext.WriteLine(cs.ToText());
        }

        [Test]
        public void TestTwoArcsRing()
        {
            var pts = new[] {
                new Coordinate(-2, 0), new Coordinate(0, 2), new Coordinate(2, 0),
                new Coordinate(0, -2), new Coordinate(-2, 0) };

            CircularString cs = null;
            Assert.That(() => cs = Factory.CreateCircularString(pts), Throws.Nothing);
            Assert.That(cs, Is.Not.Null);

            Check(cs, pts, 2d * Math.PI * 2d);

            TestContext.WriteLine(cs.ToText());
        }

        public void Check(CircularString cs, Coordinate[] pts, double length)
        {
            Assert.That(cs.Length, Is.EqualTo(length).Within(LengthTolerance));

            for (int i = 0; i < pts.Length; i++)
                Assert.That(cs.IsCoordinate(pts[i]));

            Assert.That(cs.IsEmpty, Is.EqualTo(pts.Length == 0));

            bool shouldBeClosed = pts.Length > 0
                    ? pts.First().Equals2D(pts.Last())
                    : false ;

            Assert.That(cs.IsClosed, Is.EqualTo(shouldBeClosed));
            Assert.That(cs.IsRing, Is.EqualTo(shouldBeClosed && cs.IsSimple));

            Assert.That(!cs.IsRectangle);
            Assert.That(cs.Area, Is.EqualTo(0d));

            // Flatten
            var lineString = cs.Flatten();
            for (int i = 0; i < pts.Length; i++)
                Assert.That(lineString.IsCoordinate(pts[i]));

            if (length < double.Epsilon)
                return;

            double arcSegmentLength = ((CurveGeometryFactory)cs.Factory).ArcSegmentLength;
            if (arcSegmentLength < double.Epsilon)
                return;

            double maxDistance = -double.MaxValue;
            var p0 = lineString.CoordinateSequence.GetCoordinate(0);
            for (int i = 1; i < lineString.CoordinateSequence.Count; i++)
            {
                var p1 = lineString.CoordinateSequence.GetCoordinate(i);
                double distance = p0.Distance(p1);
                if (distance > maxDistance)
                    maxDistance = distance;
                p0 = p1;
            }

            Assert.That(maxDistance <= arcSegmentLength);
        }
    }
}
