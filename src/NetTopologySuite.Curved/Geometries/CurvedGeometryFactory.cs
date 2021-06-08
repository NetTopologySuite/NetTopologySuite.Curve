using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    public class CurvedGeometryFactory : GeometryFactoryEx
    {
        /// <summary>
        /// Creates an instance of this class using the provided arguments
        /// </summary>
        /// <param name="precisionModel">The precision model to use during computation</param>
        /// <param name="srid">A spatial reference identifier</param>
        /// <param name="coordinateSequenceFactory">The coordinate sequence factory to use when building sequences</param>
        /// <param name="arcSegmentLength">A default arc segment length. A value of <c>0d</c> will lead to arc segment
        /// length to be computed from <see cref="Operation.Buffer.BufferParameters.DefaultQuadrantSegments"/>.</param>
        public CurvedGeometryFactory(PrecisionModel precisionModel, int srid, CoordinateSequenceFactory coordinateSequenceFactory, double arcSegmentLength)
            : base(precisionModel, srid, coordinateSequenceFactory)
        {
            if (arcSegmentLength < 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength));

            ArcSegmentLength = arcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the maximum arc segment length used by curved geometry when
        /// flattening the geometry
        /// </summary>
        public double ArcSegmentLength { get; }

        /// <summary>
        /// Creates a <c>CIRCULARSTRING EMPTY</c> geometry
        /// </summary>
        /// <returns>An empty <c>CIRCULARSTRING</c></returns>
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

            Coordinate last = null;
            // Check for invalid types in linealGeometries
            for (int i = 0; i < linealGeometries.Length; i++)
            {
                if (linealGeometries[i] == null || linealGeometries[i].IsEmpty)
                    throw new ArgumentException(
                        $"linealGeometries contains null or empty geometry!",
                        nameof(linealGeometries));

                var ls = linealGeometries[i] as LineString;
                var cs = linealGeometries[i] as CircularString;
                if (ls == null && cs == null)
                    throw new ArgumentException(
                        $"linealGeometries contains geometry of invalid type: {linealGeometries[i].GeometryType}!",
                        nameof(linealGeometries));

                // Check connectivity
                if (last != null)
                {
                    var first = ls != null
                        ? ls.CoordinateSequence.First()
                        : cs.ControlPoints.First();

                    if (Math.Abs(last.Distance(first)) > 5E-7)
                        throw new ArgumentException("Geometries are not in a sequence", nameof(linealGeometries));
                }

                // Keep last for connectivity check
                last = ls != null
                    ? ls.CoordinateSequence.Last()
                    : cs.ControlPoints.Last();
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
