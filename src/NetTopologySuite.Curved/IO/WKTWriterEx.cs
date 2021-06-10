using System;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    public class WKTWriterEx : WKTWriter
    {
        public WKTWriterEx ForSqlServer()
        {
            return new WKTWriterEx(4, true);}

        public WKTWriterEx()
            : this(2)
        {
        }
        public WKTWriterEx(int outputDimension)
            : this(2, false)
        {
        }
        public WKTWriterEx(int outputDimension, bool mssql)
            : base(outputDimension, mssql)
        {
        }

        public override void Write(Geometry geometry, TextWriter writer)
        {
            if (geometry is ICurveGeometry)
            {
                WriteCurveFormatted(geometry, false, writer, PrecisionModel);
            }
            else
            {
                base.Write(geometry, writer);
            }
        }

        /// <inheritdoc cref="WKTWriter.WriteFormatted(Geometry, TextWriter)"/>
        /// <remarks>Additionally handles
        /// <list type="bullet">
        /// <item><description><c>CircularString</c>,</description></item>
        /// <item><description><c>CompoundCurve</c>,</description></item>
        /// <item><description><c>CurvePolygon</c>,</description></item>
        /// <item><description><c>MultiCurve</c> and</description></item>
        /// <item><description><c>MultiSurface</c></description></item>
        /// </list></remarks>
        public override void WriteFormatted(Geometry geometry, TextWriter writer)
        {
            if (geometry is ICurveGeometry)
            {
                WriteCurveFormatted(geometry, true, writer, PrecisionModel);
            }
            else
            {
                base.WriteFormatted(geometry, writer);
            }
        }

        private void WriteCurveFormatted(Geometry geometry, bool useFormatting, TextWriter writer, PrecisionModel precisionModel)
        {
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            // ensure we have a precision model
            precisionModel = precisionModel ?? geometry.PrecisionModel;

            // create the ordinate format
            var ordinateFormat = CreateOrdinateFormat(precisionModel);

            // evaluate the ordinates actually present in the geometry
            var outputOrdinates = GetOutputOrdinates(geometry);

            // append the WKT
            AppendCurveGeometryTaggedText(geometry, outputOrdinates, useFormatting, 0, writer, ordinateFormat);
        }

        private void AppendCurveGeometryTaggedText(Geometry geometry, Ordinates outputOrdinates, bool useFormatting,
            int level, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            Indent(useFormatting, level, writer);
            
            switch (geometry)
            {
                case CircularString cs:
                    AppendCircularStringTaggedText(cs, outputOrdinates, useFormatting, level, false, writer, ordinateFormat);
                    break;
                case CompoundCurve cc:
                    AppendCompoundCurveTaggedText(cc, outputOrdinates, useFormatting, level, false, writer, ordinateFormat);
                    break;
                case CurvePolygon cp:
                    AppendCurvePolygonTaggedText(cp, outputOrdinates, useFormatting, level, false, writer, ordinateFormat);
                    break;
                case MultiCurve mc:
                    AppendMultiCurveTaggedText(mc, outputOrdinates, useFormatting, level, writer, ordinateFormat);
                    break;
                case MultiSurface ms:
                    AppendMultiSurfaceTaggedText(ms, outputOrdinates, useFormatting, level, writer, ordinateFormat);
                    break;
            }
        }

        private void AppendCircularStringTaggedText(CircularString cs, Ordinates outputOrdinates, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"CIRCULARSTRING ");
            AppendOrdinateText(outputOrdinates, writer);
            AppendSequenceText(cs.ControlPoints, outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
        }

        private void AppendCompoundCurveTaggedText(CompoundCurve cc, Ordinates outputOrdinates, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"COMPOUNDCURVE ");
            AppendOrdinateText(outputOrdinates, writer);

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
                    AppendCircularStringTaggedText(cs, outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
                    break;
                case CompoundCurve cc:
                    AppendCompoundCurveTaggedText(cc, outputOrdinates, useFormatting, level, indentFirst, writer, ordinateFormat);
                    break;
                default:
                    Assert.ShouldNeverReachHere($"Invalid geometry type for curve: {curve.GeometryType}");
                    break;

            }

        }
        private void AppendCurvePolygonTaggedText(CurvePolygon cp, Ordinates outputOrdinates, bool useFormatting, int level, bool indentFirst, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"CURVEPOLYGON ");
            AppendOrdinateText(outputOrdinates, writer);

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

        private void AppendMultiCurveTaggedText(MultiCurve mc, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"MULTICURVE ");
            AppendOrdinateText(outputOrdinates, writer);

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

        private void AppendMultiSurfaceTaggedText(MultiSurface ms, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"MULTISURFACE ");
            AppendOrdinateText(outputOrdinates, writer);

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
                    AppendCurvePolygonTaggedText(cp, outputOrdinates, useFormatting, level2, doIndent, writer, ordinateFormat);
                else
                    Assert.ShouldNeverReachHere($"Invalid geometry type for MultiSurface member: {testGeom.GeometryType}");
            }
            writer.Write(")");
        }

    }
}
