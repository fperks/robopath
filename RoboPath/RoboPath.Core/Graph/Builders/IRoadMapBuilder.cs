// *******************************************************
// Project: RoboPath.Core
// File Name: IRoadMapBuilder.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

using GeoAPI.Geometries;

using QuickGraph;

namespace RoboPath.Core.Graph.Builders
{
    public interface IRoadMapBuilder
    {
        IGeometryFactory GeometryFactory { get; }
        Envelope Bounds { get; }
        IRoadMap Graph { get; }
        IGeometry InvalidRegions { get; set; }

        List<IEdge<Coordinate>> CreateValidEdges(Envelope envelope);
        List<IEdge<Coordinate>> CreateValidEdges(IGeometry geometry, bool includeHoles = true);
        List<IEdge<Coordinate>> CreateValidEdges(IPolygon polygon, bool includeHoles = true);
        List<IEdge<Coordinate>> CreateValidEdges(ILinearRing ring);
        List<IEdge<Coordinate>> CreateValidEdges(ILineString lineString, bool isRing = false);

        Coordinate GetNearestNeighbour(Coordinate source);

        bool IsValidVertex(Coordinate vertex);
        bool IsValidEdge(IEdge<Coordinate> edge);
        bool IsNewEdgeValid(IEdge<Coordinate> edge);

        bool AddVertex(Coordinate vertex);
        bool AddEdge(IEdge<Coordinate> edge);
        
        List<Coordinate> AddVertices(IEnumerable<Coordinate> vertices);
        List<IEdge<Coordinate>> AddEdges(IEnumerable<IEdge<Coordinate>> edges);
    }
}