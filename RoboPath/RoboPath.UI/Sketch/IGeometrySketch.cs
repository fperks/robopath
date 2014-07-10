// *******************************************************
// Project: RoboPath.UI
// File Name: IGeometrySketch.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;

using GeoAPI.Geometries;

namespace RoboPath.UI.Sketch
{
    public interface IGeometrySketch
    {
        IList<Coordinate> Vertices { get; }
        bool IsValid { get; }
        
        void AddVertex(Coordinate vertex);
        void Clear();
        IList<IGeometry> ToGeometry(IGeometryFactory factory);
    }
}