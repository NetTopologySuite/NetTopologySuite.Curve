using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public sealed class CircularString : Curve, ILinearizable<LineString>
    {
        private CoordinateSequence _controlPoints;
        
        internal CircularString(CoordinateSequence points, CurveGeometryFactory factory)
            : base(factory)
        {
            _controlPoints = points;
        }

        #region CircularString specific

        /// <summary>
        /// Gets a value indicating the control points of this <c>CircularString</c>.
        /// </summary>
        public CoordinateSequence ControlPoints
        {
            get => _controlPoints;
        }

        /// <summary>
        /// Gets a value indicating the number of <c>CircularArc</c>s.
        /// </summary>
        public int NumArcs { get => ControlPoints.Count == 0 ? 0 : (ControlPoints.Count - 1) / 2; }

        /// <summary>
        /// Gets the <c>CircularArc</c> at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the arc</param>
        /// <returns>A <c>CircularArc</c></returns>
        public CircularArc GetArcN(int index)
        {
            if (index < NumArcs)
                return new CircularArc(ControlPoints, index * 2);
            throw new ArgumentOutOfRangeException(nameof(index), $"Must be less than {NumArcs}");
        }

        #endregion

        #region ILinearize{T}

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
        private LineString Linearized { get; set; }

        /// <summary>
        /// Linearize this curve geometry. The default arc segment length is used.
        /// </summary>
        /// <returns>A <c>LineString</c></returns>
        public LineString Linearize()
        {
            return Linearize(ArcSegmentLength);
        }

        /// <summary>
        /// Linearize this curve geometry using the provided arc segment length.
        /// </summary>
        /// <param name="arcSegmentLength">The length of arc segments</param>
        /// <returns>A flattened geometry</returns>
        public LineString Linearize(double arcSegmentLength)
        {
            if (arcSegmentLength < 0d)
                throw new ArgumentOutOfRangeException(nameof(arcSegmentLength), "must be positive!");

            var linearized = Linearized;
            if (arcSegmentLength == ArcSegmentLength && linearized != null)
                return linearized;

            if (ControlPoints.Count == 0)
                linearized = Factory.CreateLineString();
            else
            {

                var cl = new CoordinateList {ControlPoints.GetCoordinate(0)};
                var caIt = new CircularArcEnumerator(ControlPoints);
                var pm = Factory.PrecisionModel;
                while (caIt.MoveNext())
                {
                    var ca = caIt.Current;
                    if (ca == null) continue;

                    var itms = ca.Flatten(arcSegmentLength, pm);
                    cl.AddRange(itms.Skip(1));
                }

                linearized = Factory.CreateLineString(cl.ToCoordinateArray());
            }

            if (arcSegmentLength == ArcSegmentLength)
                Linearized = linearized;

            return linearized;
        }

        #endregion

        #region Curve implementation

        /// <inheritdoc cref="Curve.IsClosed"/>
        public override bool IsClosed
        {
            get
            {
                if (IsEmpty)
                    return false;
                return ControlPoints.First().Equals2D(ControlPoints.Last());
            }
        }

        /// <inheritdoc cref="Curve.StartPoint"/>
        public override Point StartPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return Factory.CreatePoint(ControlPoints.First());
            }
        }

        /// <inheritdoc cref="Curve.EndPoint"/>
        public override Point EndPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return Factory.CreatePoint(ControlPoints.Last());
            }
        }

        #endregion

        #region Geometry overloads

        /// <inheritdoc cref="Geometry.SortIndex"/>
        protected override SortIndexValue SortIndex
        {
            get { return IsRing ? SortIndexValue.LinearRing : SortIndexValue.LineString; }
        }

        /// <inheritdoc cref="Geometry.Coordinates"/>
        public override Coordinate[] Coordinates => Linearize().Coordinates;

        /// <inheritdoc cref="Geometry.GetOrdinates"/>
        public override double[] GetOrdinates(Ordinate ordinate)
        {
            var filter = new GetOrdinatesFilter(ordinate, NumPoints);
            Apply(filter);
            return filter.Ordinates;
        }

        /// <inheritdoc cref="Geometry.NumPoints"/>
        public override int NumPoints => Linearize().NumPoints;

        /// <inheritdoc cref="Geometry.Boundary"/>
        public override Geometry Boundary => Linearize().Boundary;

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
            if (_controlPoints.Count == 0)
                return;

            for (int i = 0; i < _controlPoints.Count; i++)
            {
                filter.Filter(_controlPoints, i);
                if (filter.Done)
                    break;
            }

            if (!filter.GeometryChanged)
                return;

            GeometryChanged();
            Linearized = null;
        }

        /// <inheritdoc cref="Geometry.Apply(IEntireCoordinateSequenceFilter)"/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (_controlPoints.Count == 0)
                return;

            filter.Filter(_controlPoints);

            if (!filter.GeometryChanged)
                return;

            GeometryChanged();
            Linearized = null;
        }

        /// <inheritdoc cref="Geometry.IsEquivalentClass"/>
        protected override bool IsEquivalentClass(Geometry other)
        {
            return other is ILinearizable<LineString> || other is LineString;
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
            Apply(new NewLinearizedGeometry<LineString>(Linearized));
        }

        #endregion

        /// <inheritdoc cref="Geometry.Length"/>
        public override double Length
        {
            get
            {
                double res = 0d;
                var it = new CircularArcEnumerator(ControlPoints);
                while (it.MoveNext())
                    res += it.Current?.Length ?? 0d;
                return res;
            }
        }

        /// <inheritdoc cref="IsEmpty"/>
        public override bool IsEmpty
        {
            get => ControlPoints.Count == 0;
        }

        /// <inheritdoc cref="Geometry.ReverseInternal"/>
        protected override Geometry ReverseInternal()
        {
            var seq = _controlPoints.Reversed();
            return new CircularString(seq, (CurveGeometryFactory)Factory);
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType => CurveGeometry.TypeNameCircularString;

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.CircularString;

        /// <inheritdoc cref="Geometry.InteriorPoint"/>
        public override Point InteriorPoint
        {
            get
            {
                return IsEmpty
                    ? Factory.CreatePoint()
                    : Factory.CreatePoint(ControlPoints.GetCoordinate(ControlPoints.Count / 2));
            }
        }

        /// <inheritdoc cref="Geometry.ComputeEnvelopeInternal"/>
        protected override Envelope ComputeEnvelopeInternal()
        {
            var env = new Envelope();
            var caIt = new CircularArcEnumerator(ControlPoints);
            while (caIt.MoveNext())
            {
                var ca = caIt.Current;
                if (ca == null) continue;
                env.ExpandToInclude(ca.Envelope);
            }

            return env;
        }

        /// <inheritdoc cref="Geometry.CompareToSameClass(object)"/>
        protected override int CompareToSameClass(object o)
        {
            if (!(o is Curve))
                throw new ArgumentException("Not a Curve", nameof(o));

            if (o is CircularString cs)
            {
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    int comparison = _controlPoints.GetCoordinate(i).CompareTo(cs.ControlPoints.GetCoordinate(i));
                    if (comparison != 0)
                        return comparison;
                }

                return _controlPoints.Count.CompareTo(cs.ControlPoints.Count);
            }

            if (o is CompoundCurve cc)
                return Linearize().CompareTo(cc.Linearize());

            if (o is LineString ls)
                return Linearize().CompareTo(ls);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected override int CompareToSameClass(object o, IComparer<CoordinateSequence> comparer)
        {
            if (!(o is Curve))
                throw new ArgumentException("Not a Curve", nameof(o));

            if (o is CircularString cs)
            {
                for (int i = 0; i < _controlPoints.Count; i++)
                {
                    int comparison = comparer.Compare(_controlPoints, cs.ControlPoints);
                    if (comparison != 0)
                        return comparison;
                }

                return _controlPoints.Count.CompareTo(cs.ControlPoints.Count);
            }

            if (o is CompoundCurve cc)
                return Linearize().CompareTo(cc.Linearize());

            if (o is LineString ls)
                return Linearize().CompareTo(ls);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="EqualsExact"/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (other is CircularString cs)
            {
                if (ControlPoints.Count != cs.ControlPoints.Count)
                    return false;

                for (int i = 0; i < ControlPoints.Count; i++)
                {
                    var p1 = ControlPoints.GetCoordinate(i);
                    var p2 = cs.ControlPoints.GetCoordinate(i);
                    if (!p1.Equals2D(p2, tolerance))
                        return false;
                }

                return true;
            }

            return Linearize().EqualsExact(other, tolerance);
        }

        /// <inheritdoc cref="CopyInternal"/>
        protected override Geometry CopyInternal()
        {
            var seq = ControlPoints.Copy();
            var res = new CircularString(seq, (CurveGeometryFactory)Factory);
            res.Linearized = (LineString)Linearized?.Copy();
            return res;
        }


        /// <inheritdoc cref="Coordinate"/>
        public override Coordinate Coordinate
        {
            get => _controlPoints.GetCoordinate(0);
        }


        private class CircularArcEnumerator : IEnumerator<CircularArc>
        {
            private int _startOffset;
            private CircularArc _current;
            private readonly CoordinateSequence _sequence;

            public CircularArcEnumerator(CoordinateSequence sequence)
            {
                _sequence = sequence;
                _startOffset = -1;
                _current = null;
            }

            public bool MoveNext()
            {
                if (_startOffset < 0)
                    _startOffset = 0;

                if (_startOffset + 2 >= _sequence.Count)
                    return false;

                _current = new CircularArc(_sequence, _startOffset);

                _startOffset += 2;
                return true;
            }

            public void Reset()
            {
                _startOffset = -1;
                _current = null;
            }

            public CircularArc Current
            {
                get => _current;
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }
        }
    }
}
