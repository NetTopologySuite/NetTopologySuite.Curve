using System;
using System.Collections.Generic;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A <see cref="Surface{T}"/> implementation of a <see cref="Polygon"/> but
    /// whose rings are made up of <see cref="Curve"/> rings.
    /// </summary>
    [Serializable]
    public class CurvePolygon : Surface<Curve>, ILinearizable<Polygon>
    {
        internal CurvePolygon(Curve exteriorRing, Curve[] interiorRings, CurveGeometryFactory factory)
            : base(factory)
        {
            ExteriorRing = exteriorRing;
            InteriorRings = interiorRings;
        }

        #region Surface{T} implementation

        /// <inheritdoc cref="Surface{T}.ExteriorRing"/>
        public override Curve ExteriorRing { get; }

        private IReadOnlyList<Curve> InteriorRings { get; }


        /// <inheritdoc cref="Surface{T}.NumInteriorRings"/>
        public override int NumInteriorRings
        {
            get => InteriorRings.Count;
        }

        /// <inheritdoc cref="Surface{T}.GetInteriorRingN(int)"/>
        public override Curve GetInteriorRingN(int index)
        {
            return InteriorRings[index];
        }

        #endregion

        #region ILinearizable{T} implementation
        /// <summary>
        /// Gets a value indicating the default maximum length of arc segments that is
        /// used when flattening the curve.
        /// </summary>
        private double ArcSegmentLength
        {
            get => ((CurveGeometryFactory)Factory).ArcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the flattened geometry
        /// </summary>
        private Polygon Linearized { get; set; }

        /// <inheritdoc cref="ILinearizable{T}.Linearize()"/>
        /// <returns>A <c>Polygon</c></returns>
        public Polygon Linearize()
        {
            return Linearize(ArcSegmentLength);
        }

        /// <inheritdoc cref="ILinearizable{T}.Linearize(double)"/>
        /// <returns>A <c>Polygon</c></returns>
        public Polygon Linearize(double arcSegmentLength)
        {
            if (arcSegmentLength < 0)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength), "Must not be negative");

            var linearized = Linearized;
            if (arcSegmentLength == ArcSegmentLength && linearized != null)
                return linearized;

            if (IsEmpty)
                linearized = Factory.CreatePolygon();
            else
            {
                var flattenedShell = ToLinearRing(ExteriorRing, arcSegmentLength);
                var flattenedHoles = new LinearRing[InteriorRings.Count];
                for (int i = 0; i < InteriorRings.Count; i++)
                    flattenedHoles[i] = ToLinearRing(InteriorRings[i], arcSegmentLength);

                linearized = Factory.CreatePolygon(flattenedShell, flattenedHoles);
            }

            if (arcSegmentLength == ArcSegmentLength)
                Linearized = linearized;

            return linearized;
        }

        #endregion

        /// <inheritdoc cref="Geometry.Centroid"/>
        public override Point Centroid => Linearize().Centroid;

        /// <inheritdoc cref="Geometry.InteriorPoint"/>
        public override Point InteriorPoint => Linearize().InteriorPoint;

        /// <inheritdoc cref="Geometry.Coordinates"/>
        public sealed override Coordinate[] Coordinates => Linearize().Coordinates;

        /// <inheritdoc cref="Geometry.GetOrdinates"/>
        public sealed override double[] GetOrdinates(Ordinate ordinate)
        {
            var filter = new GetOrdinatesFilter(ordinate, NumPoints);
            Apply(filter);
            return filter.Ordinates;
        }

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public sealed override int NumPoints => Linearize().NumPoints;

        /// <inheritdoc cref="Geometry.Boundary"/>
        public sealed override Geometry Boundary => Linearize().Boundary;

        /// <inheritdoc cref="Apply(IGeometryFilter)"/>
        public override void Apply(IGeometryFilter filter)
        {
            Linearize().Apply(filter);
        }

        /// <inheritdoc cref="Geometry.Apply(IGeometryComponentFilter)"/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            Linearize().Apply(filter);
        }

        /// <inheritdoc cref="Apply(ICoordinateFilter)"/>
        public override void Apply(ICoordinateFilter filter)
        {
            Linearize().Apply(filter);
        }

        /// <inheritdoc cref="Geometry.Apply(ICoordinateSequenceFilter)"/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            if (IsEmpty)
                return;

            ExteriorRing.Apply(filter);
            if (!filter.Done)
            {
                for (int i = 0; i < InteriorRings.Count; i++)
                {
                    InteriorRings[i].Apply(filter);
                    if (filter.Done)
                        break;
                }
            }
            if (!filter.GeometryChanged)
                return;

            GeometryChanged();
            Linearized = null;
        }

        /// <inheritdoc cref="Geometry.Apply(IEntireCoordinateSequenceFilter)"/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (IsEmpty)
                return;

            ExteriorRing.Apply(filter);
            if (!filter.Done)
            {
                for (int i = 0; i < InteriorRings.Count; i++)
                {
                    InteriorRings[i].Apply(filter);
                    if (filter.Done)
                        break;
                }
            }
            if (!filter.GeometryChanged)
                return;

            GeometryChanged();
            Linearized = null;
        }

        /// <inheritdoc cref="Geometry.IsEquivalentClass"/>
        protected sealed override bool IsEquivalentClass(Geometry other)
        {
            return other is ILinearizable<Polygon> || other is Polygon;
        }

        /// <inheritdoc cref="Geometry.ConvexHull"/>
        public override Geometry ConvexHull()
        {
            return Linearize().ConvexHull();
        }

        /// <inheritdoc cref="Geometry.Contains"/>
        public override bool Contains(Geometry g)
        {
            return Linearize().Contains(g);
        }

        /// <inheritdoc cref="Geometry.Covers"/>
        public override bool Covers(Geometry g)
        {
            return Linearize().Covers(g);
        }

        /// <inheritdoc cref="Geometry.Crosses"/>
        public override bool Crosses(Geometry g)
        {
            return Linearize().Crosses(g);
        }

        /// <inheritdoc cref="Geometry.Intersects"/>
        public override bool Intersects(Geometry g)
        {
            return Linearize().Intersects(g);
        }

        /// <inheritdoc cref="Geometry.Distance"/>
        public override double Distance(Geometry g)
        {
            return Linearize().Distance(g);
        }

        /// <inheritdoc cref="Geometry.Relate(Geometry,string)"/>
        public override bool Relate(Geometry g, string intersectionPattern)
        {
            return Linearize().Relate(g, intersectionPattern);
        }

        /// <inheritdoc cref="Geometry.Relate(Geometry)"/>
        public override IntersectionMatrix Relate(Geometry g)
        {
            return Linearize().Relate(g);
        }

        /// <inheritdoc cref="Geometry.IsSimple"/>
        public override bool IsSimple
        {
            get => Linearize().IsSimple;
        }

        /// <inheritdoc cref="Geometry.IsValid"/>
        public override bool IsValid
        {
            get => Linearize().IsValid;
        }

        /// <inheritdoc cref="Geometry.EqualsTopologically"/>
        public override bool EqualsTopologically(Geometry g)
        {
            return Linearize().EqualsTopologically(g);
        }

        /// <inheritdoc cref="Geometry.Normalize"/>
        public override void Normalize()
        {
            Linearize().Normalize();
            Apply(new NewLinearizedGeometry<Polygon>(Linearized));
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

        /// <inheritdoc cref="Geometry.EqualsExact(NetTopologySuite.Geometries.Geometry,double)"/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (other is CurvePolygon cp)
            {
                if (!ExteriorRing.EqualsExact(cp.ExteriorRing, tolerance))
                    return false;

                if (InteriorRings.Count != cp.InteriorRings.Count)
                    return false;

                for (int i = 0; i < InteriorRings.Count; i++)
                    if (!InteriorRings[i].EqualsExact(cp.InteriorRings[i]))
                        return false;

                return true;
            }

            return Linearize().EqualsExact(other);
        }

        /// <inheritdoc cref="Geometry.CopyInternal()"/>
        protected override Geometry CopyInternal()
        {
            var interiorRings = new Curve[InteriorRings.Count];
            for (int i = 0; i < InteriorRings.Count; i++)
                interiorRings[i] = (Curve)InteriorRings[i].Copy();

            var res = new CurvePolygon((Curve)ExteriorRing.Copy(), interiorRings, (CurveGeometryFactory)Factory);
            return res;
        }

        /// <inheritdoc cref="Geometry.ComputeEnvelopeInternal"/>
        protected override Envelope ComputeEnvelopeInternal()
        {
            return ExteriorRing.EnvelopeInternal;
        }

        /// <inheritdoc cref="Geometry.CompareToSameClass(object)"/>
        protected override int CompareToSameClass(object o)
        {
            if (!(o is ISurface))
                throw new ArgumentException("Not a surface", nameof(o));

            if (o is CurvePolygon cp)
            {
                int comp = ExteriorRing.CompareTo(cp.ExteriorRing);
                if (comp != 0) return comp;
                int minNumRings = Math.Min(NumInteriorRings, cp.NumInteriorRings);
                for (int i = 0; i < minNumRings; i++)
                {
                    comp = InteriorRings[i].CompareTo(cp.InteriorRings[i]);
                    if (comp != 0) return comp;
                }

                return NumInteriorRings.CompareTo(cp.NumInteriorRings);
            }

            if (o is Polygon p)
                return Linearize().CompareTo(p);

            throw new NotSupportedException();
        }

        /// <inheritdoc cref="Geometry.CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected override int CompareToSameClass(object o, IComparer<CoordinateSequence> comparer)
        {
            if (!(o is ISurface))
                throw new ArgumentException("Not a surface", nameof(o));

            if (o is CurvePolygon cp)
            {
                int comp = ExteriorRing.CompareTo(cp.ExteriorRing, comparer);
                if (comp != 0) return comp;
                int minNumRings = Math.Min(NumInteriorRings, cp.NumInteriorRings);
                for (int i = 0; i < minNumRings; i++)
                {
                    comp = InteriorRings[i].CompareTo(cp.InteriorRings[i], comparer);
                    if (comp != 0) return comp;
                }

                return NumInteriorRings.CompareTo(cp.NumInteriorRings);
            }

            if (o is Polygon p)
                return Linearize().CompareTo(p, comparer);

            throw new NotSupportedException();
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType
        {
            get => CurveGeometry.TypeNameCurvePolygon;
        }

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType
        {
            get => OgcGeometryType.CurvePolygon;
        }

        /// <inheritdoc cref="Geometry.Coordinate"/>
        public override Coordinate Coordinate
        {
            get => ExteriorRing.Coordinate;
        }

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty
        {
            get => ExteriorRing.IsEmpty;
        }

        private static LinearRing ToLinearRing(Geometry geom, double arcSegmentLength)
        {
            if (geom is LinearRing linearRing)
                return linearRing;

            if (geom is LineString lineString)
                return geom.Factory.CreateLinearRing(lineString.CoordinateSequence);

            if (geom is ILinearizable<LineString> curve)
                return geom.Factory.CreateLinearRing(curve.Linearize(arcSegmentLength).CoordinateSequence);

            Assert.ShouldNeverReachHere("Invalid geometry type");
            return null;
        }
    }
}
