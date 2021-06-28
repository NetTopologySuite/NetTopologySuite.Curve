using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A collection of multiple <see cref="Curve"/> geometries
    /// </summary>
    public class MultiCurve : GeometryCollection, ILinearizable<MultiLineString>, ILineal
    {
        internal MultiCurve(Geometry[] geometries, CurveGeometryFactory factory)
            : base(geometries ?? Array.Empty<Geometry>(), factory)
        {
        }

        #region ILinearizable{T}

        /// <summary>
        /// Gets a value indicating the default maximum length of arc segments that is
        /// used when linearizing the curves.
        /// </summary>
        private double ArcSegmentLength
        {
            get => ((CurveGeometryFactory) Factory).ArcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the linearized geometry
        /// </summary>
        private MultiLineString Linearized { get; set; }

        /// <summary>
        /// Linearize this <c>MultiCurve</c> geometry. The default arc segment length is used.
        /// </summary>
        /// <returns>A <c>MultiLineString</c></returns>
        public MultiLineString Linearize()
        {
            return Linearize(ArcSegmentLength);
        }

        /// <summary>
        /// Linearize this curve geometry using the provided arc segment length.
        /// </summary>
        /// <param name="arcSegmentLength">The length of arc segments</param>
        /// <returns>A <c>MultiLineString</c></returns>
        public MultiLineString Linearize(double arcSegmentLength)
        {
            var linearized = Linearized;
            if (arcSegmentLength == ArcSegmentLength && linearized != null)
                return linearized;

            if (IsEmpty)
                linearized = Factory.CreateMultiLineString();
            else
            {
                var geoms = new LineString[NumGeometries];
                for (int i = 0; i < NumGeometries; i++)
                {
                    var testGeom = GetGeometryN(i);
                    if (testGeom is ILinearizable<LineString> c)
                        geoms[i] = c.Linearize(arcSegmentLength);
                    else
                        geoms[i] = (LineString) testGeom;
                }

                linearized = Factory.CreateMultiLineString(geoms);
            }

            if (arcSegmentLength == ArcSegmentLength)
                Linearized = linearized;

            return linearized;
        }

        #endregion

        /// <inheritdoc cref="Geometry.SortIndex"/>
        protected override SortIndexValue SortIndex
        {
            get => SortIndexValue.MultiLineString;
        }

        /// <inheritdoc cref="Geometry.Dimension"/>
        public override Dimension Dimension => Dimension.Curve;

        /// <inheritdoc cref="Geometry.BoundaryDimension"/>
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
        /// <returns>"MultiCurve"</returns>
        public override string GeometryType => CurveGeometry.TypeNameMultiCurve;

        /// <summary>
        /// Gets the OGC geometry type
        /// </summary>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.MultiCurve;

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
                    if (testGeom is Curve ls && !ls.IsClosed)
                        return false;
                }
                return true;
            }
        }

        /// <inheritdoc cref="Geometry.Boundary"/>
        public override Geometry Boundary
        {
            get => Linearize().Boundary;
        }

        /// <inheritdoc cref="Geometry.ReverseInternal"/>
        protected override Geometry ReverseInternal()
        {
            var lineStrings = new Geometry[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
                lineStrings[i] = GetGeometryN(i).Reverse();

            return new MultiCurve(lineStrings, (CurveGeometryFactory)Factory);
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>
        protected override Geometry CopyInternal()
        {
            var lineStrings = new Geometry[NumGeometries];
            for (int i = 0; i < lineStrings.Length; i++)
                lineStrings[i] = GetGeometryN(i).Copy();

            var res = new MultiCurve(lineStrings, (CurveGeometryFactory)Factory);
            res.Linearized = (MultiLineString)Linearized?.Copy();

            return res;
        }

        /// <inheritdoc cref="Geometry.EqualsExact(NetTopologySuite.Geometries.Geometry,double)"/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;
            return base.EqualsExact(other, tolerance);
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
