// *******************************************************
// Project: RoboPath.Core
// File Name: IRoadMap.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

using GeoAPI.Geometries;

using QuickGraph;

namespace RoboPath.Core.Graph
{
    public interface IRoadMap : 
        IMutableUndirectedGraph<Coordinate, IEdge<Coordinate>>
    {
        IGeometryFactory GeometryFactory { get; }
        Dictionary<IEdge<Coordinate>, double> EdgeCosts { get; }

        double GetEdgeCost(IEdge<Coordinate> edge);
        void ComputeEdgeCosts(Func<IEdge<Coordinate>, double> edgeCostDelegate);
        List<IEdge<Coordinate>> ComputeShortestPath(Coordinate startVertex, Coordinate goalVertex, ShortestPathAlgorithm algorithmType);

        List<Coordinate> GetNearestNeighbours(Coordinate source, int count);
    }
}