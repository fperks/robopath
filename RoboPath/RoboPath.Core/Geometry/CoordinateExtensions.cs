// *******************************************************
// Project: RoboPath.Core
// File Name: CoordinateExtensions.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

namespace RoboPath.Core.Geometry
{
    public static class CoordinateExtensions
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Public Methods

        public static IPoint ToPoint(this Coordinate coordinate, IGeometryFactory geometryFactory)
        {
            return geometryFactory.CreatePoint(coordinate);
        }

        public static bool IsNull(this Coordinate coordinate)
        {
            return double.IsNaN(coordinate.X) && double.IsNaN(coordinate.Y);
        }

        #region Coordinate Collection Extensions

        public static List<Coordinate> ToClosedRing(this IList<Coordinate> vertices)
        {
            return new List<Coordinate>(vertices) { vertices.First() };
        }

        public static List<Coordinate> ToOpenRing(this IList<Coordinate> vertices)
        {
            if (vertices.Count <= 2)
            {
                throw new ArgumentException("Vertex Collection does not have enough Coordinates to be Opened");
            }

            if (!vertices.IsClosedRing())
            {
                return vertices.ToList();
            }
            return new List<Coordinate>(vertices.Take(vertices.Count - 1));
        }

        public static bool IsClosedRing(this IList<Coordinate> vertices)
        {
            if (vertices.Count <= 3)
            {
                throw new ArgumentException("Input Vertex Collection does not have enough Vertices to be a Ring");
            }
            return vertices.First().Equals(vertices.Last());
        }



        #endregion Coordinate Extensions

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}