// *******************************************************
// Project: RoboPath.Core
// File Name: RoadMap.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using QuickGraph;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.ShortestPath;
using QuickGraph.Algorithms.Observers;

namespace RoboPath.Core.Graph
{
    public class RoadMap : 
        UndirectedGraph<Coordinate, IEdge<Coordinate>>, 
        IRoadMap
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public Dictionary<IEdge<Coordinate>, double> EdgeCosts { get; private set; }

        #endregion Properties

        #region Public Methods

        public RoadMap(IGeometryFactory geometryFactory)
        {
            GeometryFactory = geometryFactory;
            EdgeCosts = new Dictionary<IEdge<Coordinate>, double>();
        }
        
        public void ComputeEdgeCosts(Func<IEdge<Coordinate>, double> edgeCostDelegate)
        {
            EdgeCosts.Clear();
            foreach(var edge in Edges)
            {
                EdgeCosts.Add(edge, edgeCostDelegate(edge));
            }
        }

        public double GetEdgeCost(IEdge<Coordinate> edge)
        {
            if(!EdgeCosts.ContainsKey(edge))
            {
                throw new RoadMapOperationException("Edge [ {0} ] does not have a cost defined for it", edge);
            }
            return EdgeCosts[edge];
        }

        public List<IEdge<Coordinate>> ComputeShortestPath(Coordinate startVertex, Coordinate goalVertex, ShortestPathAlgorithm algorithmType)
        {
            Log.Debug("Computing Shortest Path between [ {0} -> {1} ]", startVertex, goalVertex);

            //if(!EdgeCosts.Any())
            //{
            //    throw new RoadMapOperationException("Edge Costs Must be Computed First, before computing Shortest Path");
            //}

            if(!ContainsVertex(startVertex))
            {
                throw new RoadMapOperationException("Start Vertex [ {0} ] does not exist in Graph", startVertex);
            }

            if(!ContainsVertex(goalVertex))
            {
                throw new RoadMapOperationException("Goal Vertex [ {0} ] does not exist in Graph", goalVertex);
            }

            var result = ComputeUndirectedDijkstraShortestPath(startVertex, goalVertex);
            if(result == null)
            {
                Log.Info("No Shortest Path [ {0} -> {1} ] exists", startVertex, goalVertex);
                return new List<IEdge<Coordinate>>();
            }

            return result;
        }

        public List<Coordinate> GetNearestNeighbours(Coordinate source, int count)
        {
            //Log.Debug("Getting Nearest Neighbours");
            var result = from vertex in Vertices
                         orderby source.Distance(vertex)
                         select vertex;
            return result.Take(count).ToList();
        }

        #endregion Public Methods

        #region Internal Methods

        private List<IEdge<Coordinate>> ComputeUndirectedDijkstraShortestPath(Coordinate startVertex, Coordinate goalVertex)
        {
            // Initialize the algorithm
            var dijkstra = new UndirectedDijkstraShortestPathAlgorithm<Coordinate, IEdge<Coordinate>>(this, GetEdgeCost);
            var vertexDistanceRecorder = new UndirectedVertexDistanceRecorderObserver<Coordinate, IEdge<Coordinate>>(GetEdgeCost);
            vertexDistanceRecorder.Attach(dijkstra);

            // Records the path the SP takes
            var vertexPredecessorRecorder = new UndirectedVertexPredecessorRecorderObserver<Coordinate, IEdge<Coordinate>>();
            vertexPredecessorRecorder.Attach(dijkstra);

            // Compute SP
            dijkstra.Compute(startVertex);

            // extract the SP
            IEnumerable<IEdge<Coordinate>> shortestPath;
            var isSuccess = vertexPredecessorRecorder.TryGetPath(goalVertex, out shortestPath);

            if(!isSuccess)
            {
                return null;
            }
            return shortestPath.ToList();
        }

        #endregion Internal Methods


    }
}