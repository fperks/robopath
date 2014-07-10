// *******************************************************
// Project: RoboPath.UI
// File Name: GeometrySketch.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

namespace RoboPath.UI.Sketch
{
    public abstract class GeometrySketch : IGeometrySketch
    {
        #region Fields

        #endregion Fields

        #region Properties

        public abstract SketchType SketchType { get; }

        public bool IsValid
        {
            get { return ValidateVertices(); }
        }

        public IList<Coordinate> Vertices { get; protected set; }

        #endregion Properties

        #region Public Methods

        protected GeometrySketch()
        {
            Vertices = new List<Coordinate>();
        }

        public virtual void AddVertex(Coordinate vertex)
        {
            if(ValidateVertex(vertex))
            {
                Vertices.Add(vertex);
            }            
        }

        public void Clear()
        {
            Vertices.Clear();
        }

        public abstract IList<IGeometry> ToGeometry(IGeometryFactory factory);

        #endregion Public Methods

        #region Internal Methods

        protected abstract bool ValidateVertices();

        protected virtual bool ValidateVertex(Coordinate vertex)
        {
            return true;
        }

        #endregion Internal Methods
    }
}