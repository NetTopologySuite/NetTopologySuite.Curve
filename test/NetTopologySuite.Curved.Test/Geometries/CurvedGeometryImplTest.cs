using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

namespace NetTopologySuite.Test.Geometries
{
    public abstract class CurvedGeometryImplTest
    {

        private readonly NtsGeometryServices _instance = new NtsCurvedGeometryServices(
            CoordinateArraySequenceFactory.Instance, new PrecisionModel(PrecisionModels.Floating), 0,
            new CoordinateEqualityComparer(), 0d);

        protected CurvedGeometryFactory Factory
        {
            get { return (CurvedGeometryFactory) _instance.CreateGeometryFactory(); }
        }
    }
}
