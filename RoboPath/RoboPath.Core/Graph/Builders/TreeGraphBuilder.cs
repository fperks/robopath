// *******************************************************
// Project: RoboPath.Core
// File Name: TreeGraphBuilder.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

namespace RoboPath.Core.Graph.Builders
{
    public class TreeGraphBuilder : RoadMapBuilder
    {
        public Coordinate BiasVertex { get; set; }

        public TreeGraphBuilder(IGeometryFactory geometryFactory, Envelope bounds, IGeometry invalidRegions, Coordinate biasVertex)
            : base(geometryFactory, bounds, invalidRegions)
        {
            BiasVertex = biasVertex;
            
        }
    }
}