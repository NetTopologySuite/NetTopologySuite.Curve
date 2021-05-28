using System.Xml;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Test.Geometries
{
    public class CircularArcTest
    {

        
        public void Test(double p0X, double p0Y, double p1X, double p1Y, double p2X, double p2Y, double cx, double cy, double radius)
        {
            var p0 = new Coordinate(p0X, p0Y);
            var p1 = new Coordinate(p0X, p0Y);
            var p2 = new Coordinate(p0X, p0Y);

            var ca = new CircularArc(p0, p1, p2);
        }
    }
}
