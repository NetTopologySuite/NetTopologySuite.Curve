using System;

namespace NetTopologySuite.Geometries
{
    public class MultiCurve : GeometryCollection, ICurveGeometry<MultiLineString>, ILineal
    {
        private MultiLineString _flattened;

        public MultiCurve(Geometry[] geometries, CurveGeometryFactory factory)
            : base(geometries, factory)
        {
            if (geometries == null)
                geometries = new Geometry[0];
        }

        Geometry ICurveGeometry.Flatten()
        {
            return Flatten();
        }

        public MultiLineString Flatten(double arcSegmentLength)
        {
            if (arcSegmentLength == ArcSegmentLength && _flattened != null)
                return _flattened;

            var geoms = new LineString[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
            {
                var testGeom = GetGeometryN(i);
                if (testGeom is CurveGeometry<LineString> c)
                    geoms[i] = c.Flatten(arcSegmentLength);
                else
                    geoms[i] = (LineString)testGeom;
            }

            return Factory.CreateMultiLineString(geoms);
        }

        public MultiLineString Flatten()
        {
            return _flattened ?? (_flattened = Flatten(ArcSegmentLength));
        }

        public double ArcSegmentLength { get; }

        protected override SortIndexValue SortIndex
        {
            get => SortIndexValue.MultiLineString;
        }

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public override Dimension Dimension => Dimension.Curve;

        /// <summary>
        ///
        /// </summary>
        /// <value></value>
        public override Dimension BoundaryDimension
        {
            get
            {
                if (IsClosed)
                    return Dimension.False;
                return Dimension.Point;
            }
        }

        /// <summary>
        /// Returns the name of this object's interface.
        /// </summary>
        /// <returns>"MultiLineString"</returns>
        public override string GeometryType => CurveGeometry.TypeNameMultiCurve;

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiLineString;

        /// <summary>
        /// Gets a value indicating whether this instance is closed.
        /// </summary>
        /// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
        public bool IsClosed
        {
            get
            {
                if (IsEmpty)
                    return false;

                for (int i = 0; i < Geometries.Length; i++)
                {
                    var testGeom = GetGeometryN(i);
                    if (testGeom is LineString ls && !ls.IsClosed)
                        return false;
                    if (testGeom is CurveLineString cs && !cs.IsClosed)
                        return false;
                }
                return true;
            }
        }

        public override Geometry Boundary
        {
            get => Flatten().Boundary;
        }

        protected override Geometry ReverseInternal()
        {
            var lineStrings = new Geometry[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
                lineStrings[i] = GetGeometryN(i).Reverse();

            return new MultiCurve(lineStrings, (CurveGeometryFactory)Factory);
        }

        protected override Geometry CopyInternal()
        {
            var lineStrings = new Geometry[NumGeometries];
            for (int i = 0; i < lineStrings.Length; i++)
                lineStrings[i] = GetGeometryN(i).Copy();

            var res = new MultiCurve(lineStrings, (CurveGeometryFactory)Factory);
            res._flattened = (MultiLineString)_flattened?.Copy();

            return res;
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            return base.EqualsExact(other, tolerance);
        }
    }
}
