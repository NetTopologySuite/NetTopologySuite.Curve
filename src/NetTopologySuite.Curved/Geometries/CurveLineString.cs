using System;
using System.Collections.Generic;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Base class for curved single component geometries
    /// </summary>
    [Serializable]
    public abstract class CurveLineString : CurveGeometry<LineString>, ICurve
    {
        /// <summary>
        /// Creates an instance of this class using the provided Factory
        /// </summary>
        /// <param name="factory"></param>
        protected CurveLineString(CurveGeometryFactory factory)
            : base(factory)
        {
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
        /// Returns a <c>CoordinateSequence</c> containing the values of all the vertices for
        /// <b>the flattened version</b> of this geometry.
        /// </summary>
        /// <remarks>
        /// If the geometry is a composite, the coordinate sequence will contain all the vertices
        /// for the components, in the order in which the components occur in the geometry.
        /// <para>
        /// In general, the sequence cannot be assumed to be the actual internal
        /// storage for the vertices. Thus modifying the array
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
        public CoordinateSequence CoordinateSequence => Flatten().CoordinateSequence;

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Coordinate GetCoordinateN(int n)
        {
            if (IsEmpty)
                return null;

            return Flatten().GetCoordinateN(n);
        }

        public override Dimension Dimension { get => Dimension.Curve; }

        /// <inheritdoc cref="Geometry.BoundaryDimension"/>
        public override Dimension BoundaryDimension
        {
            get
            {
                if (IsClosed)
                {
                    return Dimension.False;
                }
                return Dimension.Point;
            }
        }

        public Point GetPointN(int n)
        {
            if (IsEmpty)
                return null;

            return Flatten().GetPointN(n);
        }

        /// <summary>
        /// Gets a value indicating the start point of this <c>Curve</c>
        /// </summary>
        public Point StartPoint
        {
            get
            {
                return GetPointN(0);
            }
        }

        /// <summary>
        /// Gets a value indicating the end point of this <c>Curve</c>
        /// </summary>
        public Point EndPoint
        {
            get
            {
                return GetPointN(NumPoints - 1);
            }
        }

        public virtual bool IsClosed
        {
            get
            {
                return Flatten().IsClosed;
            }
        }

        public bool IsRing
        {
            get => Flatten().IsRing && IsSimple;
        }

        /// <summary>
        /// Returns true if the given point is a vertex of this <c>LineString</c>.
        /// </summary>
        /// <param name="pt">The <c>Coordinate</c> to check.</param>
        /// <returns><c>true</c> if <c>pt</c> is one of this <c>CurvedLineString</c>'s vertices.</returns>
        public virtual bool IsCoordinate(Coordinate pt)
        {
            var points = CoordinateSequence;
            for (int i = 0; i < points.Count; i++)
                if (points.GetCoordinate(i).Equals(pt))
                    return true;
            return false;
        }
    }
}
