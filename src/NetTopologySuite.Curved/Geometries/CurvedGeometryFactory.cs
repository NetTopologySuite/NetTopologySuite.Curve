using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    public class CurvedGeometryFactory : GeometryFactoryEx
    {
        public CurvedGeometryFactory(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory, double arcSegmentLength)
            : base(precisionModel, srid, coordinateSequenceFactory)
        {
            if (arcSegmentLength <= 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength));

            ArcSegmentLength = arcSegmentLength;
        }

        public double ArcSegmentLength { get; }

        public CircularString CreateCircularString() => CreateCircularString((CoordinateSequence)null);

        public CircularString CreateCircularString(Coordinate[] coordinates)
        {
            return CreateCircularString(CoordinateSequenceFactory.Create(coordinates));
        }

        public CircularString CreateCircularString(CoordinateSequence sequence)
        {
            if (sequence == null)
                sequence = CoordinateSequenceFactory.Create(0, Ordinates.XY);

            if (sequence.Count > 0)
            {
                if (sequence.Count < 3 || (sequence.Count - 1) % 2 != 0)
                    throw new ArgumentException("Invalid number of control points", nameof(sequence));
            }
            return new CircularString(sequence, this, ArcSegmentLength);
        }

        public CompoundCurve CreateCompoundCurve() => CreateCompoundCurve(Array.Empty<Geometry>());

        public CompoundCurve CreateCompoundCurve(Geometry[] linealGeometries)
        {
            // Ensure linealGeometries is not null
            if (linealGeometries == null)
                linealGeometries = new Geometry[0];

            // Check for invalid types in linealGeometries
            for (int i = 0; i < linealGeometries.Length; i++)
            {
                if (!(linealGeometries[i] is LineString ||
                      linealGeometries[i] is CircularString))
                    throw new ArgumentException(
                        $"linealGeometries contains geometry of invalid type: {linealGeometries[i].GeometryType}!",
                        nameof(linealGeometries));
            }

            // Create geometry
            return new CompoundCurve(linealGeometries, this, ArcSegmentLength);
        }

        public CurvedPolygon CreateCurvedPolygon() => CreateCurvedPolygon(CreateCompoundCurve());

        public CurvedPolygon CreateCurvedPolygon(CompoundCurve exteriorRing)
        {
            return CreateCurvedPolygon(exteriorRing, new CompoundCurve[0]);
        }

        public CurvedPolygon CreateCurvedPolygon(CompoundCurve exteriorRing, CompoundCurve[] interiorRings)
        {
            return new CurvedPolygon(exteriorRing, interiorRings, this, ArcSegmentLength);
        }

        public MultiCurve CreateMultiCurve(params Geometry[] geometries)
        {
            return new MultiCurve(geometries, this, ArcSegmentLength);
        }

        public override Geometry BuildGeometry(IEnumerable<Geometry> geomList)
        {
            return base.BuildGeometry(geomList);
        }

    }
}
