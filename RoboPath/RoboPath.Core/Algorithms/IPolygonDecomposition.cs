// *******************************************************
// Project: RoboPath.Core
// File Name: IPolygonDecomposition.cs
// By: Frank Perks
// *******************************************************

using System.Collections;
using System.Collections.Generic;

using GeoAPI.Geometries;

namespace RoboPath.Core.Algorithms
{
    public interface IPolygonDecomposition
    {
        IGeometryFactory GeometryFactory { get; }
        IList<IPolygon> DecomposePolygon(IPolygon inputPolygon);
    }
}