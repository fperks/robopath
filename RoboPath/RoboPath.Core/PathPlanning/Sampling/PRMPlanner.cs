// *******************************************************
// Project: RoboPath.Core
// File Name: PRMPlanner.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using QuickGraph;

using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Sampling
{
    public class PRMPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IRoadMapBuilder _graphBuilder;

        #endregion Fields

        #region Properties

        public int N { get; private set; }
        public int NeighbourCount { get; private set; }
        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.ProbalisticRoadMap; }
        }

        #endregion Properties

        #region Public Methods

        public PRMPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot, int count, int neighbourCount)
            : base(factory, cspace, robot)
        {
            N = count;
            NeighbourCount = neighbourCount;
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            _graphBuilder = new RoadMapBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace);
            AddNodes();
            AddEdges();
            Graph = _graphBuilder.Graph;
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

        private void AddNodes()
        {
            _graphBuilder.AddVertex(StartVertex);
            _graphBuilder.AddVertex(GoalVertex);
            for(var i = 0; i < N; i++)
            {
                AddRandomVertex();
            }
        }

        private void AddEdges()
        {
            AddNeighbourEdges(StartVertex);
            AddNeighbourEdges(GoalVertex);
            foreach(var sourceVertex in _graphBuilder.Graph.Vertices)
            {
                AddNeighbourEdges(sourceVertex);
            }
        }

        private void AddNeighbourEdges(Coordinate sourceVertex)
        {
            var count = 0;
            foreach(var target in from vertex in _graphBuilder.Graph.Vertices
                                  orderby vertex.Distance(sourceVertex)
                                  select vertex)
            {
                if(_graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(sourceVertex, target)))
                {
                    count++;
                    if(count >= NeighbourCount)
                    {
                        break;
                    }
                }
            }
        }

        private void AddRandomVertex()
        {
            var generator = new Random();
            while(true)
            {
                var x = generator.Next((int) CSpace.Bounds.Width - 1);
                var y = generator.Next((int) CSpace.Bounds.Height - 1);

                var vertex = new Coordinate(x, y);
                if(!_graphBuilder.AddVertex(vertex))
                {
                    continue;
                }
                break;
            }
        }


        #endregion Internal Methods
    }
}