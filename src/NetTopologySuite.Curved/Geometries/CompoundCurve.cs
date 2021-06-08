using System;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A curved geometry made up of several <see cref="ICurve"/>s.
    /// </summary>
    public class CompoundCurve : CurvedLineString
    {
        private readonly Geometry[] _geometries;

        internal CompoundCurve(Geometry[] geometries, CurvedGeometryFactory factory, double arcSegmentLength)
            : base(factory, arcSegmentLength)
        {
            _geometries = geometries;
        }

        /// <summary>
        /// Gets a list of the underlying <see cref="ICurve"/> geometries.
        /// </summary>
        public IReadOnlyList<Geometry> Curves
        {
            get => _geometries;
        }

        /// <inheritdoc cref="EqualsExact"/>
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

        /// <inheritdoc cref="CopyInternal"/>
        protected override Geometry CopyInternal()
        {
            var res = new Geometry[NumGeometries];
            for (int i = 0; i < NumGeometries; i++)
                res[i] = _geometries[i].Copy();

            return new CompoundCurve(res, (CurvedGeometryFactory)Factory, ArcSegmentLength);
        }

        /// <inheritdoc cref="ComputeEnvelopeInternal"/>
        protected override Envelope ComputeEnvelopeInternal()
        {
            var env = new Envelope();
            for (int i = 0; i < _geometries.Length; i++)
                env.ExpandToInclude(_geometries[i].EnvelopeInternal);
            return env;
        }

        /// <inheritdoc cref="CompareToSameClass(object)"/>
        protected internal override int CompareToSameClass(object o)
        {
            if (!(o is ICurve))
                throw new ArgumentException("Not a Curve", nameof(o));

            if (o is CompoundCurve cc)
            {
                int minNumComponents = Math.Min(_geometries.Length, cc._geometries.Length);
                for (int i = 0; i < minNumComponents; i++)
                {
                    int comparison = _geometries[i].CompareToSameClass(cc._geometries[i]);
                    if (comparison != 0)
                        return comparison;
                }

                return _geometries.Length.CompareTo(cc._geometries.Length);
            }

            if (o is CircularString cs)
                return Flatten().CompareToSameClass(cs.Flatten());

            if (o is LineString ls)
                Flatten().CompareToSameClass(ls);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="CompareToSameClass(object, IComparer{CoordinateSequence})"/>
        protected internal override int CompareToSameClass(object o, IComparer<CoordinateSequence> comparer)
        {
            if (!(o is ICurve))
                throw new ArgumentException("Not a Curve", nameof(o));

            if (o is CompoundCurve cc)
            {
                int minNumComponents = Math.Min(_geometries.Length, cc._geometries.Length);
                for (int i = 0; i < minNumComponents; i++)
                {
                    int comparison = _geometries[i].CompareToSameClass(cc._geometries[i], comparer);
                    if (comparison != 0)
                        return comparison;
                }

                return _geometries.Length.CompareTo(cc._geometries.Length);
            }

            if (o is CircularString cs)
                return Flatten().CompareToSameClass(cs.Flatten(), comparer);

            if (o is LineString ls)
                Flatten().CompareToSameClass(ls, comparer);

            throw new ArgumentException("Invalid type", nameof(o));
        }

        /// <inheritdoc cref="GeometryType"/>
        public override string GeometryType
        {
            get => CurvedGeometry.TypeNameCompoundCurve;
        }

        /// <inheritdoc cref="OgcGeometryType"/>
        public override OgcGeometryType OgcGeometryType
        {
            get => OgcGeometryType.CompoundCurve;
        }

        /// <inheritdoc cref="Coordinate"/>
        public override Coordinate Coordinate { get => IsEmpty ? null : _geometries[0].Coordinate; }

        /// <inheritdoc cref="IsEmpty"/>
        public override bool IsEmpty
        {
            get => _geometries.Length == 0;
        }

        /// <inheritdoc cref="Length"/>
        public override double Length
        {
            get
            {
                return _geometries.Sum(t => t.Length);
            }
        }

        /// <inheritdoc cref="FlattenInternal"/>
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
