using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A collection of multiple <see cref="Surface{T}"/>s.
    /// </summary>
    public class MultiSurface : GeometryCollection, ILinearizable<MultiPolygon>, IPolygonal
    {
        internal MultiSurface(Geometry[] geometries, CurveGeometryFactory factory)
            : base(geometries, factory)
        {
            for (int i = 0; i < geometries.Length; i++)
            {
                var testGeom = geometries[i];
                if (!(testGeom is ISurface))
                    throw new ArgumentException(nameof(geometries));
                if (testGeom is GeometryCollection)
                    throw new ArgumentException(nameof(geometries));
            }
        }


        #region ILinearizable{T} implementation

        /// <summary>
        /// Gets a value indicating the default maximum length of arc segments that is
        /// used when linearizing the curve.
        /// </summary>
        private double ArcSegmentLength
        {
            get => ((CurveGeometryFactory)Factory).ArcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the linearized geometry
        /// </summary>
        private MultiPolygon Linearized { get; set; }

        /// <inheritdoc cref="ILinearizable{T}.Linearize()"/>
        /// <returns>A <c>MultiPolygon</c></returns>
        public MultiPolygon Linearize()
        {
            return Linearize(ArcSegmentLength);
        }

        /// <inheritdoc cref="ILinearizable{T}.Linearize(double)"/>
        /// <returns>A <c>MultiPolygon</c></returns>
        public MultiPolygon Linearize(double arcSegmentLength)
        {
            if (arcSegmentLength < 0)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength), "Must not be negative");

            var linearized = Linearized;
            if (arcSegmentLength == ArcSegmentLength && linearized != null)
                return linearized;

            if (IsEmpty)
                linearized = Factory.CreateMultiPolygon();
            else
            {
                var geoms = new Polygon[NumGeometries];
                for (int i = 0; i < NumGeometries; i++)
                {
                    var testGeom = GetGeometryN(i);
                    if (testGeom is ILinearizable<Polygon> c)
                        geoms[i] = c.Linearize(arcSegmentLength);
                    else
                        geoms[i] = (Polygon) testGeom;
                }

                linearized = Factory.CreateMultiPolygon(geoms);
            }

            if (arcSegmentLength == ArcSegmentLength)
                Linearized = linearized;

            return linearized;
        }

        #endregion

        /// <inheritdoc cref="Geometry.SortIndex"/>
        protected override SortIndexValue SortIndex => SortIndexValue.MultiPolygon;

        /// <inheritdoc cref="Geometry.Dimension"/>
        public override Dimension Dimension => Dimension.Surface;

        /// <inheritdoc cref="Geometry.BoundaryDimension"/>
        public override Dimension BoundaryDimension => Dimension.Curve;

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => CurveGeometry.TypeNameMultiSurface;

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiSurface;

        /// <inheritdoc cref="Geometry.IsSimple"/>
        public override bool IsSimple
        {
            get => Linearize().IsSimple;
        }

        /// <inheritdoc cref="Geometry.Boundary"/>
        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty)
                    return Factory.CreateLineString();

                var allRings = new List<Geometry>();
                for (int i = 0; i < NumGeometries; i++)
                {
                    var surface = GetGeometryN(i);
                    var rings = surface.Boundary;
                    for (int j = 0; j < rings.NumGeometries; j++)
                        allRings.Add(rings.GetGeometryN(i));
                }

                return Factory.BuildGeometry(allRings);
            }
        }

        /// <inheritdoc cref="Geometry.EqualsExact(Geometry, double)"/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            return base.EqualsExact(other, tolerance);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>
        protected override Geometry CopyInternal()
        {
            var surfaces = new Geometry[NumGeometries];
            for (int i = 0; i < surfaces.Length; i++)
                surfaces[i] = GetGeometryN(i).Copy();

            return new MultiSurface(surfaces, (CurveGeometryFactory)Factory);
        }

        /// <inheritdoc cref="Geometry.ReverseInternal"/>
        protected override Geometry ReverseInternal()
        {
            var surfaces = new Geometry[NumGeometries];
            for (int i = 0; i < surfaces.Length; i++)
                surfaces[i] = GetGeometryN(i).Reverse();
            return new MultiSurface(surfaces, (CurveGeometryFactory)Factory);
        }

        /// <inheritdoc cref="Geometry.Apply(ICoordinateSequenceFilter)"/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            base.Apply(filter);
            if (filter.GeometryChanged)
                Linearized = null;
        }

        /// <inheritdoc cref="Geometry.Apply(IEntireCoordinateSequenceFilter)"/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            base.Apply(filter);
            if (filter.GeometryChanged)
                Linearized = null;
        }
    }
}
