// *******************************************************
// Project: RoboPath.Core
// File Name: EarClipPolygonDecomposition.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Algorithms.Triangulation.EarClipping;
using RoboPath.Core.Geometry;

namespace RoboPath.Core.Algorithms.Decomposition
{
    public class EarClipPolygonDecomposition : IPolygonDecomposition
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public bool ShouldApplyEdgeFlipping { get; set; }

        #endregion Properties

        #region Public Methods

        public EarClipPolygonDecomposition(IGeometryFactory geometryFactory)
        {
            ShouldApplyEdgeFlipping = false;
            GeometryFactory = geometryFactory;
        }

        public IList<IPolygon> DecomposePolygon(IPolygon inputPolygon)
        {
            // Validate the Input Polygon
            if(inputPolygon.IsEmpty)
            {
                throw new GeometryOperationException("Input Geometry is Empty");
            }

            // Create a new Ear Clipping Triangulation
            var earClipper = new EarClippingTriangulation(GeometryFactory, inputPolygon);
            var triangulated = earClipper.Triangulate(ShouldApplyEdgeFlipping);
            var result = triangulated.GetPolygons();
            return result;
        }

        #endregion Public Methods

    }
}