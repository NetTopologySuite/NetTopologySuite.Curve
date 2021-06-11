using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// Outputs the textual representation of a <see cref="Geometry" />.
    /// The <see cref="WKTWriter" /> outputs coordinates rounded to the precision
    /// model. No more than the maximum number of necessary decimal places will be
    /// output.
    /// The Well-known Text format is defined in the <A
    /// HREF="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
    /// Specification for SQL</A>.
    /// <para>
    /// <remarks>This implementation additionally handles
    /// <list type="bullet">
    /// <item><description><c>CircularString</c>,</description></item>
    /// <item><description><c>CompoundCurve</c>,</description></item>
    /// <item><description><c>CurvePolygon</c>,</description></item>
    /// <item><description><c>MultiCurve</c> and</description></item>
    /// <item><description><c>MultiSurface</c></description></item>
    /// </list></remarks>
    /// </para>
    /// </summary>
    public class WKTWriterEx : WKTWriter
    {
        public WKTWriterEx ForSqlServer()
        {
            return new WKTWriterEx(4, true);
        }

        /// <summary>
        /// Creates an instance of this class which is writing at most 2 dimensions.
        /// </summary>
        public WKTWriterEx()
            : this(2)
        {
        }

        /// <summary>
        /// Creates an instance of this class which is writing at most <paramref name="outputDimension"/> dimensions.
        /// </summary>
        public WKTWriterEx(int outputDimension)
            : base(outputDimension, false)
        {
        }

        /// <summary>
        /// Creates an instance of this class which is writing at most
        /// <paramref name="outputDimension"/> dimensions.
        /// </summary>
        /// <param name="outputDimension">Number of dimensions written</param>
        /// <param name="mssql">A flag indicating if SQLServer WKT should be written</param>
        protected WKTWriterEx(int outputDimension, bool mssql)
            : base(outputDimension, mssql)
        {
        }

        /// <inheritdoc cref="WKTWriter.AppendOtherGeometryTaggedText"/>
        /// <remarks>This implementation additionally handles
        /// <list type="bullet">
        /// <item><description><c>CircularString</c>,</description></item>
        /// <item><description><c>CompoundCurve</c>,</description></item>
        /// <item><description><c>CurvePolygon</c>,</description></item>
        /// <item><description><c>MultiCurve</c> and</description></item>
        /// <item><description><c>MultiSurface</c></description></item>
        /// </list></remarks>
        protected override bool AppendOtherGeometryTaggedText(Geometry geometry, Ordinates outputOrdinates, bool topLevel, bool useFormatting, int level,
            TextWriter writer, OrdinateFormat ordinateFormat)
        {
            switch (geometry)
            {
                case CircularString cs:
                    AppendCircularStringTaggedText(cs, outputOrdinates, topLevel, useFormatting, level, false, writer, ordinateFormat);
                    return true;
                case CompoundCurve cc:
                    AppendCompoundCurveTaggedText(cc, outputOrdinates, topLevel, useFormatting, level, false, writer, ordinateFormat);
                    return true;
                case CurvePolygon cp:
                    AppendCurvePolygonTaggedText(cp, outputOrdinates, topLevel, useFormatting, level, false, writer, ordinateFormat);
                    return true;
                case MultiCurve mc:
                    AppendMultiCurveTaggedText(mc, outputOrdinates, topLevel, useFormatting, level, writer, ordinateFormat);
                    return true;
                case MultiSurface ms:
                    AppendMultiSurfaceTaggedText(ms, outputOrdinates, topLevel, useFormatting, level, writer, ordinateFormat);
                    return true;
            }

            return false;
        }

        private void AppendCircularStringTaggedText(CircularString cs, Ordinates outputOrdinates, bool topLevel, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"CIRCULARSTRING ");
            if (topLevel) AppendOrdinateText(outputOrdinates, writer);
            AppendSequenceText(cs.ControlPoints, outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
        }

        private void AppendCompoundCurveTaggedText(CompoundCurve cc, Ordinates outputOrdinates, bool topLevel, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"COMPOUNDCURVE ");
            if (topLevel) AppendOrdinateText(outputOrdinates, writer);

            if (cc.IsEmpty)
            {
                writer.Write(WKTConstants.EMPTY);
                return;
            }

            // Write curves
            writer.Write("(");
            for (int i = 0; i < cc.Curves.Count; i++)
            {
                if (i > 0) writer.Write(", ");
                AppendCurveText(cc.Curves[i], outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
            }
            writer.Write(")");
        }

        private void AppendCurveText(Geometry curve, Ordinates outputOrdinates, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            switch (curve)
            {
                case LineString ls:
                    AppendSequenceText(ls.CoordinateSequence, outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
                    break;
                case CircularString cs:
                    AppendCircularStringTaggedText(cs, outputOrdinates, false, useFormatting, level, indentFirst, writer, ordinateFormat);
                    break;
                case CompoundCurve cc:
                    AppendCompoundCurveTaggedText(cc, outputOrdinates, false, useFormatting, level, indentFirst, writer, ordinateFormat);
                    break;
                default:
                    Assert.ShouldNeverReachHere($"Invalid geometry type for curve: {curve.GeometryType}");
                    break;

            }

        }
        private void AppendCurvePolygonTaggedText(CurvePolygon cp, Ordinates outputOrdinates, bool topLevel, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"CURVEPOLYGON ");
            if (topLevel) AppendOrdinateText(outputOrdinates, writer);

            if (cp.IsEmpty)
            {
                writer.Write(WKTConstants.EMPTY);
                return;
            }

            writer.Write("(");
            AppendCurveText(cp.ExteriorRing, outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);

            for (int i = 0; i < cp.NumInteriorRings; i++)
            {
                writer.Write(", ");
                AppendCurveText(cp.GetInteriorRingN(i), outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
            }
            writer.Write(")");
        }

        private void AppendMultiCurveTaggedText(MultiCurve mc, Ordinates outputOrdinates, bool topLevel, bool useFormatting, int level, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"MULTICURVE ");
            if (topLevel) AppendOrdinateText(outputOrdinates, writer);

            if (mc.IsEmpty)
            {
                writer.Write(WKTConstants.EMPTY);
                return;
            }

            int level2 = level;
            bool doIndent = false;
            writer.Write("(");
            for (int i = 0; i < mc.NumGeometries; i++)
            {
                if (i > 0)
                {
                    writer.Write(", ");
                    level2 = level + 1;
                    doIndent = true;
                }
                var testGeom = mc.GetGeometryN(i);
                AppendCurveText(testGeom, outputOrdinates, useFormatting, level2, doIndent, writer, ordinateFormat);
            }
            writer.Write(")");
        }

        private void AppendMultiSurfaceTaggedText(MultiSurface ms, Ordinates outputOrdinates, bool topLevel, bool useFormatting, int level, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"MULTISURFACE ");
            if (topLevel) AppendOrdinateText(outputOrdinates, writer);

            if (ms.IsEmpty)
            {
                writer.Write(WKTConstants.EMPTY);
                return;
            }

            int level2 = level;
            bool doIndent = false;
            writer.Write("(");
            for (int i = 0; i < ms.NumGeometries; i++)
            {
                if (i > 0)
                {
                    writer.Write(", ");
                    level2 = level + 1;
                    doIndent = true;
                }
                var testGeom = ms.GetGeometryN(i);
                if (testGeom is Polygon p)
                    AppendPolygonText(p, outputOrdinates, useFormatting, level2, doIndent, writer, ordinateFormat);
                else if (testGeom is CurvePolygon cp)
                    AppendCurvePolygonTaggedText(cp, outputOrdinates, false, useFormatting, level2, doIndent, writer, ordinateFormat);
                else
                    Assert.ShouldNeverReachHere($"Invalid geometry type for MultiSurface member: {testGeom.GeometryType}");
            }
            writer.Write(")");
        }

    }
}
