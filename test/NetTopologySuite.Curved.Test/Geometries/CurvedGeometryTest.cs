using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Test.Geometries
{
    public abstract class CurvedGeometryTest
    {
        protected CurvedGeometryFactory Factory { get; }  = new CurvedGeometryFactory(
            NtsGeometryServices.Instance.DefaultPrecisionModel,
            NtsGeometryServices.Instance.DefaultSRID,
            NtsGeometryServices.Instance.DefaultCoordinateSequenceFactory, 0.001d);
    }
}
