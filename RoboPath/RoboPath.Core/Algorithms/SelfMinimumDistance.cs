// *******************************************************
// Project: RoboPath.Core
// File Name: SelfMinimumDistance.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;

namespace RoboPath.Core.Algorithms
{
    public class SelfMinimumDistance
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public IPolygon SourcePolygon { get; private set; }

        public double MinimumDistance { get; private set; }

        public List<LineSegment> Segments { get; private set; }

        #endregion Properties

        #region Public Methods

        public SelfMinimumDistance(IGeometryFactory geometryFactory, IPolygon input)
        {
            GeometryFactory = geometryFactory;
            SourcePolygon = input;
            MinimumDistance = double.NaN;
            Segments = new List<LineSegment>();
        }

        public void ComputeMinimumDistance()
        {
            Log.Debug("Computining Minimum Self Distance");
            
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}