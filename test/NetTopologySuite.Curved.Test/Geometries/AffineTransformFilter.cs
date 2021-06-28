using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Test.Geometries
{
    public class AffineTransformationFilter1 : ICoordinateSequenceFilter
    {
        private readonly AffineTransformation _at;

        public AffineTransformationFilter1(AffineTransformation at)
        {
            _at = at;
        }


        public void Filter(CoordinateSequence seq, int i)
        {
            _at.Transform(seq, i);
        }

        public bool Done => false;

        public bool GeometryChanged
        {
            get => !_at.IsIdentity;
        }
    }
    public class AffineTransformationFilter2 : IEntireCoordinateSequenceFilter
    {
        private readonly AffineTransformation _at;

        public AffineTransformationFilter2(AffineTransformation at)
        {
            _at = at;
        }


        public void Filter(CoordinateSequence seq)
        {
            for (int i = 0; i < seq.Count; i++)
                _at.Transform(seq, i);
        }

        public bool Done => false;

        public bool GeometryChanged
        {
            get => !_at.IsIdentity;
        }
    }
}
