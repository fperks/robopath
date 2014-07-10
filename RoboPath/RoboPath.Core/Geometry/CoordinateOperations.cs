// *******************************************************
// Project: RoboPath.Core
// File Name: CoordinateOperations.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

namespace RoboPath.Core.Geometry
{
    public static class CoordinateOperations
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Public Methods

        public static Coordinate CreateRandomCoordinate(int maxX, int maxY)
        {
            var generator = new Random();
            return new Coordinate(generator.Next(0, maxX), generator.Next(0, maxY));            
        }

        public static Coordinate CreateNullCoordinate()
        {
            return new Coordinate(double.NaN, double.NaN);
        }

        public static Coordinate Add(Coordinate lhs, Coordinate rhs)
        {
            return new Coordinate(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Coordinate Subtract(Coordinate lhs, Coordinate rhs)
        {
            return new Coordinate(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static List<Tuple<Coordinate, Coordinate>> ToVertexEdgePairs(this List<Coordinate> input, bool isRing = true)
        {
            var result = input.Zip(input.Skip(1), (source, target) => new Tuple<Coordinate, Coordinate>(source, target)).ToList();
            if(isRing)
            {
                result.Add(new Tuple<Coordinate, Coordinate>(result.Last().Item2, input.First()));
            }
            return result;
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}