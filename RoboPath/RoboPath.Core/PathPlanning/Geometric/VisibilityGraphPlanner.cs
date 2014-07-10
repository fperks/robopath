// *******************************************************
// Project: RoboPath.Core
// File Name: VisibilityGraphPlanner.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Graph;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Geometric
{
    public class VisibilityGraphPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.VisibilityGraph; }
        }

        public VisibilityGraphBuilder GraphBuilder { get; private set; }

        public bool IncludeBoundary { get; set; }
        public bool StopOnGoalVertexDiscovery { get; set; }

        #endregion Properties

        #region Public Methods

        public VisibilityGraphPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot)
            : base(factory, cspace, robot)
        {

        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            GraphBuilder = new VisibilityGraphBuilder(GeometryFactory, CSpace.Bounds, CSpace.Obstacles)
                               {
                                   IncludeBoundary = IncludeBoundary,
                                   StopOnGoalVertexDiscovery = StopOnGoalVertexDiscovery
                               };
            Graph = GraphBuilder.CreateVisibilityGraph(StartVertex, GoalVertex);
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

        #endregion Internal Methods
    }
}