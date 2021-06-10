using System;
using System.Collections.Generic;
using System.Globalization;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    public class MultiSurface : GeometryCollection, ICurvedGeometry<MultiPolygon>, IPolygonal
    {
        private MultiPolygon _flattened;

        public MultiSurface(Geometry[] geometries, CurveGeometryFactory factory, double arcSegmentLength)
            : base(geometries, factory)
        {
            if (arcSegmentLength <= 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength));

            for (int i = 0; i < geometries.Length; i++)
            {
                var testGeom = geometries[i];
                if (!(testGeom is IPolygonal))
                    throw new ArgumentException(nameof(geometries));
                if (testGeom is GeometryCollection)
                    throw new ArgumentException(nameof(geometries));
            }

            ArcSegmentLength = arcSegmentLength;
        }

        Geometry ICurvedGeometry.Flatten()
        {
            return Flatten();
        }

        public MultiPolygon Flatten(double arcSegmentLength)
        {
            if (arcSegmentLength == ArcSegmentLength && _flattened != null)
                return _flattened;

            var geoms = new Polygon[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
            {
                var testGeom = GetGeometryN(i);
                if (testGeom is CurveGeometry<Polygon> c)
                    geoms[i] = c.Flatten(arcSegmentLength);
                else
                    geoms[i] = (Polygon)testGeom;
            }

            return Factory.CreateMultiPolygon(geoms);
        }

        public MultiPolygon Flatten()
        {
            return _flattened ?? (_flattened = Flatten(ArcSegmentLength));
        }

        public double ArcSegmentLength { get; }

        protected override Geometry CopyInternal()
        {
            var surfaces = new Geometry[NumGeometries];
            for (int i = 0; i < surfaces.Length; i++)
                surfaces[i] = GetGeometryN(i).Copy();

            return new MultiSurface(surfaces, (CurveGeometryFactory)Factory, ArcSegmentLength);
        }

        protected override SortIndexValue SortIndex => SortIndexValue.MultiPolygon;

        /// <inheritdoc cref="Geometry.Dimension"/>
        public override Dimension Dimension => Dimension.Surface;

        /// <inheritdoc cref="Geometry.BoundaryDimension"/>
        public override Dimension BoundaryDimension => Dimension.Curve;

        
        public override string GeometryType => CurveGeometry.TypeNameMultiSurface;

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiPolygon;

        public override bool IsSimple
        {
            get => Flatten().IsSimple;
        }

        public override Geometry Boundary
        {
            get
            {
                if (IsEmpty)
                    return new LineString(null, (CurveGeometryFactory)Factory);

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

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            return base.EqualsExact(other, tolerance);
        }

        protected override Geometry ReverseInternal()
        {
            var surfaces = new Geometry[NumGeometries];
            for (int i = 0; i < surfaces.Length; i++)
                surfaces[i] = GetGeometryN(i).Reverse();
            return new MultiSurface(surfaces, (CurveGeometryFactory)Factory, ArcSegmentLength);
        }
    }
}
