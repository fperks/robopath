// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerSetup.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Algorithms;
using RoboPath.Core.Algorithms.Decomposition;
using RoboPath.Core.Geometry;
using RoboPath.Core.PathPlanning;
using RoboPath.Core.PathPlanning.Geometric;
using RoboPath.Core.PathPlanning.Geometric.Decomposition;
using RoboPath.Core.PathPlanning.Sampling;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;
using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.PathPlanning.Robot;
using RoboPath.PlannerApp.PathPlanning.Space;
using RoboPath.PlannerApp.Properties;
using RoboPath.PlannerApp.Utility;

namespace RoboPath.PlannerApp.PathPlanning
{
    
    public class PathPlannerSetup : INotifyPropertyChanged
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IWorkspace _workspace;
        private IConfigurationSpace _cspace;
        private IRobot _robot;
        
        private Coordinate _startPosition;
        private Coordinate _goalPosition;

        private IRobotConfiguration _startConfiguration;
        private IRobotConfiguration _goalConfiguration;

        private IPolygonDecomposition _polygonDecomposition;

        private ClientServiceLocator _serviceLocator;

        #endregion Fields

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public ClientServiceLocator ServiceLocator
        {
            get
            {
                if(_serviceLocator == null)
                {
                    _serviceLocator = new ClientServiceLocator();
                }
                return _serviceLocator;
            }
        }

        public IGeometryFactory GeometryFactory { get; private set; }

        public Envelope Bounds
        {
            get { return Workspace.Bounds; }
        }

        public IConfigurationSpace CurrentEnvironment
        {
            get
            {
                if(ConfigurationSpace == null)
                {
                    return Workspace;
                }
                return ConfigurationSpace;
            }
        }
        public IWorkspace Workspace
        {
            get { return _workspace; }
            set { OnWorkspaceChanged(value); }
        }

        public IConfigurationSpace ConfigurationSpace
        {
            get { return _cspace; }
            set { OnConfigurationSpaceChanged(value); }
        }

        public IRobot Robot
        {
            get { return _robot; }
            set { OnRobotChanged(value); }
        }

        public Coordinate StartPosition
        {
            get { return _startPosition; }
            set { OnStartPositionChanged(value); }
        }

        public Coordinate GoalPosition
        {
            get { return _goalPosition; }
            set { OnGoalPositionChanged(value); }
        }

        public IRobotConfiguration StartConfiguration
        {
            get { return _startConfiguration; }
            private set { OnStartConfigurationChanged(value); }
        }

        public IRobotConfiguration GoalConfiguration
        {
            get { return _goalConfiguration; }
            private set { OnGoalConfigurationChanged(value); }
        }

        public IList<IGeometry> DecomposedObstaclePolygons { get; private set; }

        public IPolygonDecomposition PolygonDecompositionStrategy
        {
            get { return _polygonDecomposition; }
            set { OnPolygonDecompositionStrategyChanged(value); }
        }

        public PropertyChangedEventFilter<bool> SettingsIsDecompositionVisibleFilter { get; private set; }  

        #endregion Properties

        #region Public Methods

        public PathPlannerSetup(IGeometryFactory geometryFactory)
        {
            GeometryFactory = geometryFactory;
            _polygonDecomposition = new EarClipPolygonDecomposition(GeometryFactory);
            SettingsIsDecompositionVisibleFilter = new PropertyChangedEventFilter<bool>(Settings.Default, "IsObstacleDecompositionVisible", OnSettingIsDecompositionVisibleChanged);
        }

        public void Initialize()
        {
            var robotManager = ServiceLocator.RobotManager;
            robotManager.CurrentRobotChanged += OnCurrentRobotChanged;

            if(Robot == null)
            {
                Robot = robotManager.CurrentRobot;
            }

            // Register callbacks to factory
            var pathPlannerManager = ServiceLocator.PathPlannerManager;
            foreach(var item in GetSupportedPathPlannerTypes())
            {
                pathPlannerManager.Register(item.Key, item.Value);
            }

            GenerateDecomposedPolygons();
        }

        public void ComputeConfigurationSpace()
        {
            Log.Debug("Computing Configuration Space");
            if(Robot.BodyType == RobotBodyType.Polygon)
            {
                GenerateDecomposedPolygons();            
            }

            var cspaceFactory = new ConfigurationSpaceFactory(GeometryFactory, Workspace, Robot, PolygonDecompositionStrategy);
            var cspace = cspaceFactory.CreateConfigurationSpace();
            ConfigurationSpace = cspace;
        }

        public void ClearConfigurationSpace()
        {
            if(ConfigurationSpace == null)
            {
                return;
            }

            Log.Debug("Clearing Configuration Space");
            ConfigurationSpace = null;
        }

        public bool Validate()
        {
            //Log.Debug("Attempting to Validate PathPlannerSetup");
            //var isValid = true;
            //isValid &= StartPosition != null && GoalPosition != 
            //isValid &= GoalPosition != null;

            
            //if(StartPosition == null)
            //{
                
            //}

            //if(StartPosition != null)
            //{
            //    var location = CurrentEnvironment.QueryLocationType(StartPosition);
            //    if(location != SpaceLocationType.FreeSpace)
            //    {                    
            //    }
            //}

            //if(GoalPosition != null)
            //{
                
            //}
            
            throw new NotImplementedException();
        }
        
        #endregion Public Methods

        #region Internal Methods

        private PathPlannerSetup()
        {            
        }

        private Dictionary<PathPlannerAlgorithmType, Func<IPathPlanner>> GetSupportedPathPlannerTypes()
        {
            return new Dictionary<PathPlannerAlgorithmType, Func<IPathPlanner>>()
                       {
                           {
                               PathPlannerAlgorithmType.VisibilityGraph,
                               () =>
                                   {
                                       return new VisibilityGraphPlanner(GeometryFactory, ConfigurationSpace, Robot)
                                                  {
                                                      IncludeBoundary = Settings.Default.AlgorithmVisibilityGraphIncludeBoundary,
                                                  };
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.Triangulation,
                               () =>
                                   {
                                       return new TriangulationPathPlanner(GeometryFactory, ConfigurationSpace, Robot);
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.GeneralizedVoronoiDiagram,
                               () =>
                                   {
                                       return new GeneralizedVoronoiDiagramPlanner(GeometryFactory, ConfigurationSpace, Robot, Settings.Default.AlgorithmVoronoiSampleDistance);
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.GridSampling,
                               () =>
                                   {
                                       return new GridSamplingPlanner(GeometryFactory, ConfigurationSpace, Robot)
                                                  {
                                                      SamplingDistance = Settings.Default.AlgorithmGridSamplingDistance
                                                  };
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.ProbalisticRoadMap,
                               () =>
                                   {
                                       return new PRMPlanner(GeometryFactory, ConfigurationSpace, Robot,
                                                             Settings.Default.AlgorithmPRMSampleCount,
                                                             Settings.Default.AlgorithmPRMNeighbourCount);
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.RRT,
                               () =>
                                   {
                                       return new RRTPlanner(GeometryFactory, ConfigurationSpace, Robot,
                                                             Settings.Default.AlgorithmRRTNeighbourCount,
                                                             Settings.Default.AlgorithmRRTStepSize)
                                                  {
                                                      StopAtGoal = Settings.Default.AlgorithmRRTStopAtGoal
                                                  };
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.GreedyRRT,
                               () =>
                                   {
                                       return new GreedyRRTPlanner(GeometryFactory, ConfigurationSpace, Robot,
                                                                   Settings.Default.AlgorithmRRTNeighbourCount,
                                                                   Settings.Default.AlgorithmRRTStepSize,
                                                                   Settings.Default.AlgorithmRRTGoalBias)
                                                  {
                                                      StopAtGoal = Settings.Default.AlgorithmRRTStopAtGoal
                                                  };
                                   }
                           },
                           {
                               PathPlannerAlgorithmType.DualRRT,
                               () =>
                                   {
                                       return new DualRRTPlanner(GeometryFactory, ConfigurationSpace, Robot,
                                                                   Settings.Default.AlgorithmRRTNeighbourCount,
                                                                   Settings.Default.AlgorithmRRTStepSize,
                                                                   Settings.Default.AlgorithmRRTGoalBias);
                                   }
                           },
                           {
                                PathPlannerAlgorithmType.TrapezoidalDecomposition, 
                                () =>
                                    {
                                        return new TrapezoidalDecompositionPlanner(GeometryFactory, ConfigurationSpace, Robot);
                                    }
                           },

                       };
        }

        private void GenerateDecomposedPolygons()
        {
            if(Workspace == null)
            {
                return;
            }

            if(!Settings.Default.IsObstacleDecompositionVisible)
            {
                DecomposedObstaclePolygons = new List<IGeometry>();
                DrawLayer(MapLayers.DecomposedPolygonsLayer);
                return;
            }

            var decomposedPolygons = new List<IPolygon>();
            var polygonObstacles = Workspace.Obstacles;

            // Generate decomposed polygons for rendering
            foreach(var obstacle in polygonObstacles)
            {
                if(obstacle.IsConvex())
                {
                    decomposedPolygons.Add(obstacle);
                }
                else
                {
                    // its not convex or has complex holes
                    var decomposed = PolygonDecompositionStrategy.DecomposePolygon(obstacle);    
                    decomposedPolygons.AddRange(decomposed);
                }               
            }

            DecomposedObstaclePolygons = new List<IGeometry>(decomposedPolygons);
            DrawLayer(MapLayers.DecomposedPolygonsLayer);
        }
        
        private void DrawLayer(string name)
        {
            var renderer = ServiceLocator.MapRenderer;
            if(renderer != null)
            {
                renderer.Draw();
                
                //renderer.DrawLayer(name);
            }
        }

        private void NotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        
        #region Value Changed Methods

        protected virtual void OnCurrentRobotChanged(object source, EventArgs args)
        {
            var robotManager = (RobotManager) source;
            Robot = robotManager.CurrentRobot;
        }

        protected virtual void OnWorkspaceGeometryChanged(object source, EventArgs args)
        {
            if(ConfigurationSpace != null)
            {
                ComputeConfigurationSpace();
            }
            
            GenerateDecomposedPolygons();
            var renderer = ServiceLocator.MapRenderer;
            renderer.Draw();
        }

        protected virtual void OnWorkspaceChanged(IWorkspace newWorkspace)
        {
            Log.Debug("Workspace Changed [ {0} -> {1} ]", Workspace, newWorkspace);

            if(Workspace != null)
            {
                Workspace.GeometryChanged -= OnWorkspaceGeometryChanged;
            }
            _workspace = newWorkspace;
            if(Workspace != null)
            {
                Workspace.GeometryChanged += OnWorkspaceGeometryChanged;
            }

            ClearConfigurationSpace();
            DrawLayer(MapLayers.WorkspaceLayer);
        }

        protected virtual void OnConfigurationSpaceChanged(IConfigurationSpace newConfigurationSpace)
        {
            Log.Debug("CSpace Changed [ {0} -> {1} ]", ConfigurationSpace, newConfigurationSpace);
            _cspace = newConfigurationSpace;

            if(ConfigurationSpace == null)
            {
                ServiceLocator.MapRenderer.RenderState = MapRenderState.Initial;
            }

            DrawLayer(MapLayers.CSpaceLayer);
        }

        protected virtual void OnRobotChanged(IRobot newRobot)
        {
            Log.Debug("Robot Changed [ {0} -> {1} ]", Robot, newRobot);
            _robot = newRobot;

            // Update our configurations
            if(Robot != null)
            {
                if(StartPosition != null)
                {
                    StartConfiguration = Robot.GetConfiguration(StartPosition);    
                }
                
                if(GoalPosition != null)
                {
                    GoalConfiguration = Robot.GetConfiguration(GoalPosition);    
                }
            }
            
            if(ConfigurationSpace != null)
            {
                ComputeConfigurationSpace();    
            }
            
            DrawLayer(MapLayers.StartAndGoalLayer);
        }

        protected virtual void OnStartPositionChanged(Coordinate newPosition)
        {
            Log.Debug("Start Position Changed from [ {0} ] to [ {1} ]", StartPosition, newPosition);
            _startPosition = newPosition;

            if(StartPosition != null && Robot != null)
            {
                StartConfiguration = Robot.GetConfiguration(StartPosition);
            }

            DrawLayer(MapLayers.StartAndGoalLayer);
        }

        protected virtual void OnGoalPositionChanged(Coordinate newPosition)
        {
            Log.Debug("Goal Position Changed from [ {0} ] to [ {1} ]", GoalPosition, newPosition);
            _goalPosition = newPosition;

            if(GoalPosition != null && Robot != null)
            {
                GoalConfiguration = Robot.GetConfiguration(GoalPosition);
            }

            DrawLayer(MapLayers.StartAndGoalLayer);
            
        }

        protected virtual void OnStartConfigurationChanged(IRobotConfiguration newValue)
        {
            _startConfiguration = newValue;
            DrawLayer(MapLayers.StartAndGoalLayer);
        }

        protected virtual void OnGoalConfigurationChanged(IRobotConfiguration newValue)
        {
            _goalConfiguration = newValue;
            DrawLayer(MapLayers.StartAndGoalLayer);
        }

        protected virtual void OnPolygonDecompositionStrategyChanged(IPolygonDecomposition newStrategy)
        {
            _polygonDecomposition = newStrategy;        

            if(ConfigurationSpace != null)
            {
                ComputeConfigurationSpace();   
            }
        }

        protected virtual void OnSettingIsDecompositionVisibleChanged(bool value)
        {
            GenerateDecomposedPolygons();
        }

        #endregion Value Changed Methods
        #endregion Internal Methods
    }
}