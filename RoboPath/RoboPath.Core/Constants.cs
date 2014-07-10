// *******************************************************
// Project: RoboPath.Core
// File Name: Constants.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

namespace RoboPath.Core
{
    public static class Constants
    {
        public const double MachineEpsilon = 1.0E-4;

        public const int DefaultCircleGeometryEdgeCount = 32;
        public const PrecisionModels DefaultPrecisionModelType = PrecisionModels.Floating;
        public const double CoordinateUniquenessMinDistance = 0.0001;
        public const double MinimumAreaForValidPolygon = 1.0;
        public const double SimplifyTolerance = 5.0;

        public static Coordinate RobotBodyCenter { get; private set; }

        static Constants()
        {
            RobotBodyCenter = new Coordinate(0.0, 0.0);
        }
    }
}