// *******************************************************
// Project: RoboPath.Core
// File Name: GeometricRobot.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Robot
{
    public abstract class GeometricRobot : IRobot
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public IGeometry Geometry { get; protected set; } 
        
        public bool IsPoint
        {
            get { return BodyType == RobotBodyType.Point; }
        }

        public bool IsCircular
        {
            get { return BodyType == RobotBodyType.Circle; }
        }

        public bool IsPolygonal
        {
            get { return BodyType == RobotBodyType.Polygon; }
        }

        public abstract RobotBodyType BodyType { get; }
        
        #endregion Properties

        #region Public Methods
        
        protected GeometricRobot(IGeometryFactory geometryFactory)
        {
            GeometryFactory = geometryFactory;
        }

        protected GeometricRobot(IGeometryFactory geometryFactory, IGeometry body)
        {
            GeometryFactory = geometryFactory;
            Geometry = body;
        }

        public virtual IRobotConfiguration GetConfiguration(Coordinate position)
        {
            var transformed = Geometry.TransformPosition(position);
            var configuration = new RobotConfiguration(this, transformed, position);
            return configuration;
        }

        public override string ToString()
        {
            return string.Format("Robot[BodyType={0},Geometry={1}]", BodyType, Geometry);
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}