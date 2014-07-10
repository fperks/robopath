// *******************************************************
// Project: RoboPath.Core
// File Name: CircularRobot.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Robot.Geometric
{
    public class CircularRobot : GeometricRobot
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override RobotBodyType BodyType
        {
            get { return RobotBodyType.Circle; }
        }

        public double Radius { get; private set; }

        #endregion Properties

        #region Public Methods

        public CircularRobot(IGeometryFactory geometryFactory, double radius)
            : base(geometryFactory)
        {
            Radius = radius;
            Geometry = CreateBodyGeometry();            
        }

        public override string ToString()
        {
            return string.Format("Robot[BodyType={0},Geometry={1},Radius={2}]", BodyType, Geometry, Radius);
        }

        #endregion Public Methods

        #region Internal Methods
        
        private IGeometry CreateBodyGeometry()
        {
            var geometry = GeometryBuilder.CreateCyclicPolygon(GeometryFactory, Constants.RobotBodyCenter, Constants.DefaultCircleGeometryEdgeCount, Radius);
            return geometry;
        }

        #endregion Internal Methods
    }
}