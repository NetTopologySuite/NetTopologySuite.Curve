using System;
using System.Collections.Generic;

namespace NetTopologySuite.Geometries
{
    public class CurvedPolygon : CurvedGeometry<Polygon>
    {
        public CurvedPolygon(CompoundCurve exteriorRing, CurvedGeometryFactory factory, double arcSegmentLength)
            : this(exteriorRing, new CompoundCurve[0], factory, arcSegmentLength)
        {
        }

        internal CurvedPolygon(CompoundCurve exteriorRing, CompoundCurve[] interiorRings, CurvedGeometryFactory factory, double arcSegmentLength)
            : base(factory, arcSegmentLength)
        {
            if (interiorRings == null)
                interiorRings = new CompoundCurve[0];

            if (!exteriorRing.IsRing)
                throw new ArgumentException("Not a ring", nameof(exteriorRing));

            ExteriorRing = exteriorRing;

            for (int i = 0; i < interiorRings.Length; i++)
            {
                if (!interiorRings[i].IsRing)
                    throw new ArgumentException($"Not a ring in interiorRings at {i}", nameof(interiorRings));
            }

            InteriorRings = interiorRings;
        }


        public CompoundCurve ExteriorRing { get; }

        public IReadOnlyList<CompoundCurve> InteriorRings { get; }

        public override double[] GetOrdinates(Ordinate ordinate)
        {
            throw new System.NotImplementedException();
        }

        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (other is CurvedPolygon cp)
            {
                if (!ExteriorRing.EqualsExact(cp.ExteriorRing, tolerance))
                    return false;

                if (InteriorRings.Count != cp.InteriorRings.Count)
                    return false;

                for (int i = 0; i < InteriorRings.Count; i++)
                    if (!InteriorRings[i].EqualsExact(cp.InteriorRings[i]))
                        return false;
            }

            return Flatten().EqualsExact(other);
        }

        protected override Geometry CopyInternal()
        {
            var interiorRings = new CompoundCurve[InteriorRings.Count];
            for (int i = 0; i < InteriorRings.Count; i++)
                interiorRings[i] = (CompoundCurve)InteriorRings[i].Copy();

            var res = new CurvedPolygon((CompoundCurve)ExteriorRing.Copy(), interiorRings, (CurvedGeometryFactory)Factory, ArcSegmentLength);
            return res;
        }

        protected override Envelope ComputeEnvelopeInternal()
        {
            return ExteriorRing.EnvelopeInternal;
        }

        protected override int CompareToSameClass(object o, IComparer<CoordinateSequence> comp)
        {
            throw new System.NotImplementedException();
        }

        public override string GeometryType
        {
            get => CurvedGeometry.TypeNameMultiCurve;
        }
        public override OgcGeometryType OgcGeometryType
        {
            get => OgcGeometryType.MultiCurve;
        }
        public override Coordinate Coordinate
        {
            get => ExteriorRing.Coordinate;
        }

        public override bool IsEmpty
        {
            get => ExteriorRing.IsEmpty;
        }

        public override Dimension Dimension { get; }

        public override Dimension BoundaryDimension { get; }

        protected override Polygon FlattenInternal(double arcSegmentLength)
        {
            var flattenedShell = ToRing(ExteriorRing.Flatten(arcSegmentLength));
            var flattenedHoles = new LinearRing[InteriorRings.Count];
            for (int i = 0; i < InteriorRings.Count; i++)
                flattenedHoles[i] = ToRing(InteriorRings[i].Flatten(arcSegmentLength));

            return Factory.CreatePolygon(flattenedShell, flattenedHoles);
        }

        private static LinearRing ToRing(LineString lineString)
        {
            return lineString.Factory.CreateLinearRing(lineString.CoordinateSequence);
        }
    }
}
