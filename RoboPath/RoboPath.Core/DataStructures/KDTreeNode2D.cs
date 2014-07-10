// *******************************************************
// Project: RoboPath.Core
// File Name: KDTreeNode2D.cs
// By: Frank Perks
// *******************************************************

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.GeometriesGraph;

namespace RoboPath.Core.DataStructures
{
    public class KDTreeNode2D
    {
        #region Properties

        public KDTreeNode2D Left { get; set; }
        public KDTreeNode2D Right { get; set; }

        public Coordinate Vertex { get; private set; }

        public bool IsLeaf
        {
            get { return Left == null && Right == null; }
        }

        #endregion Properties

        #region Public Methods

        public KDTreeNode2D(Coordinate vertex)
        {
            Vertex = vertex;
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}