// *******************************************************
// Project: RoboPath.Core
// File Name: GreedyRRTPlanner.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;

using QuickGraph;

using RoboPath.Core.Geometry;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Sampling
{
    public class GreedyRRTPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IRoadMapBuilder _graphBuilder;

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get
            {
                return PathPlannerAlgorithmType.RRT;
            }
        }

        public bool StopAtGoal { get; set; }

        public int N { get; private set; }
        public double StepSize { get; private set; }
        public double GoalBias { get; private set; }

        #endregion Properties

        #region Public Methods

        public GreedyRRTPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot, int count, double stepSize, double goalBias)
            : base(factory, cspace, robot)
        {
            N = count;
            StepSize = stepSize;
            GoalBias = goalBias;
            StopAtGoal = false;
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            _graphBuilder = new RoadMapBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace);

            _graphBuilder.AddVertex(StartVertex);

            CreateTree();
            _graphBuilder.AddVertex(GoalVertex);

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

        private void CreateTree()
        {
            //var v = StartVertex;
            var count = 0;
            var generator = new Random();
            while(count <= N)
            {
                Coordinate q;

                if(GoalBias < generator.NextDouble())
                {
                    var x = generator.Next((int)CSpace.Bounds.Width - 1);
                    var y = generator.Next((int)CSpace.Bounds.Height - 1);
                    q = new Coordinate(x, y);
                }
                else
                {
                    q = GoalVertex;
                }

                var v = _graphBuilder.GetNearestNeighbour(q);
                var p = GetPointAlongRay(v, q);

                var edge = new UndirectedEdge<Coordinate>(v, p);
                if(_graphBuilder.IsNewEdgeValid(edge))
                {
                    if(!_graphBuilder.AddVertex(p))
                    {
                        continue;
                    }
                    if(p.Distance(GoalVertex) <= StepSize)
                    {
                        _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(p, GoalVertex));
                        if(StopAtGoal)
                        {
                            break;
                        }
                    }
                    _graphBuilder.AddEdge(edge);
                }
                count++;
            }
        }

        private Coordinate GetPointAlongRay(Coordinate source, Coordinate target)
        {
            var segment = new LineSegment(source, target);
            var result = segment.PointAlong(StepSize / segment.Length);
            return result;
        }

        private Coordinate GetRandomVertex()
        {
            var generator = new Random();


            while(true)
            {
                var vertex = new Coordinate(generator.NextDouble() * CSpace.Bounds.Width, generator.NextDouble() * CSpace.Bounds.Height);
                var location = OccupiedRegions.LocateVertex(vertex);
                if(OccupiedRegions.LocateVertex(vertex) != Location.Interior)
                {
                    return vertex;
                }
            }
        }

        #endregion Internal Methods
    }
}