// *******************************************************
// Project: RoboPath.UI
// File Name: PolygonSketch.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;

namespace RoboPath.UI.Sketch
{
    public class PolygonSketch : GeometrySketch
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override SketchType SketchType
        {
            get { return SketchType.Polygon; }
        }

        #endregion Properties

        #region Public Methods

        public PolygonSketch()
            : base()
        {
        }

        public override IList<IGeometry> ToGeometry(IGeometryFactory factory)
        {
            if(!IsValid)
            {
                throw new InvalidOperationException("Cannot Convert Input To Geometry");
            }

            Log.Debug("Creating Geometry Type from Sketch [ Polygon ]");
            var polygons = CreatePolygons(factory);
            if(!polygons.Any())
            {
                Log.Error("No Polygons Created from Geometry");
            }
            return polygons.Cast<IGeometry>().ToList();
        }

        #endregion Public Methods

        #region Internal Methods

        private List<IPolygon> CreatePolygons(IGeometryFactory factory)
        {
            var vertices = Vertices.ToClosedRing();
            var polygonBuilder = new ComplexPolygonBuilder(factory, vertices);
            polygonBuilder.BuildPolygons();
            return polygonBuilder.Polygons.ToList();
        }

        protected override bool ValidateVertices()
        {
            if(Vertices.Count < 3)
            {
                return false;
            }
            return true;
        }

        #endregion Internal Methods
    }
}