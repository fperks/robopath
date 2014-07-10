// *******************************************************
// Project: RoboPath.Core
// File Name: RasterGrid2D.cs
// By: Frank Perks
// *******************************************************

using System.Diagnostics;

namespace RoboPath.Core.DataStructures
{
    public class RasterGrid2D<T>
        where T : class
    {
        #region Fields

        private readonly T[] _data;

        #endregion Fields

        #region Properties

        public int Width { get; private set; }
        public int Height { get; private set; }

        #endregion Properties

        #region Public Methods

        public RasterGrid2D(int width, int height)
        {
            Width = width;
            Height = height;
            _data = new T[Width * Height];
        }

        public T this[int x, int y]
        {
            get { return _data[y * Width + x]; }
            set { _data[y * Width + x] = value; }
        }
        
        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}