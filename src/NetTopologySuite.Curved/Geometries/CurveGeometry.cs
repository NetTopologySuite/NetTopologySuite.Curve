using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Curved geometry parameter
    /// </summary>
    internal class CurveGeometry
    {
        public const string TypeNameCircularString = "CircularString";
        public const string TypeNameCompoundCurve = "CompoundCurve";
        public const string TypeNameCurvedPolygon = "CurvePolygon";
        public const string TypeNameMultiCurve = "MultiCurve";
        public const string TypeNameMultiSurface = "MultiSurface";
    }

    /// <summary>
    /// Base class for curved geometries
    /// </summary>
    /// <typeparam name="T">The type of the flattened geometry</typeparam>
    [Serializable]
    public abstract class CurveGeometry<T> : Geometry, ICurvedGeometry<T> where T:Geometry
    {

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="factory">A factory</param>
        protected CurveGeometry(CurveGeometryFactory factory)
            : base(factory)
        {
        }

        /// <summary>
        /// Gets a value indicating the default maximum length of arc segments that is
        /// used when flattening the curve.
        /// </summary>
        public double ArcSegmentLength
        {
            get => ((CurveGeometryFactory) Factory).ArcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the flattened geometry
        /// </summary>
        protected T Flattened { get; set; }

        /// <summary>
        /// Flatten this curve geometry. The arc segment length used is <see cref="ArcSegmentLength"/>.
        /// </summary>
        /// <returns>A <c>LineString</c></returns>
        public T Flatten()
        {
            return Flatten(ArcSegmentLength);
        }

        /// <summary>
        /// Flatten this curve geometry using the provided arc segment length.
        /// </summary>
        /// <param name="arcSegmentLength">The length of arc segments</param>
        /// <returns>A flattened geometry</returns>
        public T Flatten(double arcSegmentLength)
        {
            if (arcSegmentLength < 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength), "must be positive!");

            if (arcSegmentLength == ArcSegmentLength)
                return Flattened ?? (Flattened = FlattenInternal(ArcSegmentLength));

            return FlattenInternal(arcSegmentLength);
        }

        /// <summary>
        /// Actual implementation of the flatten procedure
        /// </summary>
        /// <param name="arcSegmentLength">The maximum length of arc segments</param>
        /// <returns>A flattened geometry</returns>
        protected abstract T FlattenInternal(double arcSegmentLength);

        Geometry ICurvedGeometry.Flatten()
        {
            return Flatten();
        }

        /// <summary>
        /// Returns an array containing the values of all the vertices for
        /// this geometry.
        /// </summary>
        /// <remarks>
        /// If the geometry is a composite, the array will contain all the vertices
        /// for the components, in the order in which the components occur in the geometry.
        /// <para>
        /// In general, the array cannot be assumed to be the actual internal
        /// storage for the vertices.  Thus modifying the array
        /// may not modify the geometry itself.
        /// Use the <see cref="Geometries.CoordinateSequence.SetOrdinate(int, int, double)"/> method
        /// (possibly on the components) to modify the underlying data.
        /// If the coordinates are modified,
        /// <see cref="Geometry.GeometryChanged"/> must be called afterwards.
        /// </para>
        /// </remarks>
        /// <returns>The vertices of this <c>Geometry</c>.</returns>
        /// <seealso cref="Geometry.GeometryChanged"/>
        /// <seealso cref="Geometries.CoordinateSequence.SetOrdinate(int, int, double)"/>
        /// <seealso cref="Geometries.CoordinateSequence.SetOrdinate(int, Ordinate, double)"/>
        public sealed override Coordinate[] Coordinates => Flatten().Coordinates;

        /// <inheritdoc cref="Geometry.GetOrdinates"/>
        public sealed override double[] GetOrdinates(Ordinate ordinate)
        {
            var filter = new GetOrdinatesFilter(ordinate, NumPoints);
            Apply(filter);
            return filter.Ordinates;
        }

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public sealed override int NumPoints => Flatten().NumPoints;

        ///// <summary>
        ///// Returns the length of this <c>LineString</c>
        ///// </summary>
        ///// <returns>The length of the polygon.</returns>
        //public override double Length => Flatten().Length;

        /// <summary>
        /// Returns the boundary, or an empty geometry of appropriate dimension
        /// if this <c>Geometry</c> is empty.
        /// For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <c>Geometry</c>.</returns>
        public sealed override Geometry Boundary => Flatten().Boundary;

        /// <inheritdoc cref="Apply(IGeometryFilter)"/>
        public override void Apply(IGeometryFilter filter)
        {
            Flatten().Apply(filter);
        }

        /// <inheritdoc cref="Apply(IGeometryComponentFilter)"/>
        public override void Apply(IGeometryComponentFilter filter)
        {
            Flatten().Apply(filter);
        }

        /// <inheritdoc cref="Apply(ICoordinateFilter)"/>
        public override void Apply(ICoordinateFilter filter)
        {
            Flatten().Apply(filter);
        }

        private class NewFlattenedGeometry : IGeometryComponentFilter
        {
            public NewFlattenedGeometry(T flattened)
            {
                Flattened = flattened;
            }

            private T Flattened { get; }

            public void Filter(Geometry geom)
            {
                if (geom is CurveGeometry<T> curve)
                {
                    //TODO 
                }
                geom.GeometryChangedAction();
            }
        }

        /// <inheritdoc cref="Apply(ICoordinateSequenceFilter)"/>
        public override void Apply(ICoordinateSequenceFilter filter)
        {
            Flatten().Apply(filter);
            if (filter.GeometryChanged)
                Apply(new NewFlattenedGeometry(Flattened));
        }

        /// <inheritdoc cref="Apply(IEntireCoordinateSequenceFilter)"/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            Flatten().Apply(filter);
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        /// <inheritdoc cref="IsEquivalentClass"/>
        protected sealed override bool IsEquivalentClass(Geometry other)
        {
            return other is CurveGeometry<T> || other is T;
        }

        /// <inheritdoc cref="ConvexHull"/>
        public override Geometry ConvexHull()
        {
            return Flatten().ConvexHull();
        }

        /// <inheritdoc cref="Contains"/>
        public override bool Contains(Geometry g)
        {
            return Flatten().Contains(g);
        }

        /// <inheritdoc cref="Covers"/>
        public override bool Covers(Geometry g)
        {
            return Flatten().Covers(g);
        }

        /// <inheritdoc cref="Crosses"/>
        public override bool Crosses(Geometry g)
        {
            return Flatten().Crosses(g);
        }

        /// <inheritdoc cref="Intersects"/>
        public override bool Intersects(Geometry g)
        {
            return Flatten().Intersects(g);
        }

        /// <inheritdoc cref="Distance"/>
        public override double Distance(Geometry g)
        {
            return Flatten().Distance(g);
        }

        /// <inheritdoc cref="Relate(Geometry,string)"/>
        public override bool Relate(Geometry g, string intersectionPattern)
        {
            return Flatten().Relate(g, intersectionPattern);
        }

        /// <inheritdoc cref="Relate(Geometry)"/>
        public override IntersectionMatrix Relate(Geometry g)
        {
            return Flatten().Relate(g);
        }

        /// <inheritdoc cref="IsSimple"/>
        public override bool IsSimple
        {
            get => Flatten().IsSimple;
        }

        /// <inheritdoc cref="IsValid"/>
        public override bool IsValid
        {
            get => Flatten().IsValid;
        }

        /// <inheritdoc cref="EqualsTopologically"/>
        public override bool EqualsTopologically(Geometry g)
        {
            return Flatten().EqualsTopologically(g);
        }

        /// <inheritdoc cref="Normalize"/>
        public override void Normalize()
        {
            Flatten().Normalize();
            Apply(new NewFlattenedGeometry(Flattened));
        }
    }
}
