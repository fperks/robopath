// *******************************************************
// Project: RoboPath.Core
// File Name: IRobotConfiguration.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

using RoboPath.Core.Space;

namespace RoboPath.Core.Robot
{
    public interface IRobotConfiguration
    {
        IRobot Robot { get; }
        Coordinate Position { get; }
        IGeometry Geometry { get; }

        bool IsValid(IConfigurationSpace space);
    }
}