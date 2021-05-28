using System;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    internal class CurvedGeometry
    {
        public const string TypeNameCircularString = "CircularString";
        public const string TypeNameCompoundCurve = "CompoundCurve";
        public const string TypeNameCurvedPolygon = "CurvedPolygon";
        public const string TypeNameMultiCurve = "MultiCurve";
        public const string TypeNameMultiSurface = "MultiSurface";
    }

    [Serializable]
    public abstract class CurvedGeometry<T> : Geometry, ICurvedGeometry<T> where T:Geometry
    {

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="factory">A factory</param>
        /// <param name="arcSegmentLength"></param>
        protected CurvedGeometry(CurvedGeometryFactory factory, double arcSegmentLength)
            : base(factory)
        {
            if (arcSegmentLength <= 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength));
            ArcSegmentLength = arcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the default maximum length of arc segments that is
        /// used when flattening the curve.
        /// </summary>
        public double ArcSegmentLength { get; }

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
        /// <returns>A <c>LineString</c></returns>
        public T Flatten(double arcSegmentLength)
        {
            if (arcSegmentLength <= 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength), "must be positive!");

            if (arcSegmentLength == ArcSegmentLength)
                return Flattened ?? (Flattened = FlattenInternal(ArcSegmentLength));

            return FlattenInternal(arcSegmentLength);
        }

        protected abstract T FlattenInternal(double arcSegmentLength);

        Geometry ICurvedGeometry.Flatten()
        {
            return Flatten();
        }


        /// <summary>
        /// Gets a value to sort the geometry
        /// </summary>
        /// <remarks>
        /// NOTE:<br/>
        /// For JTS v1.17 this property's getter has been renamed to <c>getTypeCode()</c>.
        /// In order not to break binary compatibility we did not follow.
        /// </remarks>
        protected override SortIndexValue SortIndex => SortIndexValue.LineString;

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
        public override Coordinate[] Coordinates => Flatten().Coordinates;

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints => Flatten().NumPoints;

        /// <summary>
        /// Returns the length of this <c>LineString</c>
        /// </summary>
        /// <returns>The length of the polygon.</returns>
        public override double Length => Flatten().Length;

        /// <summary>
        /// Returns the boundary, or an empty geometry of appropriate dimension
        /// if this <c>Geometry</c> is empty.
        /// For a discussion of this function, see the OpenGIS Simple
        /// Features Specification. As stated in SFS Section 2.1.13.1, "the boundary
        /// of a Geometry is a set of Geometries of the next lower dimension."
        /// </summary>
        /// <returns>The closure of the combinatorial boundary of this <c>Geometry</c>.</returns>
        public override Geometry Boundary => Flatten().Boundary;

        public override void Apply(IGeometryFilter filter)
        {
            Flatten().Apply(filter);
        }

        public override void Apply(IGeometryComponentFilter filter)
        {
            Flatten().Apply(filter);
        }

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
                if (geom is CurvedGeometry<T> curve)
                {
                    //TODO 
                }
                geom.GeometryChangedAction();
            }
        }

        public override void Apply(ICoordinateSequenceFilter filter)
        {
            Flatten().Apply(filter);
            if (filter.GeometryChanged)
                Apply(new NewFlattenedGeometry(Flattened));
        }

        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            Flatten().Apply(filter);
            if (filter.GeometryChanged)
                GeometryChanged();
        }

        protected override bool IsEquivalentClass(Geometry other)
        {
            return other is CurvedGeometry<T> || other is T;
        }

        protected override int CompareToSameClass(object o)
        {
            var curve = o as CurvedGeometry<T>;
            var line = o as T;
            if (curve == null && line == null)
                Assert.ShouldNeverReachHere("Curve or LineString type expected!");

            if (curve != null)
                line = curve.Flatten();

            return Flatten().CompareTo(line);
        }

        public override Geometry ConvexHull()
        {
            return Flatten().ConvexHull();
        }

        public override bool Contains(Geometry g)
        {
            return Flatten().Contains(g);
        }

        public override bool Covers(Geometry g)
        {
            return Flatten().Covers(g);
        }

        public override bool Crosses(Geometry g)
        {
            return Flatten().Crosses(g);
        }

        public override bool Intersects(Geometry g)
        {
            return Flatten().Intersects(g);
        }

        public override double Distance(Geometry g)
        {
            return Flatten().Distance(g);
        }

        public override bool Relate(Geometry g, string intersectionPattern)
        {
            return Flatten().Relate(g, intersectionPattern);
        }

        public override IntersectionMatrix Relate(Geometry g)
        {
            return Flatten().Relate(g);
        }

        public override bool IsSimple
        {
            get => Flatten().IsSimple;
        }

        public override bool IsValid
        {
            get => Flatten().IsValid;
        }

        public override bool EqualsTopologically(Geometry g)
        {
            return Flatten().EqualsTopologically(g);
        }

        public override void Normalize()
        {
            Flatten().Normalize();
            Apply(new NewFlattenedGeometry(Flattened));
        }

        
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
