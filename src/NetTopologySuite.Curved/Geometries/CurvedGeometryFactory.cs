using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.IO;

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
            WKTReader = new Lazy<WKTReader>(() => new WKTReaderEx(GeometryServices));
            WKTWriter = new Lazy<WKTWriter>(() => new WKTWriterEx(3));
            WKBReader = new Lazy<WKBReader>(() => new WKBReaderEx(GeometryServices));
            WKBWriter = new Lazy<WKBWriter>(() => new WKBWriterEx());

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
        /// Creates an empty <c>CIRCULARSTRING</c> geometry
        /// </summary>
        /// <returns>An empty <c>CIRCULARSTRING</c></returns>
        public CircularString CreateCircularString() => CreateCircularString((CoordinateSequence)null);

        /// <summary>
        /// Creates a <c>CIRCULARSTRING</c> geometry using the provided control points
        /// </summary>
        /// <param name="coordinates">The control points</param>
        /// <returns>A <c>CIRCULARSTRING</c> geometry</returns>
        public CircularString CreateCircularString(Coordinate[] coordinates)
        {
            return CreateCircularString(CoordinateSequenceFactory.Create(coordinates));
        }

        /// <summary>
        /// Creates a <c>CIRCULARSTRING</c> geometry using the provided control points in <paramref name="sequence"/>.
        /// </summary>
        /// <param name="sequence">The control points</param>
        /// <returns>A <c>CIRCULARSTRING</c> geometry</returns>
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

        /// <summary>
        /// Creates an empty <c>COMPOUNDCURVE</c> geometry
        /// </summary>
        /// <returns>An empty <c>COMPOUNDCURVE</c> geometry</returns>
        public CompoundCurve CreateCompoundCurve() => CreateCompoundCurve(Array.Empty<Geometry>());

        /// <summary>
        /// Creates a <c>COMPOUNDCURVE</c> geometry sewed together using the provided .
        /// <paramref name="linealGeometries"/> <b>must not</b> contain <c>null</c> or empty geometries,
        /// the connectivity between them must be ensured.
        /// </summary>
        /// <returns>A <c>COMPOUNDCURVE</c> geometry</returns>
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

        /// <summary>
        /// Creates an empty <c>CURVEDPOLYGON</c> geometry.
        /// </summary>
        /// <returns>An empty <c>CURVEDPOLYGON</c> geometry</returns>
        public CurvedPolygon CreateCurvedPolygon() => CreateCurvedPolygon(null);

        /// <summary>
        /// Creates a <c>CURVEDPOLYGON</c> geometry based on the provided <paramref name="exteriorRing"/> geometry.
        /// </summary>
        /// <param name="exteriorRing">The geometry defining the exterior ring.</param>
        /// <returns>An empty <c>CURVEDPOLYGON</c> geometry</returns>
        public CurvedPolygon CreateCurvedPolygon(Geometry exteriorRing)
        {
            return CreateCurvedPolygon(exteriorRing, Array.Empty<Geometry>());
        }

        /// <summary>
        /// Creates a <c>CURVEDPOLYGON</c> geometry based on the provided <paramref name="exteriorRing"/> and
        /// <paramref name="interiorRings"/> geometries.
        /// </summary>
        /// <param name="exteriorRing">The geometry defining the exterior ring.</param>
        /// <param name="interiorRings">An array of geometries defining the interior rings.</param>
        /// <returns>An empty <c>CURVEDPOLYGON</c> geometry</returns>
        public CurvedPolygon CreateCurvedPolygon(Geometry exteriorRing, Geometry[] interiorRings)
        {
            if (exteriorRing == null)
                exteriorRing = CreateLinearRing();

            if (!(exteriorRing is ICurve exteriorRingCurve))
                throw new ArgumentException("exteriorRing is not a ICurve", nameof(exteriorRing));

            if (!exteriorRingCurve.IsRing)
                throw new ArgumentException("exteriorRing does not form a valid ring", nameof(exteriorRing));

            if (interiorRings == null)
                interiorRings = Array.Empty<Geometry>();

            var extEnv = exteriorRing.EnvelopeInternal;
            for (int i = 0; i < interiorRings.Length; i++)
            {
                if (!(interiorRings[i] is ICurve interiorRingCurve))
                    throw new ArgumentException($"interiorRing[{i}] is not a ICurve", nameof(exteriorRing));
                if (!interiorRingCurve.IsRing)
                    throw new ArgumentException($"interiorRing[{i}] does not form a valid ring", nameof(interiorRings));
                if (!extEnv.Contains(interiorRings[i].EnvelopeInternal))
                    throw new ArgumentException($"interiorRing[{i}] not contained by exterior ring", nameof(interiorRings));
            }

            if (exteriorRing.IsEmpty && interiorRings.Count(t => !t.IsEmpty) > 0)
                throw new ArgumentException("exteriorRing is empty but interiorRings are not", nameof(interiorRings));

            return new CurvedPolygon(exteriorRing, interiorRings, this, ArcSegmentLength);
        }

        /// <summary>
        /// Creates an empty <c>MULTICURVE</c> geometry
        /// </summary>
        /// <returns>An empty <c>MULTICURVE</c> geometry</returns>
        public MultiCurve CreateMultiCurve() => CreateMultiCurve(Array.Empty<Geometry>());

        /// <summary>
        /// Creates a <c>MULTICURVE</c> geometry
        /// </summary>
        /// <param name="geometries">An array of <see cref="ICurve"/> geometries</param>
        /// <returns>A <c>MULTICURVE</c> geometry</returns>
        public MultiCurve CreateMultiCurve(params Geometry[] geometries)
        {
            return new MultiCurve(geometries, this, ArcSegmentLength);
        }

        /// <summary>
        /// Creates an empty <c>MULTISURFACE</c> geometry
        /// </summary>
        /// <returns>An empty <c>MULTISURFACE</c> geometry</returns>
        public MultiSurface CreateMultiSurface() => CreateMultiSurface(Array.Empty<Geometry>());

        /// <summary>
        /// Creates a <c>MULTISURFACE</c> geometry based on the provided
        /// <paramref name="geometries"/> surfaces.
        /// </summary>
        /// <param name="geometries">An array of <see cref="ISurface"/> geometries</param>
        /// <returns>An empty <c>MULTISURFACE</c> geometry</returns>
        public MultiSurface CreateMultiSurface(params Geometry[] geometries)
        {
            return new MultiSurface(geometries, this, ArcSegmentLength);
        }

        /// <inheritdoc cref="BuildGeometry"/>
        public override Geometry BuildGeometry(IEnumerable<Geometry> geomList)
        {
            var geoms = new List<Geometry>();

            /*
             * Determine some facts about the geometries in the list
             */
            Type geomClass = null;
            bool isHeterogeneous = false;
            bool hasGeometryCollection = false;
            int numCurve = 0, numSurface = 0;
            Geometry geom0 = null;
            foreach (var geom in geomList)
            {
                geoms.Add(geom);
                if (geom == null) continue;
                geom0 = geom;

                var partClass = geom.GetType();
                if (geomClass == null)
                    geomClass = partClass;
                if (partClass != geomClass)
                    isHeterogeneous = true;
                if (geom is GeometryCollection)
                    hasGeometryCollection = true;
                if (geom is ISurface)
                    numSurface++;
                if (geom is ICurve)
                    numCurve++;
            }

            /*
             * Now construct an appropriate geometry to return
             */

            // for the empty point, return an empty GeometryCollection
            if (geomClass == null)
                return CreateGeometryCollection(null);

            // Clear heterogenous flag if all geometries are surface or curve
            if (isHeterogeneous && (numSurface == geoms.Count || numCurve == geoms.Count))
                isHeterogeneous = false;

            // for heterogenous collection of geometries or if it contains a GeometryCollection, return a GeometryCollection
            if (isHeterogeneous || hasGeometryCollection)
                return CreateGeometryCollection(geoms.ToArray());

            // at this point we know the collection is homogenous.
            // Determine the type of the result from the first Geometry in the list
            // this should always return a point, since otherwise an empty collection would have already been returned
            bool isCollection = geoms.Count > 1;

            if (isCollection)
            {
                if (geom0 is Polygon)
                    return CreateMultiPolygon(ToPolygonArray(geoms));
                if (geom0 is ISurface)
                    return CreateMultiSurface(geoms.ToArray());
                if (geom0 is LineString)
                    return CreateMultiLineString(ToLineStringArray(geoms));
                if (geom0 is ICurve)
                    return CreateMultiCurve(geoms.ToArray());
                if (geom0 is Point)
                    return CreateMultiPoint(ToPointArray(geoms));

                throw new NotSupportedException($"Unhandled class: {geom0.GetType().FullName}");
                //Assert.ShouldNeverReachHere("Unhandled class: " + geom0.GetType().FullName);
            }
            return geom0;
        }
    }
}

