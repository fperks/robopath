// *******************************************************
// Project: RoboPath.Core
// File Name: PolygonalRobot.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

using NLog;

namespace RoboPath.Core.Robot.Geometric
{
    public class PolygonalRobot : GeometricRobot
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override RobotBodyType BodyType
        {
            get { return RobotBodyType.Polygon; }
        }

        #endregion Properties

        #region Public Methods

        public PolygonalRobot(IGeometryFactory geometryFactory, IGeometry geometry)
            : base(geometryFactory, geometry)
        {
        }

        #endregion Public Methods
    }
}