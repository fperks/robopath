// *******************************************************
// Project: RoboPath.Core
// File Name: RobotConfiguration.cs
// By: Frank Perks
// *******************************************************

using System.Linq;

using GeoAPI.Geometries;

using RoboPath.Core.Space;

namespace RoboPath.Core.Robot
{
    public class RobotConfiguration : IRobotConfiguration
    {
        #region Properties

        public IRobot Robot { get; private set; }
        public IGeometry Geometry { get; private set; }
        public Coordinate Position { get; private set; }

        #endregion Properties

        #region Public Methods

        public RobotConfiguration(IRobot robot, IGeometry geometry, Coordinate position)
        {
            Robot = robot;
            Geometry = geometry;
            Position = position;
        }

        public bool IsValid(IConfigurationSpace space)
        {
            
            if(!space.Obstacles.Any())
            {
                return true;
            }
            var isValid = true;
            var occupied = space.OccupiedSpace;
            if(occupied.Intersects(Geometry))
            {
                // check to see if they only touch
                if(!occupied.Touches(Geometry))
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        public override string ToString()
        {
            return string.Format("RobotConfiguration[Position={0},Geometry={1},{2}]", Position, Geometry.AsText(), Robot);
        }

        #endregion Public Methods
    }
}