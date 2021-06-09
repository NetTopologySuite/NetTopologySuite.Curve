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
    public sealed class CircularString : CurvedLineString
    {
        private CoordinateSequence _controlPoints;
        
        internal CircularString(CoordinateSequence points, CurvedGeometryFactory factory)
            : base(factory)
        {
            _controlPoints = points;
        }

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

        /// <inheritdoc cref="FlattenInternal"/>
        protected override LineString FlattenInternal(double arcSegmentLength)
        {
            if (ControlPoints.Count == 0)
                return Factory.CreateLineString();

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

            return Factory.CreateLineString(cl.ToCoordinateArray());
        }

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

        /// <inheritdoc cref="ReverseInternal"/>
        protected override Geometry ReverseInternal()
        {
            var seq = _controlPoints.Reversed();
            return new CircularString(seq, (CurvedGeometryFactory)Factory);
        }

        /// <inheritdoc cref="GeometryType"/>
        public override string GeometryType => CurvedGeometry.TypeNameCircularString;

        /// <inheritdoc cref="OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType => OgcGeometryType.CircularString;

        /// <inheritdoc cref="InteriorPoint"/>
        public override Point InteriorPoint
        {
            get
            {
                return IsEmpty
                    ? Factory.CreatePoint()
                    : Factory.CreatePoint(ControlPoints.GetCoordinate(ControlPoints.Count / 2));
            }
        }

        /// <inheritdoc cref="ComputeEnvelopeInternal"/>
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

        /// <inheritdoc cref="CompareToSameClass(object)"/>
        protected internal override int CompareToSameClass(object o)
        {
            if (!(o is ICurve))
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
                return Flatten().CompareToSameClass(cc.Flatten());

            if (o is LineString ls)
                Flatten().CompareToSameClass(ls);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comparer)
        {
            if (!(o is ICurve))
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
                return Flatten().CompareToSameClass(cc.Flatten());

            if (o is LineString ls)
                Flatten().CompareToSameClass(ls);

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

            return Flatten().EqualsExact(other, tolerance);
        }

        /// <inheritdoc cref="CopyInternal"/>
        protected override Geometry CopyInternal()
        {
            var seq = ControlPoints.Copy();
            var res = new CircularString(seq, (CurvedGeometryFactory)Factory);
            res.Flattened = (LineString)Flattened?.Copy();
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
