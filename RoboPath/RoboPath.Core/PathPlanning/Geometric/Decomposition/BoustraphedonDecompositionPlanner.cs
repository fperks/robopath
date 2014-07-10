// *******************************************************
// Project: RoboPath.Core
// File Name: BoustraphedonDecompositionPlanner.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Geometric.Decomposition
{
    public class BoustraphedonDecompositionPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IRoadMapBuilder _graphBuilder;

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.BoustraphedonDecomposition; }
        }

        public List<IGeometry> Regions { get; private set; }

        public List<ILineString> TrapezoidalEdges { get; private set; } 

        #endregion Properties

        #region Public Methods

        public BoustraphedonDecompositionPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot)
            : base(factory, cspace, robot)
        {
            _graphBuilder = new RoadMapBuilder(factory, cspace.Bounds, cspace.OccupiedSpace);
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            Regions = new List<IGeometry>();
            TrapezoidalEdges  = new List<ILineString>();
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

        private void CreateRegions()
        {
            foreach(var obstacle in CSpace.Obstacles)
            {
                
            }
        }

        private void ProcessObstacle(IPolygon region)
        {

        }
        
        #endregion Internal Methods
    }
}