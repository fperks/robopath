// *******************************************************
// Project: RoboPath.Core
// File Name: GeometryBuilder.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

namespace RoboPath.Core.Geometry
{
    public static class GeometryBuilder
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Public Methods

        public static IGeometry CreateCyclicPolygon(IGeometryFactory geometryFactory, Coordinate center, int edgeCount, double radius)
        {
            Log.Debug("Creating Polygon [EdgeCount={0},Radius={1}]", edgeCount, radius);
            var vertices = new List<Coordinate>();
            var angle = 2 * Math.PI / edgeCount;
            for(var i = 0; i < edgeCount; i++)
            {
                var x = radius * Math.Cos(angle * i) + center.X;
                var y = radius * Math.Sin(angle * i) + center.Y;
                var coordinate = new Coordinate(x, y);
                vertices.Add(coordinate);
            }

            var ring = geometryFactory.CreateLinearRing(vertices.ToClosedRing().ToArray());
            var geometry = geometryFactory.CreatePolygon(ring);
            return geometry;
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods


    }
}