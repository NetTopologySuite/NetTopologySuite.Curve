using System;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Curve geometry parameter
    /// </summary>
    internal class CurveGeometry
    {
        /// <summary>
        /// OGC
        /// </summary>
        public const string TypeNameCircularString = "CircularString";
        public const string TypeNameCompoundCurve = "CompoundCurve";
        public const string TypeNameCurvePolygon = "CurvePolygon";
        public const string TypeNameMultiCurve = "MultiCurve";
        public const string TypeNameMultiSurface = "MultiSurface";
    }

    ///// <summary>
    ///// Base class for curve geometries
    ///// </summary>
    ///// <typeparam name="T">The type of the flattened geometry</typeparam>
    //[Serializable]
    //public abstract class CurveGeometry<T> : Geometry, ILinearizable<T> where T:Geometry
    //{

    //    /// <summary>
    //    /// Creates an instance of this class
    //    /// </summary>
    //    /// <param name="factory">A factory</param>
    //    protected CurveGeometry(CurveGeometryFactory factory)
    //        : base(factory)
    //    {
    //    }

    //    /// <summary>
    //    /// Gets a value indicating the default maximum length of arc segments that is
    //    /// used when flattening the curve.
    //    /// </summary>
    //    protected double ArcSegmentLength
    //    {
    //        get => ((CurveGeometryFactory) Factory).ArcSegmentLength;
    //    }

    //    /// <summary>
    //    /// Gets a value indicating the flattened geometry
    //    /// </summary>
    //    protected T Linearized { get; set; }

    //    /// <summary>
    //    /// Flatten this curve geometry. The arc segment length used is <see cref="ArcSegmentLength"/>.
    //    /// </summary>
    //    /// <returns>A <c>LineString</c></returns>
    //    public T Linearize()
    //    {
    //        return Linearize(ArcSegmentLength);
    //    }

    //    /// <summary>
    //    /// Flatten this curve geometry using the provided arc segment length.
    //    /// </summary>
    //    /// <param name="arcSegmentLength">The length of arc segments</param>
    //    /// <returns>A flattened geometry</returns>
    //    public T Linearize(double arcSegmentLength)
    //    {
    //        if (arcSegmentLength < 0d)
    //            throw new ArgumentOutOfRangeException(nameof(arcSegmentLength), "must be positive!");

    //        if (arcSegmentLength == ArcSegmentLength)
    //            return Linearized ?? (Linearized = LinearizeInternal(ArcSegmentLength));

    //        return LinearizeInternal(arcSegmentLength);
    //    }

    //    /// <summary>
    //    /// Actual implementation of the flatten procedure
    //    /// </summary>
    //    /// <param name="arcSegmentLength">The maximum length of arc segments</param>
    //    /// <returns>A flattened geometry</returns>
    //    protected abstract T LinearizeInternal(double arcSegmentLength);

    //    /// <inheritdoc cref="Geometry.Centroid"/>
    //    public override Point Centroid => Linearize().Centroid;

    //    /// <inheritdoc cref="Geometry.InteriorPoint"/>
    //    public override Point InteriorPoint => Linearize().InteriorPoint;

    //    /// <inheritdoc cref="Geometry.Coordinates"/>
    //    public sealed override Coordinate[] Coordinates => Linearize().Coordinates;

    //    /// <inheritdoc cref="Geometry.GetOrdinates"/>
    //    public sealed override double[] GetOrdinates(Ordinate ordinate)
    //    {
    //        var filter = new GetOrdinatesFilter(ordinate, NumPoints);
    //        Apply(filter);
    //        return filter.Ordinates;
    //    }

    //    /// <inheritdoc cref="Geometry.NumPoints"/>
    //    public sealed override int NumPoints => Linearize().NumPoints;

    //    /// <inheritdoc cref="Geometry.Boundary"/>
    //    public sealed override Geometry Boundary => Linearize().Boundary;

    //    /// <inheritdoc cref="Apply(IGeometryFilter)"/>
    //    public override void Apply(IGeometryFilter filter)
    //    {
    //        Linearize().Apply(filter);
    //    }

    //    /// <inheritdoc cref="Geometry.Apply(IGeometryComponentFilter)"/>
    //    public override void Apply(IGeometryComponentFilter filter)
    //    {
    //        Linearize().Apply(filter);
    //    }

    //    /// <inheritdoc cref="Apply(ICoordinateFilter)"/>
    //    public override void Apply(ICoordinateFilter filter)
    //    {
    //        Linearize().Apply(filter);
    //    }

    //    /// <inheritdoc cref="Geometry.Apply(ICoordinateSequenceFilter)"/>
    //    public override void Apply(ICoordinateSequenceFilter filter)
    //    {
    //        Linearize().Apply(filter);
    //        if (filter.GeometryChanged)
    //        {
    //            Apply(new NewLinearizedGeometry<T>(Linearized));
    //            GeometryChanged();
    //        }
    //    }

    //    /// <inheritdoc cref="Geometry.Apply(IEntireCoordinateSequenceFilter)"/>
    //    public override void Apply(IEntireCoordinateSequenceFilter filter)
    //    {
    //        Linearize().Apply(filter);
    //        if (filter.GeometryChanged)
    //        {
    //            Apply(new NewLinearizedGeometry<T>(Linearized));
    //            GeometryChanged();
    //        }
    //    }

    //    /// <inheritdoc cref="Geometry.IsEquivalentClass"/>
    //    protected sealed override bool IsEquivalentClass(Geometry other)
    //    {
    //        return other is CurveGeometry<T> || other is T;
    //    }

    //    /// <inheritdoc cref="Geometry.ConvexHull"/>
    //    public override Geometry ConvexHull()
    //    {
    //        return Linearize().ConvexHull();
    //    }

    //    /// <inheritdoc cref="Geometry.Contains"/>
    //    public override bool Contains(Geometry g)
    //    {
    //        return Linearize().Contains(g);
    //    }

    //    /// <inheritdoc cref="Geometry.Covers"/>
    //    public override bool Covers(Geometry g)
    //    {
    //        return Linearize().Covers(g);
    //    }

    //    /// <inheritdoc cref="Geometry.Crosses"/>
    //    public override bool Crosses(Geometry g)
    //    {
    //        return Linearize().Crosses(g);
    //    }

    //    /// <inheritdoc cref="Geometry.Intersects"/>
    //    public override bool Intersects(Geometry g)
    //    {
    //        return Linearize().Intersects(g);
    //    }

    //    /// <inheritdoc cref="Geometry.Distance"/>
    //    public override double Distance(Geometry g)
    //    {
    //        return Linearize().Distance(g);
    //    }

    //    /// <inheritdoc cref="Geometry.Relate(Geometry,string)"/>
    //    public override bool Relate(Geometry g, string intersectionPattern)
    //    {
    //        return Linearize().Relate(g, intersectionPattern);
    //    }

    //    /// <inheritdoc cref="Geometry.Relate(Geometry)"/>
    //    public override IntersectionMatrix Relate(Geometry g)
    //    {
    //        return Linearize().Relate(g);
    //    }

    //    /// <inheritdoc cref="Geometry.IsSimple"/>
    //    public override bool IsSimple
    //    {
    //        get => Linearize().IsSimple;
    //    }

    //    /// <inheritdoc cref="Geometry.IsValid"/>
    //    public override bool IsValid
    //    {
    //        get => Linearize().IsValid;
    //    }

    //    /// <inheritdoc cref="Geometry.EqualsTopologically"/>
    //    public override bool EqualsTopologically(Geometry g)
    //    {
    //        return Linearize().EqualsTopologically(g);
    //    }

    //    /// <inheritdoc cref="Geometry.Normalize"/>
    //    public override void Normalize()
    //    {
    //        Linearize().Normalize();
    //        Apply(new NewLinearizedGeometry<T>(Linearized));
    //    }
    //}
}
