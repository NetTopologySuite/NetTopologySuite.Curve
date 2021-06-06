using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries
{
    [Serializable]
    public sealed class CircularString : CurvedLineString
    {
        private CoordinateSequence _controlPoints;
        
        internal CircularString(CoordinateSequence points, CurvedGeometryFactory factory, double arcSegmentLength)
            : base(factory, arcSegmentLength)
        {
            _controlPoints = points;
        }

        public CoordinateSequence ControlPoints
        {
            get => _controlPoints;
        }

        public int NumArcs { get => ControlPoints.Count == 0 ? 0 : (ControlPoints.Count - 1) / 2; }

        public CircularArc GetArc(int index)
        {
            if (index < NumArcs)
                return new CircularArc(ControlPoints, index * 2);
            throw new ArgumentOutOfRangeException(nameof(index), $"Must be less than {NumArcs}");
        }

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

        public override bool IsEmpty
        {
            get => ControlPoints.Count == 0;
        }

        protected override Geometry ReverseInternal()
        {
            var seq = _controlPoints.Reversed();
            return new CircularString(seq, (CurvedGeometryFactory)Factory, ArcSegmentLength);
        }

        public override string GeometryType => CurvedGeometry.TypeNameCircularString;

        public override OgcGeometryType OgcGeometryType => OgcGeometryType.CircularString;

        public override Point InteriorPoint
        {
            get
            {
                return IsEmpty
                    ? Point.Empty
                    : Factory.CreatePoint(ControlPoints.GetCoordinate(ControlPoints.Count / 2));
            }
        }

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

        protected override Geometry CopyInternal()
        {
            var seq = ControlPoints.Copy();
            var res = new CircularString(seq, (CurvedGeometryFactory)Factory, ArcSegmentLength);
            res.Flattened = (LineString)Flattened?.Copy();
            return res;
        }


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
