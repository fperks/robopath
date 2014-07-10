// *******************************************************
// Project: RoboPath.Core
// File Name: ISafetyBuffer.cs
// By: Frank Perks
// *******************************************************

using System.Collections;
using System.Collections.Generic;

using GeoAPI.Geometries;

using RoboPath.Core.Robot;

namespace RoboPath.Core.Algorithms
{
    public interface ISafetyBuffer<out T> 
        where T : IRobot
    {
        IGeometryFactory GeometryFactory { get; }
        IPolygonDecomposition PolygonDecompositionStrategy { get; }
        T Robot { get; }
        IList<IPolygon> InputPolygons { get; }
        IList<IPolygon> Result { get; }
        
        void Solve();
    }
}