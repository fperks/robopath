// *******************************************************
// Project: RoboPath.Core
// File Name: BasePathPlanner.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;

using GeoAPI.Geometries;

using NLog;

using QuickGraph;

using RoboPath.Core.Geometry;
using RoboPath.Core.Graph;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning
{
    public abstract class BasePathPlanner : IPathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IRobotConfiguration _startConfiguration;
        private IRobotConfiguration _goalConfiguration;

        private Coordinate _startVertex;
        private Coordinate _goalVertex;

        private IRobot _robot;
        
        #endregion Fields

        #region Properties

        public abstract PathPlannerAlgorithmType AlgorithmType { get; }

        public IGeometryFactory GeometryFactory { get; protected set; }
        public IConfigurationSpace CSpace { get; protected set; }

        public IGeometry OccupiedRegions
        {
            get { return CSpace.OccupiedSpace; }
        }

        public IRobot Robot
        {
            get { return _robot; }
            set
            {
                OnRobotChanged(value);
            }
        }
        public Coordinate StartVertex
        {
            get { return _startVertex; }
            set { OnStartChanged(value); }
        }
        public Coordinate GoalVertex
        {
            get { return _goalVertex; }
            set { OnGoalChanged(value); }
        }

        public IRobotConfiguration StartConfiguration
        {
            get { return _startConfiguration; }
        }

        public IRobotConfiguration GoalConfiguration
        {
            get { return _goalConfiguration; }
        }

        public PathPlannerAlgorithmState State { get; protected set; }
        public ShortestPathAlgorithm ShortestPathAlgorithm { get; set; }

        public IRoadMap Graph { get; protected set; }

        public List<IEdge<Coordinate>> ShortestPath { get; protected set; }

        #endregion Properties

        #region Public Methods

        protected BasePathPlanner(IGeometryFactory geometryFactory, IConfigurationSpace cspace, IRobot robot)
        {
            GeometryFactory = geometryFactory;
            CSpace = cspace;

            Graph = new RoadMap(GeometryFactory);
            Robot = robot;
            State = PathPlannerAlgorithmState.Initialized;
            ShortestPathAlgorithm = ShortestPathAlgorithm.Dijkstra;
        }

        public void Initialize()
        {
            Log.Debug("Initializing {0} Path Planner [StartVertex={1},GoalVertex={2}]", AlgorithmType, StartVertex, GoalVertex);
            if(Validate())
            {
                State = PathPlannerAlgorithmState.Initialized;  
            }
            else
            {
                throw new PathPlannerException("Path planner is not valid");
            }
            OnInitialization();
        }

        public PathPlannerAlgorithmState Solve()
        {
            if(State != PathPlannerAlgorithmState.Initialized)
            {
                Initialize();
            }

            State = OnSolve();
            return State;
        }

        public virtual bool Validate()
        {
            if(StartVertex == null || GoalVertex == null)
            {
                throw new PathPlannerException("A Start and Goal Vertex is Required");
            }

            if(OccupiedRegions.LocateVertex(StartVertex) == Location.Interior)
            {
                throw new PathPlannerException("Start Vertex is in an Invalid Location [ {0} ]", StartVertex);    
            }

            if(OccupiedRegions.LocateVertex(GoalVertex) == Location.Interior)
            {
                throw new PathPlannerException("Goal Vertex is in an Invalid Location [ {0} ]", StartVertex);    
            }
            return OnValidate();
        }

        #endregion Public Methods

        #region Internal Methods

        private void UpdateConfigurations()
        {
            if(Robot != null && StartVertex != null)
            {
                _startConfiguration = Robot.GetConfiguration(StartVertex);
            }

            if(Robot != null && GoalVertex != null)
            {
                _goalConfiguration = Robot.GetConfiguration(GoalVertex);
            }
        }

        protected abstract void OnInitialization();
        protected abstract PathPlannerAlgorithmState OnSolve();
        protected abstract bool OnValidate();

        protected virtual void OnStartChanged(Coordinate newStart)
        {
            _startVertex = newStart;
            UpdateConfigurations();
        }

        protected virtual void OnGoalChanged(Coordinate newGoal)
        {
            _goalVertex = newGoal;
            UpdateConfigurations();            
        }

        protected virtual void OnRobotChanged(IRobot robot)
        {
            _robot = robot;
        }

        #endregion Internal Methods
    }
}