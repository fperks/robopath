// *******************************************************
// Project: RoboPath.UI
// File Name: GeometrySketchFactory.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

namespace RoboPath.UI.Sketch
{
    public class GeometrySketchFactory : IGeometrySketchFactory
    {
        #region Fields
       
        #endregion Fields

        #region Properties

        public IDictionary<SketchType, Func<IGeometrySketch>> SketchDefinitions { get; private set; }

        #endregion Properties

        #region Public Methods

        public GeometrySketchFactory()
        {
            SketchDefinitions = new Dictionary<SketchType, Func<IGeometrySketch>>()
                                    {
                                        { SketchType.Polygon, () => new PolygonSketch() },
                                    };
        }

        public IGeometrySketch CreateSketch(SketchType sketchType)
        {
            if(!SketchDefinitions.ContainsKey(sketchType))
            {
                throw new ArgumentException(string.Format("Sketch Type is Not Supported by the factory [ {0} ]", sketchType));
            }
            return SketchDefinitions[sketchType]();
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}