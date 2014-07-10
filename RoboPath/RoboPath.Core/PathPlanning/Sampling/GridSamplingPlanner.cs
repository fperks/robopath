// *******************************************************
// Project: RoboPath.Core
// File Name: GridSamplingPlanner.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using QuickGraph;

using RoboPath.Core.Algorithms.Decomposition;
using RoboPath.Core.Algorithms.Triangulation.EarClipping;
using RoboPath.Core.DataStructures;
using RoboPath.Core.Geometry;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Sampling
{
    public class GridSamplingPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IRoadMapBuilder _graphBuilder;

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.GridSampling; }
        }

        public double SamplingDistance { get; set; }

        public RasterGrid2D<Coordinate> GridCells { get; private set; }

        #endregion Properties

        #region Public Methods

        public GridSamplingPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot)
            : base(factory, cspace, robot)
        {
            SamplingDistance = double.NaN;

        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            _graphBuilder = new RoadMapBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace);
            if(double.IsNaN(SamplingDistance))
            {
                throw new PathPlannerException("Grid Sampling planner requires a valid gridsize");
            }

            Log.Debug("Creating Grid with Sampling Distance of [ {0} ]", SamplingDistance);
            PopulateGrid();
            AddEdges();

            _graphBuilder.AddVertex(StartVertex);
            //_graphBuilder.AddEdge(new Edge<Coordinate>(
            //                          _graphBuilder.GetNearestNeighbour(StartVertex),
            //                          StartVertex));

            _graphBuilder.AddVertex(GoalVertex);
            //_graphBuilder.AddEdge(new Edge<Coordinate>(
            //                          _graphBuilder.GetNearestNeighbour(GoalVertex),
            //                          GoalVertex));
            
            Graph = _graphBuilder.Graph;
            Graph.AddEdge(new UndirectedEdge<Coordinate>(_graphBuilder.GetNearestNeighbour(GoalVertex), GoalVertex));
            Graph.AddEdge(new UndirectedEdge<Coordinate>(_graphBuilder.GetNearestNeighbour(StartVertex), StartVertex));
        }

        protected override PathPlannerAlgorithmState OnSolve()
        {
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

        private void PopulateGrid()
        {
            var colCount = (int)Math.Ceiling(CSpace.Bounds.Width / SamplingDistance) + 1;
            var rowCount = (int)Math.Ceiling(CSpace.Bounds.Height / SamplingDistance) + 1;

            GridCells = new RasterGrid2D<Coordinate>(colCount, rowCount);
            for(var y = 0; y < rowCount; y++)
            {
                for(var x = 0; x < colCount; x++)
                {
                    var vertex = new Coordinate(x * SamplingDistance, y * SamplingDistance);
                    if(_graphBuilder.AddVertex(vertex))
                    {
                        GridCells[x, y] = vertex;
                    }
                    else
                    {
                        GridCells[x, y] = null;
                    }
                }
            }

            
        }

        private void AddEdges()
        {
            for(var y = 0; y < GridCells.Height; y++)
            {
                for(var x = 0; x < GridCells.Width; x++)
                {
                    AddAdjacentEdges(x, y);
                }
            }
        }

        private void AddAdjacentEdges(int x, int y)
        {
            var source = GridCells[x, y];
            if(source == null)
            {
                return;
            }

            foreach(var offset in GetAdjacentOffsets(x, y))
            {
                var dx = offset.Item1;
                var dy = offset.Item2;
                var target = GridCells[dx, dy];
                if(target == null)
                {
                    continue;
                }
                _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(source, target));
            }
        }

        private List<Tuple<int, int>> GetAdjacentOffsets(int x, int y)
        {
            var result = new List<Tuple<int, int>>();
            for(var dx = (x > 0 ? -1 : 0); dx <= (x < GridCells.Width -1 ? 1 : 0); ++dx)
            {
                for(var dy = (y > 0 ? -1 : 0); dy <= (y < GridCells.Height - 1 ? 1 : 0); ++dy)
                {
                    if(dx != 0 || dy != 0)
                    {
                        result.Add(Tuple.Create(x + dx, y + dy));
                    }
                }
            }
            return result;
        }

        #endregion Internal Methods
    }
}