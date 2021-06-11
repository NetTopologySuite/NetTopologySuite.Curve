using System;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class WKBWriterEx : WKBWriter
    {
        /// <inheritdoc cref="GetOtherGeometryRequiredBufferSize"/>
        protected override int GetOtherGeometryRequiredBufferSize(Geometry geometry, bool includeSRID)
        {
            if (geometry is CircularString cs)
                return GetRequiredBufferSize(cs, includeSRID);
            if (geometry is CompoundCurve cc)
                return GetRequiredBufferSize(cc, includeSRID);
            if (geometry is CurvePolygon cp)
                return GetRequiredBufferSize(cp, includeSRID);
            if (geometry is MultiCurve mc)
                return GetRequiredBufferSize(mc, includeSRID);
            if (geometry is MultiSurface ms)
                return GetRequiredBufferSize(ms, includeSRID);

            return 0;
        }

        private int GetRequiredBufferSize(CircularString cs, bool includeSRID)
        {
            int pointSize = CoordinateSize;
            int numPoints = cs.ControlPoints.Count;
            int count = GetHeaderSize(includeSRID);
            count += 4;                             // NumPoints
            count += pointSize * numPoints;
            return count;
        }

        private int GetRequiredBufferSize(CompoundCurve cc, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4; // number of curve items
            for (int i = 0; i < cc.Curves.Count; i++)
                count += GetRequiredBufferSize(cc.Curves[i], false);
            return count;
        }

        private int GetRequiredBufferSize(CurvePolygon cp, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4; // number of curve items
            count += GetRequiredBufferSize(cp.ExteriorRing, false);
            for (int i = 0; i < cp.NumInteriorRings; i++)
                count += GetRequiredBufferSize(cp.GetInteriorRingN(i), false);
            return count;
        }

        private int GetRequiredBufferSize(GeometryCollection gc, bool includeSRID)
        {
            int count = GetHeaderSize(includeSRID);
            count += 4; // number of curve items
            for (int i = 0; i < gc.NumGeometries; i++)
                count += GetRequiredBufferSize(gc.GetGeometryN(i), false);
            return count;
        }

        /// <inheritdoc cref="GetGeometryType"/>
        protected override uint GetGeometryType(Geometry geom)
        {
            switch (geom)
            {
                case CircularString _:
                    return 8u;
                case CompoundCurve _:
                    return 9u;
                case CurvePolygon _:
                    return 10u;
                case MultiCurve _:
                    return 11u;
                case MultiSurface _:
                    return 12u;
            }
            return base.GetGeometryType(geom);
        }

        /// <inheritdoc cref="WriteOtherGeometry"/>
        protected override bool WriteOtherGeometry(Geometry geometry, BinaryWriter writer, bool includeSRID)
        {
            switch (geometry)
            {
                case CircularString cs:
                    Write(cs, writer, includeSRID);
                    return true;
                case CompoundCurve cc:
                    Write(cc, writer, includeSRID);
                    return true;
                case CurvePolygon cp:
                    Write(cp, writer, includeSRID);
                    return true;
                case MultiCurve mc:
                    Write(mc, writer, includeSRID);
                    return true;
                case MultiSurface ms:
                    Write(ms, writer, includeSRID);
                    return true;
            }

            return false;
        }

        private void Write(CircularString cs, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, cs, includeSRID);
            Write(cs.ControlPoints, true, writer);
        }

        private void Write(CompoundCurve cc, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, cc, includeSRID);
            writer.Write(cc.Curves.Count);
            for (int i = 0; i < cc.Curves.Count; i++)
                Write(cc.Curves[i], writer, false);
        }

        private void Write(CurvePolygon cp, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, cp, includeSRID);
            if (cp.IsEmpty)
            {
                writer.Write(0);
                return;
            }
            writer.Write(cp.NumInteriorRings + 1);
            Write(cp.ExteriorRing, writer, false);
            for (int i = 0; i < cp.NumInteriorRings; i++)
                Write(cp.GetInteriorRingN(i), writer, false);

        }

        private void Write(MultiCurve mc, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, mc, includeSRID);
            writer.Write(mc.NumGeometries);
            for (int i = 0; i < mc.NumGeometries; i++)
            {
                var curve = mc.GetGeometryN(i);
                Write(curve, writer, false);
            }
        }

        private void Write(MultiSurface ms, BinaryWriter writer, bool includeSRID)
        {
            WriteHeader(writer, ms, includeSRID);
            writer.Write(ms.NumGeometries);
            for (int i = 0; i < ms.NumGeometries; i++)
            {
                var curve = ms.GetGeometryN(i);
                Write(curve, writer, false);
            }
        }
    }
}
