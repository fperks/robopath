// *******************************************************
// Project: RoboPath.Core
// File Name: PointRobot.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

using NLog;

namespace RoboPath.Core.Robot.Geometric
{
    public class PointRobot : GeometricRobot
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override RobotBodyType BodyType
        {
            get { return RobotBodyType.Point; }
        }

        #endregion Properties

        #region Public Methods

        public PointRobot(IGeometryFactory geometryFactory)
            : base(geometryFactory)
        {
            Geometry = CreateBodyGeometry();
        }

        #endregion Public Methods

        #region Internal Methods
        
        private IGeometry CreateBodyGeometry()
        {
            return GeometryFactory.CreatePoint(Constants.RobotBodyCenter);
        }

        #endregion Internal Methods
    }
}