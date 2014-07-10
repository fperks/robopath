// *******************************************************
// Project: RoboPath.Core
// File Name: TriangulationPathPlanner.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate.QuadEdge;

using QuickGraph;

using RoboPath.Core.Algorithms.Triangulation;
using RoboPath.Core.Graph;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Geometric
{
    public class TriangulationPathPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IRoadMapBuilder _graphBuilder;
        private Dictionary<QuadEdgeTriangle, Coordinate> _triangleGraphVertices;

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.Triangulation; }
        }

        
        public ConformingDelaunayTriangulation Triangulator { get; private set; }
        public List<QuadEdgeTriangle> ValidTriangles { get; private set; }

        public bool UseSleeveOptimization { get; set; }
        public List<QuadEdgeTriangle> SleeveTriangles { get; set; } 
        
        #endregion Properties

        #region Public Methods

        public TriangulationPathPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot)
            : base(factory, cspace, robot)
        {
            UseSleeveOptimization = false;
        }

        protected override void OnInitialization()
        {            
            _graphBuilder = new RoadMapBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace);
            Triangulate();
            CreateGraph();
        }

        protected override PathPlannerAlgorithmState OnSolve()
        {
            Log.Debug("Computing Shortest Path");
            Graph.ComputeEdgeCosts(edge => edge.Source.Distance(edge.Target));
            ShortestPath = Graph.ComputeShortestPath(StartVertex, GoalVertex, ShortestPathAlgorithm);
            if(!ShortestPath.Any())
            {
                return PathPlannerAlgorithmState.ShortestPathNotFound;
            }

            return PathPlannerAlgorithmState.ShortestPathFound;
        }

        protected override bool OnValidate()
        {
            return true;
        }

        #endregion Public Methods

        #region Internal Methods

        private void Triangulate()
        {
            _triangleGraphVertices = new Dictionary<QuadEdgeTriangle, Coordinate>();
            Triangulator = new ConformingDelaunayTriangulation(GeometryFactory, CSpace.Bounds, CSpace.Obstacles);
            Triangulator.Triangulate();

            ValidTriangles = new List<QuadEdgeTriangle>(Triangulator.Triangles);
            ValidTriangles.ForEach(triangle => _triangleGraphVertices[triangle] = null);
        }

        private void CreateGraph()
        {
            _triangleGraphVertices = new Dictionary<QuadEdgeTriangle, Coordinate>();
            
            AddVertices();
            AddEdges();

            Graph = _graphBuilder.Graph;
        }

        
        private void AddVertices()
        {
            foreach(var triangle in ValidTriangles)
            {
                var geometry = triangle.GetGeometry((GeometryFactory)GeometryFactory);
                var point = geometry.Centroid;
                var vertex = point.Coordinate;
                if(!_graphBuilder.IsValidVertex(vertex))
                {
                    continue;
                }

                _triangleGraphVertices[triangle] = vertex;
                _graphBuilder.AddVertex(vertex);
                if(triangle.Contains(StartVertex))
                {
                    _graphBuilder.AddVertex(StartVertex);
                    _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(vertex, StartVertex));
                }

                if(triangle.Contains(GoalVertex))
                {                    
                    _graphBuilder.AddVertex(GoalVertex);
                    _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(vertex, GoalVertex));
                }
            }
        }

        private void AddEdges()
        {
            foreach(var pair in _triangleGraphVertices)
            {
                var triangle = pair.Key;
                var sourceVertex = pair.Value;
                var edges = new List<IEdge<Coordinate>>();
                
                // link adjacent
                foreach(var adjacentTriangle in triangle.GetNeighbours())
                {
                    if(adjacentTriangle == null)
                    {
                        continue;
                    }

                    if(!_triangleGraphVertices.ContainsKey(adjacentTriangle))
                    {
                        // it was not a valid triangle
                        continue;
                    }
                    var targetVertex = _triangleGraphVertices[adjacentTriangle];
                    edges.Add(new UndirectedEdge<Coordinate>(sourceVertex, targetVertex));
                }

                foreach(var edge in edges)
                {
                    _graphBuilder.AddEdge(edge);
                }                
            }

           
        }


        #endregion Internal Methods
    }
}