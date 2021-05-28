using System.Collections.Generic;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    public abstract class CurvedLineString : CurvedGeometry<LineString>
    {
        protected CurvedLineString(CurvedGeometryFactory factory, double arcSegmentLength)
            : base(factory, arcSegmentLength)
        {
        }

        /// <summary>
        /// Gets an array of <see cref="double"/> ordinate values
        /// </summary>
        /// <param name="ordinate">The ordinate index</param>
        /// <returns>An array of ordinate values</returns>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            if (IsEmpty)
                return new double[0];

            var ordinateFlag = (Ordinates)(1 << (int)ordinate);
            var points = Flatten().CoordinateSequence;
            if ((points.Ordinates & ordinateFlag) != ordinateFlag)
                return CreateArray(points.Count, Coordinate.NullOrdinate);

            return CreateArray(points, ordinate);
        }

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
        /// <returns><c>true</c> if <c>pt</c> is one of this <c>LineString</c>'s vertices.</returns>
        public virtual bool IsCoordinate(Coordinate pt)
        {
            var points = Flatten().CoordinateSequence;
            for (int i = 0; i < points.Count; i++)
                if (points.GetCoordinate(i).Equals(pt))
                    return true;
            return false;
        }

        protected override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            var curve = o as CurvedGeometry<LineString>;
            var line = o as LineString;
            if (curve == null && line == null)
                Assert.ShouldNeverReachHere("CurvedGeometry<LineString> or LineString type expected!");

            if (curve != null)
                line = curve.Flatten();

            return comp.Compare(Flatten().CoordinateSequence, line.CoordinateSequence);
        }

        public override string ToText()
        {
            var writer = new WKTWriterEx(3);
            return writer.Write(this);
        }
    }
}
