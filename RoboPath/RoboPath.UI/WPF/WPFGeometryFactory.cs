// *******************************************************
// Project: RoboPath.UI
// File Name: WPFGeometryFactory.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;
using WPFGeometry = System.Windows.Media.Geometry;

using GeoAPI.Geometries;

using NetTopologySuite.Windows.Media;

using RoboPath.Core.Geometry;

namespace RoboPath.UI.WPF
{
    public static class WPFGeometryFactory
    {

        #region Fields

        #endregion Fields

        #region Public Methods

        public static WPFGeometry CreateWPFPolygon(IGeometryFactory geometryFactory, IList<Coordinate> vertices)
        {
            var polygon = GeometryOperations.FastPolygonize(geometryFactory, vertices);
            var writer = new WpfStreamGeometryWriter();
            return writer.ToShape(polygon);
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods


    }
}