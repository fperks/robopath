// *******************************************************
// Project: RoboPath.Core
// File Name: ICSpace.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

using GeoAPI.Geometries;

namespace RoboPath.Core.Space
{
    public interface IConfigurationSpace
    {
        event EventHandler<EventArgs> GeometryChanged;

        Envelope Bounds { get; }
        IGeometryFactory GeometryFactory { get; }

        List<IPolygon> Obstacles { get; }
        IGeometry OccupiedSpace { get; }

        List<T> GetObstacleRegions<T>(bool copy=false) where T : class, IGeometry;

        SpaceLocationType QueryLocationType(IPoint position);
        SpaceLocationType QueryLocationType(Coordinate position);
        IGeometry QueryObstacle(IPoint position);
        IGeometry QueryObstacle(Coordinate position);
    }
}