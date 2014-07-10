// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: RobotManager.cs
// By: Frank Perks
// *******************************************************

using System;
using System.ComponentModel;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core;
using RoboPath.Core.Geometry;
using RoboPath.Core.Robot;
using RoboPath.Core.Robot.Geometric;
using RoboPath.PlannerApp.Properties;
using RoboPath.PlannerApp.Utility;

namespace RoboPath.PlannerApp.PathPlanning.Robot
{
    public class RobotManager
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ClientServiceLocator _serviceLocator;

        private IGeometry _polygonalBodyGeometry;
        private PathPlannerSetup _plannerSetup;
        private RobotType _currentRobotType;
        private IRobot _currentRobot;

        #endregion Fields

        #region Properties

        public event EventHandler<EventArgs> CurrentRobotChanged;

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

        public RobotType CurrentRobotType
        {
            get { return _currentRobotType; }
            set { OnCurrentRobotTypeChanged(value); }
        }

        public IRobot CurrentRobot
        {
            get
            {
                if(_currentRobot == null)
                {
                    UpdateRobot();
                }
                return _currentRobot;
            }
            private set
            {
                _currentRobot = value;
                NotifyCurrentRobotChanged();
            }
        }

        public IGeometry PolygonalBodyGeometry
        {
            get { return _polygonalBodyGeometry; }
            set { OnPolygonalBodyGeometryChanged(value); }
        }

        public PathPlannerSetup PlannerSetup
        {
            get { return _plannerSetup; }
            set { OnPathPlannerSetupChanged(value); }
        }

        #region Event Filters

        public PropertyChangedEventFilter<double> SettingsCircleRobotBodyRadiusChanged { get; set; }
        public PropertyChangedEventFilter<double> SettingsCyclicRobotBodyRadiusChanged { get; set; }
        public PropertyChangedEventFilter<int> SettingsCyclicRobotBodyEdgeCountChanged { get; set; }

        #endregion Event Filters

        #endregion Properties

        #region Public Methods

        public RobotManager(IGeometryFactory geometryFactory)
        {
            GeometryFactory = geometryFactory;

            // On these IPropertyChangedNotifications we update the current robot
            SettingsCircleRobotBodyRadiusChanged = new PropertyChangedEventFilter<double>(Settings.Default, "RobotBodyCircleRadius", x => UpdateRobot());
            SettingsCyclicRobotBodyRadiusChanged = new PropertyChangedEventFilter<double>(Settings.Default, "RobotBodyCyclicPolygonRadius", x => UpdateRobot());
            SettingsCyclicRobotBodyEdgeCountChanged = new PropertyChangedEventFilter<int>(Settings.Default, "RobotBodyCyclicPolygonEdgeCount", x => UpdateRobot());

            CurrentRobotType = RobotType.Point;
        }

        public void UpdateRobot()
        {
            // Create a new Robot
            var robot = CreateRobot(CurrentRobotType);
            CurrentRobot = robot;       
        }

        #endregion Public Methods

        #region Internal Methods

        #region Robot Creation Methods

        protected virtual void NotifyCurrentRobotChanged()
        {
            if(CurrentRobotChanged != null)
            {
                CurrentRobotChanged(this, new EventArgs());
            }
        }

        private IRobot CreateRobot(RobotType robotType)
        {
            // Create Robot
            IRobot robot;
            switch(robotType)
            {
                case RobotType.Point:
                    robot = CreatePointRobot();
                    break;
                case RobotType.Circle:
                    robot = CreateCircularRobot();
                    break;
                case RobotType.CyclicPolygon:
                    robot = CreateCyclicPolygonRobot();
                    break;
                case RobotType.Polygon:
                    robot = CreatePolygonalRobot();
                    break;
                default:
                    throw new ArgumentException(string.Format("Robot Type [ {0} ] is not Supported", robotType));
            }
            return robot;
        }

        private IRobot CreatePointRobot()
        {
            return new PointRobot(GeometryFactory);
        }

        private IRobot CreateCircularRobot()
        {
            var radius = Settings.Default.RobotBodyCircleRadius;
            return new CircularRobot(GeometryFactory, radius);
        }

        private IRobot CreateCyclicPolygonRobot()
        {
            var radius = Settings.Default.RobotBodyCyclicPolygonRadius;
            var edgeCount = Settings.Default.RobotBodyCyclicPolygonEdgeCount;
            var geometry = GeometryBuilder.CreateCyclicPolygon(GeometryFactory, Constants.RobotBodyCenter, edgeCount, radius);
            return new PolygonalRobot(GeometryFactory, geometry);
        }

        private IRobot CreatePolygonalRobot()
        {
            if(PolygonalBodyGeometry == null)
            {
                throw new InvalidOperationException("Cannot Create Polygonal Robot when its Body is Null");
            }
            return new PolygonalRobot(GeometryFactory, PolygonalBodyGeometry);
        }

        #endregion Robot Creation Methods

        #region Value Changed Methods

        protected virtual void OnPathPlannerSetupChanged(PathPlannerSetup newSetup)
        {
            _plannerSetup = newSetup;
            UpdateRobot();
        }

        protected virtual void OnPolygonalBodyGeometryChanged(IGeometry newBody)
        {
            if(newBody != null)
            {
                newBody = newBody.TransformPosition(Constants.RobotBodyCenter);
            }

            Log.Debug("Setting Polygonal Body Geometry to [ {0} ]", newBody);

            _polygonalBodyGeometry = newBody;
            if(CurrentRobotType == RobotType.Polygon)
            {
                UpdateRobot();
            }
        }

        protected virtual void OnCurrentRobotTypeChanged(RobotType newValue)
        {
            if(CurrentRobotType == newValue)
            {
                return;
            }

            _currentRobotType = newValue;
            UpdateRobot();
        }

        #endregion Value Changed Methods

        #endregion Internal Methods
    }
}