// *******************************************************
// Project: RoboPath.Core
// File Name: EarTriangle.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

namespace RoboPath.Core.Algorithms.Triangulation.EarClipping
{
    public class EarTriangle
    {
        #region Fields

        private List<int> _vertices;

        #endregion Fields

        #region Properties

        public IList<int> Vertices
        {
            get { return _vertices.ToList(); }
            private set { _vertices = value.ToList(); }
        }

        #endregion Properties

        #region Public Methods

        public EarTriangle(int v0, int v1, int v2)
        {
            Vertices = new List<int>() {v0, v1, v2};
        }

        public void SetVertices(int v0, int v1, int v2)
        {
            Vertices = new List<int>() {v0, v1, v2};
        }

        public IList<int> GetSharedVertices(EarTriangle triangle)
        {
            var result = new HashSet<int>();
            var vertices = triangle.Vertices;
            foreach(var srcVertex in Vertices)
            {
                var currentVertex = srcVertex;
                var sharedVertices = vertices.Where(i => i == currentVertex).ToList();
                if(!sharedVertices.Any())
                {
                    continue;
                }

                foreach(var sharedVertex in sharedVertices)
                {
                    result.Add(sharedVertex);
                }                
            }
            return result.ToList();
        }

        public override string ToString()
        {
            return string.Format("Indexed Triangle [Vertices={0}]", string.Join(",", Vertices));
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods 
    }
}