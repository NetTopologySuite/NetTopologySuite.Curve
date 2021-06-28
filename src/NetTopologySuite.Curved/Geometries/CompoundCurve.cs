using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A curved geometry made up of several <see cref="Curve"/>s.
    /// </summary>
    [Serializable]
    public sealed class CompoundCurve : Curve, ILinearizable<LineString>
    {
        private readonly Curve[] _geometries;

        internal CompoundCurve(Curve[] geometries, CurveGeometryFactory factory)
            : base(factory)
        {
            _geometries = geometries;
        }

        #region CompoundCurve specific

        /// <summary>
        /// Gets a list of the underlying <see cref="Curve"/> geometries.
        /// </summary>
        public IReadOnlyList<Curve> Curves
        {
            get => _geometries;
        }

        #endregion

        #region ILinearizeable{T}

        /// <summary>
        /// Gets a value indicating the default maximum length of arc segments that is
        /// used when flattening the curve.
        /// </summary>
        private double ArcSegmentLength
        {
            get => ((CurveGeometryFactory)Factory).ArcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the flattened geometry
        /// </summary>
        private LineString Linearized { get; set; }

        /// <summary>
        /// Flatten this curve geometry. The arc segment length used is <see cref="ArcSegmentLength"/>.
        /// </summary>
        /// <returns>A <c>LineString</c></returns>
        public LineString Linearize()
        {
            return Linearize(ArcSegmentLength);
        }

        /// <summary>
        /// Flatten this curve geometry using the provided arc segment length.
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


            if (IsEmpty)
                linearized = Factory.CreateLineString();
            else
            {
                var flattened = new LineString[_geometries.Length];
                int[] offset = new int[_geometries.Length];

                int numPoints = 0;
                Coordinate last = null;
                for (int i = 0; i < flattened.Length; i++)
                {
                    flattened[i] = _geometries[i] is CircularString cs
                        ? cs.Linearize(arcSegmentLength)
                        : (LineString) _geometries[i];

                    var sequence = flattened[i].CoordinateSequence;
                    numPoints += flattened[i].NumPoints;
                    if (last != null)
                    {
                        if (last.Equals(sequence.First()))
                        {
                            numPoints--;
                            offset[i] = 1;
                        }
                    }

                    last = sequence.Last();
                }

                var seq = Factory.CoordinateSequenceFactory.Create(numPoints,
                    flattened[0].CoordinateSequence.Ordinates);
                int tgtOffset = 0;
                for (int i = 0; i < flattened.Length; i++)
                {
                    var tmp = flattened[i].CoordinateSequence;
                    int count = tmp.Count - offset[i];
                    CoordinateSequences.Copy(tmp, offset[i], seq, tgtOffset, count);
                    tgtOffset += count;
                }

                linearized = Factory.CreateLineString(seq);
            }

            if (arcSegmentLength == ArcSegmentLength)
                Linearized = linearized;

            return linearized;
        }
        #endregion

        #region Curve implementation

        /// <inheritdoc cref="Curve.IsClosed"/>
        public override bool IsClosed => Linearize().IsClosed;

        
        /// <inheritdoc cref="Curve.StartPoint"/>
        public override Point StartPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return Curves[0].StartPoint;
            }
        }

        /// <inheritdoc cref="Curve.EndPoint"/>
        public override Point EndPoint
        {
            get
            {
                if (IsEmpty)
                    return null;
                return Curves[Curves.Count-1].EndPoint;
            }
        }

        #endregion

        /// <inheritdoc cref="Geometry.SortIndex"/>
        protected override SortIndexValue SortIndex
        {
            get { return IsRing ? SortIndexValue.LinearRing : SortIndexValue.LineString; }
        }

        /// <inheritdoc cref="Geometry.Centroid"/>
        public override Point Centroid => Linearize().Centroid;

        /// <inheritdoc cref="Geometry.InteriorPoint"/>
        public override Point InteriorPoint => Linearize().InteriorPoint;

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
            if (IsEmpty) return;

            for (int i = 0; i < _geometries.Length; i++)
            {
                _geometries[i].Apply(filter);
                if (filter.Done) break;
            }

            if (!filter.GeometryChanged)
                return;

            GeometryChanged();
            Linearized = null;
        }

        /// <inheritdoc cref="Geometry.Apply(IEntireCoordinateSequenceFilter)"/>
        public override void Apply(IEntireCoordinateSequenceFilter filter)
        {
            if (IsEmpty) return;

            for (int i = 0; i < _geometries.Length; i++)
            {
                _geometries[i].Apply(filter);
                if (filter.Done) break;
            }

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

        /// <inheritdoc cref="Geometry.EqualsExact(Geometry, double)"/>
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentClass(other))
                return false;

            if (other.NumGeometries != NumGeometries)
                return false;

            var cc = (CompoundCurve)other;
            for (int i = 0; i < Curves.Count; i++)
            {
                if (!Curves[i].EqualsExact(cc.Curves[i], tolerance))
                    return false;
            }

            return true;
        }

        /// <inheritdoc cref="Geometry.CopyInternal"/>
        protected override Geometry CopyInternal()
        {
            var res = new Curve[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
                res[i] = (Curve)_geometries[i].Copy();

            return new CompoundCurve(res, (CurveGeometryFactory)Factory);
        }

        /// <inheritdoc cref="Geometry.ComputeEnvelopeInternal"/>
        protected override Envelope ComputeEnvelopeInternal()
        {
            var env = new Envelope();
            for (int i = 0; i < _geometries.Length; i++)
                env.ExpandToInclude(_geometries[i].EnvelopeInternal);
            return env;
        }

        /// <inheritdoc cref="Geometry.CompareToSameClass(object)"/>
        protected override int CompareToSameClass(object o)
        {
            if (!(o is Curve))
                throw new ArgumentException("Not a Curve", nameof(o));

            if (o is CompoundCurve cc)
            {
                int minNumComponents = Math.Min(_geometries.Length, cc._geometries.Length);
                for (int i = 0; i < minNumComponents; i++)
                {
                    int comparison = _geometries[i].CompareTo(cc._geometries[i]);
                    if (comparison != 0)
                        return comparison;
                }

                return _geometries.Length.CompareTo(cc._geometries.Length);
            }

            if (o is CircularString cs)
                return Linearize().CompareTo(cs.Linearize());

            if (o is LineString ls)
                return Linearize().CompareTo(ls);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected override int CompareToSameClass(object o, IComparer<CoordinateSequence> comparer)
        {
            if (!(o is Curve))
                throw new ArgumentException("Not a Curve", nameof(o));

            if (o is CompoundCurve cc)
            {
                int minNumComponents = Math.Min(_geometries.Length, cc._geometries.Length);
                for (int i = 0; i < minNumComponents; i++)
                {
                    int comparison = _geometries[i].CompareTo(cc._geometries[i], comparer);
                    if (comparison != 0)
                        return comparison;
                }

                return _geometries.Length.CompareTo(cc._geometries.Length);
            }

            if (o is CircularString cs)
                return Linearize().CompareTo(cs.Linearize(), comparer);

            if (o is LineString ls)
                return Linearize().CompareTo(ls, comparer);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="Geometry.GeometryType"/>
        public override string GeometryType
        {
            get => CurveGeometry.TypeNameCompoundCurve;
        }

        /// <inheritdoc cref="Geometry.OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType
        {
            get => OgcGeometryType.CompoundCurve;
        }

        /// <inheritdoc cref="Geometry.Coordinate"/>
        public override Coordinate Coordinate { get => IsEmpty ? null : _geometries[0].Coordinate; }

        /// <inheritdoc cref="Geometry.IsEmpty"/>
        public override bool IsEmpty
        {
            get => _geometries.Length == 0;
        }

        /// <inheritdoc cref="Geometry.Length"/>
        public override double Length
        {
            get
            {
                return _geometries.Sum(t => t.Length);
            }
        }
    }
}
