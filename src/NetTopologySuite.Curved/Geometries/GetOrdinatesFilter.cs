using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Utility filter class to extract ordinate values from a geometry
    /// </summary>
    class GetOrdinatesFilter : IEntireCoordinateSequenceFilter
    {
        private readonly Ordinate _ordinate;
        private readonly List<double> _ordinates;

        /// <summary>
        /// Creates an instance of this class to filter <paramref name="ordinate"/> from the <see cref="CoordinateSequence"/>s
        /// </summary>
        /// <param name="ordinate">The ordinate to gather values from.</param>
        public GetOrdinatesFilter(Ordinate ordinate) : this(ordinate, 12) { }

        /// <summary>
        /// Creates an instance of this class to filter <paramref name="ordinate"/> from the <see cref="CoordinateSequence"/>s.
        /// A total of <paramref name="capacity"/> ordinate values are expected.
        /// </summary>
        /// <param name="ordinate">The ordinate to gather values from.</param>
        /// <param name="capacity">The initial capacity of the ordinate list</param>
        public GetOrdinatesFilter(Ordinate ordinate, int capacity)
        {
            _ordinate = ordinate;
            _ordinates = new List<double>();
        }

        public bool Done => false;

        public bool GeometryChanged => false;

        public void Filter(CoordinateSequence seq)
        {
            for (int i = 0; i < seq.Count; i++)
                _ordinates.Add(seq.GetOrdinate(i, _ordinate));
        }

        /// <summary>
        /// Gets a value indicating the array of gathered ordinate values
        /// </summary>
        public double[] Ordinates
        {
            get => _ordinates.ToArray();
        }
    }
}
