// *******************************************************
// Project: RoboPath.Core
// File Name: IWorkspace.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

using GeoAPI.Geometries;

using RoboPath.Core.Robot;

namespace RoboPath.Core.Space
{
    public interface IWorkspace : IConfigurationSpace
    {
        void AddOccupiedRegion(IGeometry region);
        void RemoveOccupiedRegion(IGeometry region);
        void Clear();
    }
}