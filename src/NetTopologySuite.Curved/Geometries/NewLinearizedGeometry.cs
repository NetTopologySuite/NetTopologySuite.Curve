namespace NetTopologySuite.Geometries
{
    internal class NewLinearizedGeometry<T> : IGeometryComponentFilter where T:Geometry
    {
        public NewLinearizedGeometry(T linearized)
        {
            Linearized = linearized;
        }

        private T Linearized { get; }

        public void Filter(Geometry geom)
        {
            if (geom is ILinearizable<T> linearized)
            {
                //TODO 
            }
            geom.GeometryChangedAction();
        }
    }
}
