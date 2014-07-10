// *******************************************************
// Project: RoboPath.Core
// File Name: VisibilityGraphBuilder.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using QuickGraph;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Graph.Builders
{
    public class VisibilityGraphBuilder : RoadMapBuilder
    {
        public enum VisibilityVertexState
        {
            Undiscovered,
            Discovered,
            Visited
        }
        
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public Coordinate StartVertex { get; private set; }
        public Coordinate GoalVertex { get; private set; }
        public List<IPolygon> Obstacles { get; private set; }
        public bool IncludeBoundary { get; set; }
        public bool StopOnGoalVertexDiscovery { get; set; }
        public Dictionary<Coordinate, VisibilityVertexState> VertexColors { get; private set; }

        #endregion Properties

        #region Public Methods

        public VisibilityGraphBuilder(IGeometryFactory geometryFactory, Envelope bounds, List<IPolygon> obstacles)
            : base(geometryFactory, bounds, GeometryOperations.Union(obstacles))
        {
            IncludeBoundary = true;
            StopOnGoalVertexDiscovery = false;

            // Copy them
            Obstacles = new List<IPolygon>(
                from obstacle in obstacles
                select (IPolygon)obstacle.Clone());
            
            VertexColors = new Dictionary<Coordinate, VisibilityVertexState>();
        }

        public IRoadMap CreateVisibilityGraph(Coordinate startVertex, Coordinate goalVertex)
        {
            StartVertex = startVertex;
            GoalVertex = goalVertex;

            Log.Debug("Creating Visibility Graph [ Start={0}, Goal={1} ]", StartVertex, GoalVertex);
            VertexColors = new Dictionary<Coordinate, VisibilityVertexState>();

            AddGraphVertices();
            AddGraphEdges();

            return Graph;
        }

        #region Overrides

        public override bool AddVertex(Coordinate vertex)
        {
            if(base.AddVertex(vertex))
            {
                VertexColors[vertex] = VisibilityVertexState.Undiscovered;
                return true;
            }
            return false;
        }

        #endregion Overrides

        #endregion Public Methods

        #region Internal Methods

        private void AddGraphVertices()
        {
            // Add the start and goal vertex
            if(!AddVertex(StartVertex))
            {
                throw new RoadMapOperationException("Invalid Start Vertex [ {0} ]", StartVertex);
            }

            if(!AddVertex(GoalVertex))
            {
                throw new RoadMapOperationException("Invalid Goal Vertex [ {0} ]", GoalVertex);
            }

            if(IncludeBoundary)
            {
                // Add bounds
                foreach(var vertex in Bounds.GetVertices())
                {
                    if(!AddVertex(vertex))
                    {
                        throw new RoadMapOperationException("Invalid Bounds Vertex [ {0} ]", vertex);
                    }
                }
            }
            
            // Add obstacles
            foreach(var obstacle in Obstacles)
            {
                var obstacleVertices = obstacle.GetVertices();
                foreach(var vertex in obstacleVertices)
                {
                    if(!AddVertex(vertex))
                    {
                        Log.Warn("Cannot Add Vertex [ {0} ]", vertex);
                    }
                }
            }
        }

        private void AddGraphEdges()
        {
            Log.Debug("Adding Visibility Graph Edges");
            var searchVertices = new List<Coordinate>
                                     {
                                         StartVertex
                                     };

            while(searchVertices.Any())
            {
                var viableVertices = new List<Coordinate>(searchVertices);
                foreach(var vertex in viableVertices)
                {
                    var visibleEdges = GetVisibleEdges(vertex);
                    
                    foreach(var edge in visibleEdges)
                    {
                        AddEdge(edge);
                        if(StopOnGoalVertexDiscovery && edge.Target.Equals2D(GoalVertex))
                        {
                            return;
                        }
                    }

                    // mark edge as visited
                    VertexColors[vertex] = VisibilityVertexState.Visited;
                }

                searchVertices = new List<Coordinate>(from targetVertex in VertexColors
                                        where targetVertex.Value == VisibilityVertexState.Discovered
                                        select targetVertex.Key);                
            }
        }

        private List<IEdge<Coordinate>> GetVisibleEdges(Coordinate sourceVertex)
        {
            var unvisitedVertices = from vertex in VertexColors
                                    where vertex.Value != VisibilityVertexState.Visited && !vertex.Key.Equals2D(sourceVertex)
                                    select vertex.Key;
            var visitedVertices = new List<Coordinate>();
            var validEdges = new HashSet<IEdge<Coordinate>>();

            foreach(var edge in unvisitedVertices.Select(targetVertex => new UndirectedEdge<Coordinate>(sourceVertex, targetVertex)))
            {
                if(IsValidEdge(edge) && !Graph.ContainsEdge(edge))
                {
                    validEdges.Add(edge);
                    visitedVertices.Add(edge.Target);
                }
                          
            }
            visitedVertices.ForEach(v => VertexColors[v] = VisibilityVertexState.Discovered);
            return validEdges.ToList();
        }

        #endregion Internal Methods
    }
}