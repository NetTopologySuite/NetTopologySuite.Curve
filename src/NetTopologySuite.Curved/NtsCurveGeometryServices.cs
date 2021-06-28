using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite
{
    /// <summary>
    /// A geometry service provider class that supports curved geometries.
    /// </summary>
    public class NtsCurveGeometryServices : NtsGeometryServices
    {
        /// <summary>
        /// Creates a new instance of this class using the provided arguments.
        /// </summary>
        /// <remarks>
        /// The <see cref="GeometryOverlay"/> argument from <see cref="NtsCurveGeometryServices"/> constructor is set internally to
        /// <see cref="CurveGeometryOverlay.CurveV2"/>.
        /// </remarks>
        /// <param name="coordinateSequenceFactory">A coordinate sequence factory</param>
        /// <param name="precisionModel">A precision model</param>
        /// <param name="srid">A spatial reference identifier</param>
        /// <param name="coordinateEqualityComparer">A coordinate equality comparer</param>
        /// <param name="defaultArcSegmentLength">An arc segment length value that is used to flatten curved geometries. Must be positive.</param>
        public NtsCurveGeometryServices(CoordinateSequenceFactory coordinateSequenceFactory,
            PrecisionModel precisionModel, int srid, 
            CoordinateEqualityComparer coordinateEqualityComparer, double defaultArcSegmentLength)
            : base(coordinateSequenceFactory, precisionModel, srid, CurveGeometryOverlay.CurveV2, coordinateEqualityComparer,
                t => new WKTReaderEx((NtsCurveGeometryServices)t), t => new WKTWriterEx(3),
                t => new WKBReaderEx((NtsCurveGeometryServices)t), t => new WKBWriterEx())
        {
            if (defaultArcSegmentLength < 0d)
                throw new ArgumentOutOfRangeException($"Must not be negative", nameof(defaultArcSegmentLength));

            DefaultArcSegmentLength = defaultArcSegmentLength;
        }

        /// <summary>
        /// Gets a value indicating the default arc segment length that is used to flatten curved geometries.
        /// </summary>
        private double DefaultArcSegmentLength { get; }

        /// <inheritdoc cref="CreateGeometryFactoryCore"/>
        protected override GeometryFactory CreateGeometryFactoryCore(PrecisionModel precisionModel, int srid,
            CoordinateSequenceFactory coordinateSequenceFactory)
        {
            return new CurveGeometryFactory(precisionModel, srid, coordinateSequenceFactory, this,
                DefaultArcSegmentLength);
        }
    }
}
