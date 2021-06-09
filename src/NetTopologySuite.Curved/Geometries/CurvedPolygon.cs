using System;
using System.Collections.Generic;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    public class CurvedPolygon : CurvedGeometry<Polygon>, ISurface<Geometry>
    {
        public CurvedPolygon(CompoundCurve exteriorRing, CurvedGeometryFactory factory)
            : this(exteriorRing, Array.Empty<Geometry>(), factory)
        {
        }

        internal CurvedPolygon(Geometry exteriorRing, Geometry[] interiorRings, CurvedGeometryFactory factory)
            : base(factory)
        {
            ExteriorRing = exteriorRing;
            InteriorRings = interiorRings;
        }


        public Geometry ExteriorRing { get; }

        private IReadOnlyList<Geometry> InteriorRings { get; }


        public int NumInteriorRings
        {
            get => InteriorRings.Count;
        }

        public Geometry GetInteriorRingN(int index)
        {
            return InteriorRings[index];
        }

        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.Polygon;

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (other is CurvedPolygon cp)
            {
                if (!ExteriorRing.EqualsExact(cp.ExteriorRing, tolerance))
                    return false;

                if (InteriorRings.Count != cp.InteriorRings.Count)
                    return false;

                for (int i = 0; i < InteriorRings.Count; i++)
                    if (!InteriorRings[i].EqualsExact(cp.InteriorRings[i]))
                        return false;
            }

            return Flatten().EqualsExact(other);
        }

        protected override Geometry CopyInternal()
        {
            var interiorRings = new Geometry[InteriorRings.Count];
            for (int i = 0; i < InteriorRings.Count; i++)
                interiorRings[i] = InteriorRings[i].Copy();

            var res = new CurvedPolygon(ExteriorRing.Copy(), interiorRings, (CurvedGeometryFactory)Factory);
            return res;
        }

        protected override Envelope ComputeEnvelopeInternal()
        {
            return ExteriorRing.EnvelopeInternal;
        }

        protected internal override int CompareToSameClass(object o)
        {
            if (!(o is ISurface))
                throw new ArgumentException("Not a surface", nameof(o));

            if (o is CurvedPolygon cp)
            {
                int comp = ExteriorRing.CompareToSameClass(cp.ExteriorRing);
                if (comp != 0) return comp;
                int minNumRings = Math.Min(NumInteriorRings, cp.NumInteriorRings);
                for (int i = 0; i < minNumRings; i++)
                {
                    comp = InteriorRings[i].CompareToSameClass(cp.InteriorRings[i]);
                    if (comp != 0) return comp;
                }

                return NumInteriorRings.CompareTo(cp.NumInteriorRings);
            }

            if (o is Polygon p)
                return Flatten().CompareToSameClass(p);

            throw new NotSupportedException();
        }

        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comparer)
        {
            if (!(o is ISurface))
                throw new ArgumentException("Not a surface", nameof(o));

            if (o is CurvedPolygon cp)
            {
                int comp = ExteriorRing.CompareToSameClass(cp.ExteriorRing, comparer);
                if (comp != 0) return comp;
                int minNumRings = Math.Min(NumInteriorRings, cp.NumInteriorRings);
                for (int i = 0; i < minNumRings; i++)
                {
                    comp = InteriorRings[i].CompareToSameClass(cp.InteriorRings[i], comparer);
                    if (comp != 0) return comp;
                }

                return NumInteriorRings.CompareTo(cp.NumInteriorRings);
            }

            if (o is Polygon p)
                return Flatten().CompareToSameClass(p, comparer);

            throw new NotSupportedException();
        }

        public override string GeometryType
        {
            get => CurvedGeometry.TypeNameCurvedPolygon;
        }

        public override OgcGeometryType OgcGeometryType
        {
            get => OgcGeometryType.CurvePolygon;
        }

        public override Coordinate Coordinate
        {
            get => ExteriorRing.Coordinate;
        }

        public override bool IsEmpty
        {
            get => ExteriorRing.IsEmpty;
        }

        public override Dimension Dimension
        {
            get => Dimension.Surface;
        }

        public override Dimension BoundaryDimension
        {
            get => Dimension.Curve;
        }

        protected override Polygon FlattenInternal(double arcSegmentLength)
        {
            var flattenedShell = ToLinearRing(ExteriorRing, arcSegmentLength);
            var flattenedHoles = new LinearRing[InteriorRings.Count];
            for (int i = 0; i < InteriorRings.Count; i++)
                flattenedHoles[i] = ToLinearRing(InteriorRings[i], arcSegmentLength);

            return Factory.CreatePolygon(flattenedShell, flattenedHoles);
        }

        private static LinearRing ToLinearRing(Geometry geom, double arcSegmentLength)
        {
            if (geom is LinearRing linearRing)
                return linearRing;

            if (geom is LineString lineString)
                return geom.Factory.CreateLinearRing(lineString.CoordinateSequence);

            if (geom is ICurvedGeometry<LineString> curved)
                return geom.Factory.CreateLinearRing(curved.Flatten(arcSegmentLength).CoordinateSequence);

            Assert.ShouldNeverReachHere("Invalid geometry type");
            return null;
        }
    }
}
