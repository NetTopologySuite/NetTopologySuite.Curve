using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries
{
    public class CompoundCurve : CurvedLineString
    {
        private readonly Geometry[] _geometries;

        internal CompoundCurve(Geometry[] geometries, CurvedGeometryFactory factory, double arcSegmentLength)
            : base(factory, arcSegmentLength)
        {
            _geometries = geometries;
        }

        public IReadOnlyList<Geometry> Curves
        {
            get => _geometries;
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;

            if (other.NumGeometries != NumGeometries)
                return false;

            var cc = (CompoundCurve)other;
            for (int i = 0; i < NumGeometries; i++)
            {
                if (!Curves[i].EqualsExact(cc.Curves[i], tolerance))
                    return false;
            }

            return true;
        }

        protected override Geometry CopyInternal()
        {
            var res = new Geometry[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
                res[i] = _geometries[i].Copy();

            return new CompoundCurve(res, (CurvedGeometryFactory)Factory, ArcSegmentLength);
        }

        protected override Envelope ComputeEnvelopeInternal()
        {
            var env = new Envelope();
            for (int i = 0; i < _geometries.Length; i++)
                env.ExpandToInclude(_geometries[i].EnvelopeInternal);
            return env;
        }

        public override string GeometryType
        {
            get => CurvedGeometry.TypeNameCompoundCurve;
        }

        public override OgcGeometryType OgcGeometryType
        {
            get => OgcGeometryType.CompoundCurve;
        }

        public override Coordinate Coordinate { get => IsEmpty ? null : _geometries[0].Coordinates[0]; }

        public override bool IsEmpty
        {
            get => _geometries.Length == 0;
        }

        public override double Length
        {
            get
            {
                return _geometries.Sum(t => t.Length);
            }
        }

        protected override LineString FlattenInternal(double arcSegmentLength)
        {
            if (IsEmpty)
                return Factory.CreateLineString();

            var flattened = new LineString[_geometries.Length];
            int[] offset = new int[_geometries.Length];

            int numPoints = 0;
            Coordinate last = null;
            for (int i = 0; i < flattened.Length; i++)
            {
                flattened[i] = _geometries[i] is CircularString cs
                    ? cs.Flatten(arcSegmentLength)
                    : (LineString) _geometries[i];

                var sequence = flattened[i].CoordinateSequence;
                numPoints += flattened[i].NumPoints;
                if (last != null) {
                    if (last.Equals(sequence.First())) {
                        numPoints--;
                        offset[i] = 1;
                    } 
                }

                last = sequence.Last();
            }

            var seq = Factory.CoordinateSequenceFactory.Create(numPoints, flattened[0].CoordinateSequence.Ordinates);
            int tgtOffset = 0;
            for (int i = 0; i < flattened.Length; i++)
            {
                var tmp = flattened[i].CoordinateSequence;
                int count = tmp.Count - offset[i];
                CoordinateSequences.Copy(tmp, offset[i], seq, tgtOffset, count);
                tgtOffset += count;
            }

            return Factory.CreateLineString(seq);
        }
    }
}
