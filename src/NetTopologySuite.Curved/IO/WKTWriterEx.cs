using System;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    public class WKTWriterEx : WKTWriter
    {
        //private bool _skipOrdinateToken;
        //private bool _alwaysEmitZWithM;
        //private string _missingOrdinateReplacementText = "NaN";

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
            if (geometry is ICurvedGeometry)
            {
                WriteCurvedFormatted(geometry, false, writer, PrecisionModel);
            }
            else
            {
                base.Write(geometry, writer);
            }
        }

        public override void WriteFormatted(Geometry geometry, TextWriter writer)
        {
            if (geometry is ICurvedGeometry)
            {
                WriteCurvedFormatted(geometry, true, writer, PrecisionModel);
            }
            else
            {
                base.WriteFormatted(geometry, writer);
            }
        }

        private void WriteCurvedFormatted(Geometry geometry, bool useFormatting, TextWriter writer, PrecisionModel precisionModel)
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
            AppendCurvedGeometryTaggedText(geometry, outputOrdinates, useFormatting, 0, writer, ordinateFormat);
        }

        private void AppendCurvedGeometryTaggedText(Geometry geometry, Ordinates outputOrdinates, bool useFormatting,
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
                case CurvedPolygon cp:
                    AppendCurvedPolygonTaggedText(cp, outputOrdinates, useFormatting, level, writer, ordinateFormat);
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
            writer.Write("(");

            for (int i = 0; i < cc.Curves.Count; i++)
            {
                if (i > 0) writer.Write(",");
                if (cc.Curves[i] is CircularString cs)
                    AppendCircularStringTaggedText(cs, outputOrdinates, useFormatting, level, true, writer, ordinateFormat);
                else if (cc.Curves[i] is LineString ls)
                    AppendSequenceText(ls.CoordinateSequence, outputOrdinates, useFormatting, level, true, writer, ordinateFormat);
                else
                    Assert.ShouldNeverReachHere("Invalid geometry in CompoundCurve");
            }
            writer.Write(")");
        }

        private void AppendCurvedPolygonTaggedText(CurvedPolygon cp, Ordinates outputOrdinates, bool useFormatting, int level, TextWriter writer, OrdinateFormat ordinateFormat)
        {
            writer.Write(@"CURVEDPOLYGON ");
            AppendOrdinateText(outputOrdinates, writer);

            if (cp.IsEmpty)
            {
                writer.Write(WKTConstants.EMPTY);
                return;
            }
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

            writer.Write("(");
            for (int i = 0; i < mc.NumGeometries; i++)
            {
                if (i > 0) writer.Write(",");
                var testGeom = mc.GetGeometryN(i);
                if (testGeom is LineString ls)
                    AppendSequenceText(ls.CoordinateSequence, outputOrdinates, useFormatting, level + 1, true, writer, ordinateFormat);
                else if (testGeom is CircularString cs)
                    AppendCircularStringTaggedText(cs, outputOrdinates, useFormatting, level + 1, true, writer, ordinateFormat);
                else if (testGeom is CompoundCurve cc)
                    AppendCompoundCurveTaggedText(cc, outputOrdinates, useFormatting, level + 1, true, writer, ordinateFormat);

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
            writer.Write("(");
            for (int i = 0; i < ms.NumGeometries; i++)
            {
                if (i > 0) writer.Write(",");
                var testGeom = ms.GetGeometryN(i);
                if (testGeom is Polygon p)
                    AppendPolygonText(p, outputOrdinates, useFormatting, level + 1, true, writer, ordinateFormat);
                else if (testGeom is CurvedPolygon cp)
                    AppendCurvedPolygonTaggedText(cp, outputOrdinates, useFormatting, level+1, writer, ordinateFormat);
            }
            writer.Write(")");
        }

    }
}
