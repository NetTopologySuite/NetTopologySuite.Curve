namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Marker interface for geometries that have 
    /// </summary>
    public interface ICurvedGeometry
    {
        /// <summary>
        /// Creates a flattened version of this curved geometry, 
        /// </summary>
        /// <returns>A flattened geometry</returns>
        Geometry Flatten();

        /// <summary>
        /// Gets a value indicating the max. length of arc segments when flattening the curved geometry.
        /// </summary>
        double ArcSegmentLength { get; }
    }

    /// <summary>
    /// Generic version of the <see cref="ICurvedGeometry"/> interface
    /// </summary>
    /// <typeparam name="T">The type of the flattened geometry.</typeparam>
    public interface ICurvedGeometry<out T> : ICurvedGeometry where T : Geometry
    {
        /// <summary>
        /// Creates a flattened version of this curved geometry, 
        /// </summary>
        /// <returns>A flattened geometry</returns>
        new T Flatten();

        /// <summary>
        /// Creates a flattened version of this curved geometry, 
        /// </summary>
        /// <param name="arcSegmentLength">The maximum length of arc segments in the flattened geometry</param>
        /// <returns>A flattened geometry</returns>
        T Flatten(double arcSegmentLength);
    }
}
