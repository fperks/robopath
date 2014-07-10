// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ConfigurationSpaceFactory.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;

using NLog;

using NetTopologySuite.Operation.Buffer;

using RoboPath.Core.Algorithms;
using RoboPath.Core.Algorithms.Decomposition;
using RoboPath.Core.Algorithms.SafetyBuffer;
using RoboPath.Core.Geometry;
using RoboPath.Core.Robot;
using RoboPath.Core.Robot.Geometric;
using RoboPath.Core.Space;
using RoboPath.PlannerApp.Properties;

namespace RoboPath.PlannerApp.PathPlanning.Space
{
    public class ConfigurationSpaceFactory
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public IWorkspace Basis { get; private set; }
        public IPolygonDecomposition PolygonDecompositionStrategy { get; private set; }
        public IRobot Robot { get; private set; }

        #endregion Properties

        #region Public Methods

        public static IConfigurationSpace CreateConfigurationSpace(IGeometryFactory geometryFactory, IWorkspace basis, IRobot robot, IPolygonDecomposition strategy)
        {
            var cspaceFactory = new ConfigurationSpaceFactory(geometryFactory, basis, robot, strategy);
            return cspaceFactory.CreateConfigurationSpace();
        }

        public ConfigurationSpaceFactory(IGeometryFactory geometryFactory, IWorkspace basis, IRobot robot, IPolygonDecomposition strategy)
        {
            GeometryFactory = geometryFactory;
            Basis = basis;
            Robot = robot;
            PolygonDecompositionStrategy = strategy;
        }

        public IConfigurationSpace CreateConfigurationSpace()
        {
            Log.Debug("Creating CSpace");
            IList<IPolygon> bufferedObstacles;
            switch(Robot.BodyType)
            {
                case RobotBodyType.Point:
                    bufferedObstacles = ApplyPointSafetyBuffer((PointRobot) Robot);
                    break; 
                case RobotBodyType.Circle:
                    bufferedObstacles = ApplyCircularSafetyBuffer((CircularRobot) Robot);
                    break;
                case RobotBodyType.Polygon:
                    bufferedObstacles = ApplyPolygonalSafetyBuffer((PolygonalRobot) Robot);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unsupported Robot Body Type [ {0} ]", Robot.BodyType));
            }

            var obstacles = bufferedObstacles.GetPolygons();
            var cspace = new ConfigurationSpace(GeometryFactory, Basis.Bounds, obstacles);
            return cspace;
        }

        #endregion Public Methods

        #region Internal Methods

        private IList<IPolygon> ApplyPointSafetyBuffer(PointRobot robot)
        {
            return Basis.GetObstacleRegions<IPolygon>(true);
        }

        private IList<IPolygon> ApplyCircularSafetyBuffer(CircularRobot robot)
        {
            var safetyBuffer = new CircularSafetyBuffer(GeometryFactory, robot, Basis.Obstacles)
                                   {
                                       CircleEdgeCount = Settings.Default.CircleEdgeCount
                                   };
            safetyBuffer.Solve();
            return safetyBuffer.Result.GetPolygons();
        }

        private IList<IPolygon> ApplyPolygonalSafetyBuffer(PolygonalRobot robot)
        {
            var safetyBuffer = new PolygonalSafetyBuffer(GeometryFactory, robot, Basis.Obstacles, PolygonDecompositionStrategy);
            safetyBuffer.Solve();
            return safetyBuffer.Result;
        }

        #endregion Internal Methods  
    }
}