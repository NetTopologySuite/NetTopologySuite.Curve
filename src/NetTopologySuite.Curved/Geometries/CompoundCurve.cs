using System.Collections.Generic;
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

        protected override LineString FlattenInternal(double arcSegmentLength)
        {
            var ls = new Operation.Linemerge.LineSequencer();
            for (int i = 0; i < _geometries.Length; i++)
            {
                if (_geometries[i] is CircularString curve)
                    ls.Add(curve.Flatten());
                else if (_geometries[i] is LineString line)
                    ls.Add(line);
                else
                    Assert.ShouldNeverReachHere("Invalid geometry in CompoundCurve");
            }

            Assert.IsTrue(ls.IsSequenceable());
            return (LineString)ls.GetSequencedLineStrings();
        }
    }
}
