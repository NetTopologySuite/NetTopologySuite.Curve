using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// 
    /// </summary>
    public class WKTReaderEx : WKTReader
    {
        /// <summary>
        /// Creates an instance of this class using the provided geometry services object
        /// </summary>
        /// <param name="geometryServices">A geometry services object</param>
        public WKTReaderEx(NtsCurveGeometryServices geometryServices)
            :base(geometryServices)
        {}

        protected override Geometry ReadOtherGeometryText(string type, TokenStream tokens, GeometryFactory factory, Ordinates ordinateFlags)
        {
            if (!(factory is CurveGeometryFactory curveFactory))
                throw new ArgumentException("Not a CurveGeometryFactory", nameof(factory));

            try
            {
                if (type.StartsWith("CIRCULARSTRING"))
                    return ReadCircularStringText(tokens, curveFactory, ordinateFlags);

                if (type.StartsWith("COMPOUNDCURVE"))
                    return ReadCompoundCurveText(tokens, curveFactory, ordinateFlags);

                if (type.StartsWith("CURVEPOLYGON"))
                    return ReadCurvePolygonText(tokens, curveFactory, ordinateFlags);

                if (type.StartsWith("MULTICURVE"))
                    return ReadMultiCurveText(tokens, curveFactory, ordinateFlags);

                if (type.StartsWith("MULTISURFACE"))
                    return ReadMultiSurfaceText(tokens, curveFactory, ordinateFlags);
            }
            catch (Exception e)
            {
                throw new ParseException(e);
            }

            // This will throw ParseException
            return base.ReadOtherGeometryText(type, tokens, factory, ordinateFlags);
        }

        private CircularString ReadCircularStringText(TokenStream tokens, CurveGeometryFactory factory, Ordinates ordinateFlags)
        {
            var sequence = GetCoordinateSequence(factory, tokens, ordinateFlags);
            return factory.CreateCircularString(sequence);
        }

        private CompoundCurve ReadCompoundCurveText(TokenStream tokens, CurveGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateCompoundCurve();

            var curves = new List<Curve>();
            do
            {
                var curve = ReadCurveText(tokens, factory, ordinateFlags, false);
                curves.Add(curve);
                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

            return factory.CreateCompoundCurve(curves.ToArray());
        }

        private Curve ReadCurveText(TokenStream tokens, CurveGeometryFactory factory, Ordinates ordinateFlags, bool allowCompoundCurve)
        {
            string current = LookAheadWord(tokens);

            if (current == "EMPTY" || current == "(") {
                var sequence = GetCoordinateSequence(factory, tokens, ordinateFlags);
                return factory.CreateLineString(sequence);
            }

            if (current.StartsWith("CIRCULARSTRING"))
            {
                GetNextWord(tokens);
                return ReadCircularStringText(tokens, factory, ordinateFlags);
            }

            if (current.StartsWith("COMPOUNDCURVE"))
            {
                if (!allowCompoundCurve)
                    throw new ParseException("CompoundCurve not allowed at this position");
                GetNextWord(tokens);
                return ReadCompoundCurveText(tokens, factory, ordinateFlags);
            }

            throw new ParseException($"Unexpected token: {current}");
        }


        private CurvePolygon ReadCurvePolygonText(TokenStream tokens, CurveGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateCurvePolygon();

            var holes = new List<Curve>();
            var shell = ReadCurveText(tokens, factory, ordinateFlags, true);
            nextToken = GetNextCloserOrComma(tokens);
            while (nextToken.Equals(","))
            {
                var hole = ReadCurveText(tokens, factory, ordinateFlags, true);
                holes.Add(hole);
                nextToken = GetNextCloserOrComma(tokens);
            }
            return factory.CreateCurvePolygon(shell, holes.ToArray());
        }

        private MultiCurve ReadMultiCurveText(TokenStream tokens, CurveGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateMultiCurve();

            var curves = new List<Geometry>();
            do
            {
                var curve = ReadCurveText(tokens, factory, ordinateFlags, true);
                curves.Add(curve);
                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

            return factory.CreateMultiCurve(curves.ToArray());
        }

        private MultiSurface ReadMultiSurfaceText(TokenStream tokens, CurveGeometryFactory factory, Ordinates ordinateFlags)
        {
            string nextToken = GetNextEmptyOrOpener(tokens);
            if (nextToken.Equals(WKTConstants.EMPTY))
                return factory.CreateMultiSurface();

            var surfaces = new List<Geometry>();
            do
            {
                string current = LookAheadWord(tokens);
                if (current == "EMPTY" || current == "(")
                    surfaces.Add(ReadPolygonText(tokens, factory, ordinateFlags));
                else if (current.StartsWith("CURVEPOLYGON")) {
                    GetNextWord(tokens);
                    surfaces.Add(ReadCurvePolygonText(tokens, factory, ordinateFlags));
                }
                else
                    throw new ParseException($"Unexpected token: {current}");

                nextToken = GetNextCloserOrComma(tokens);
            }
            while (nextToken.Equals(","));

            return factory.CreateMultiSurface(surfaces.ToArray());
        }
    }
}
