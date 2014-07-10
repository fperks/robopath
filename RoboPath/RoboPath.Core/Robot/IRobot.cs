// *******************************************************
// Project: RoboPath.Core
// File Name: IRobot.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

namespace RoboPath.Core.Robot
{
    public interface IRobot
    {
        RobotBodyType BodyType { get; }
        IGeometryFactory GeometryFactory { get; }
        IGeometry Geometry { get; }

        bool IsCircular { get; }
        bool IsPoint { get; }
        bool IsPolygonal { get; }

        IRobotConfiguration GetConfiguration(Coordinate position);        
    }
}