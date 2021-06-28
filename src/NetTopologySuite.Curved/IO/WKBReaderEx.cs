using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class WKBReaderEx : WKBReader
    {
        /// <summary>
        /// Creates an instance of this class using the provided <see cref="NtsCurveGeometryServices"/> object.
        /// </summary>
        /// <param name="geometryServices">A geometry services object.</param>
        public WKBReaderEx(NtsCurveGeometryServices geometryServices)
            : base(geometryServices)
        {
        }

        protected override Geometry ReadOtherGeometry(uint geometryType, BinaryReader reader, CoordinateSystem coordinateSystem, int srid)
        {
            switch (0xffu & geometryType)
            {
                case 8u:
                    return ReadCircularString(reader, coordinateSystem, srid);
                case 9u:
                    return ReadCompoundCurve(reader, coordinateSystem, srid);
                case 10u:
                    return ReadCurvePolygon(reader, coordinateSystem, srid);
                case 11u:
                    return ReadMultiCurve(reader, coordinateSystem, srid);
                case 12u:
                    return ReadMultiSurface(reader, coordinateSystem, srid);
            }
            return null;
        }

        private Geometry ReadCircularString(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = (CurveGeometryFactory)GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numPoints = ReadNumField(reader, "numPoints", ReasonableNumPoints(reader.BaseStream, cs));
            var sequence = ReadCoordinateSequenceLineString(reader, numPoints, cs);
            return factory.CreateCircularString(sequence);
        }

        private Geometry ReadCompoundCurve(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = (CurveGeometryFactory)GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numCurves = ReadNumField(reader, "numCurves", ReasonableNumPoints(reader.BaseStream, cs));

            var curves = new Curve[numCurves];
            for (int i = 0; i < numCurves; i++)
                curves[i] = (Curve)ReadGeometry(reader, srid);
            
            return factory.CreateCompoundCurve(curves);
        }

        private Geometry ReadCurvePolygon(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = (CurveGeometryFactory)GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numRings = ReadNumField(reader, "numRings", ReasonableNumPoints(reader.BaseStream, cs));
            if (numRings == 0)
                return factory.CreateCurvePolygon();

            var exteriorRing = (Curve)ReadGeometry(reader, srid);

            var interiorRings = new Curve[numRings - 1];
            for (int i = 0; i < numRings - 1; i++)
                interiorRings[i] = (Curve)ReadGeometry(reader, srid);

            return factory.CreateCurvePolygon(exteriorRing, interiorRings);
        }

        private Geometry ReadMultiCurve(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = (CurveGeometryFactory)GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numCurves = ReadNumField(reader, "numCurves", ReasonableNumPoints(reader.BaseStream, cs));
            if (numCurves == 0)
                return factory.CreateMultiCurve();

            var curves = new Geometry[numCurves];
            for (int i = 0; i < numCurves; i++)
                curves[i] = ReadGeometry(reader, srid);

            return factory.CreateMultiCurve(curves);
        }

        private Geometry ReadMultiSurface(BinaryReader reader, CoordinateSystem cs, int srid)
        {
            var factory = (CurveGeometryFactory)GeometryServices.CreateGeometryFactory(PrecisionModel, srid, SequenceFactory);
            int numSurfaces = ReadNumField(reader, "numSurfaces", ReasonableNumPoints(reader.BaseStream, cs));
            if (numSurfaces == 0)
                return factory.CreateMultiCurve();

            var surfaces = new Geometry[numSurfaces];
            for (int i = 0; i < numSurfaces; i++)
                surfaces[i] = ReadGeometry(reader, srid);

            return factory.CreateMultiSurface(surfaces);
        }
    }
}
