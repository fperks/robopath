// *******************************************************
// Project: RoboPath.UI
// File Name: IGeometryEditingTool.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;

using GeoAPI.Geometries;

using RoboPath.Core.Space;
using RoboPath.UI.Tools;

namespace RoboPath.PlannerApp.Tools.Editing
{
    public interface IGeometryEditingTool : ITool
    {
        IGeometryFactory GeometryFactory { get; }
        TargetSpaceType TargetSpaceType { get; }
        IList<Coordinate> Vertices { get; }
        bool IsGeometryValid { get; }
        IWorkspace TargetWorkspace { get; }

        void CommitGeometry(bool deactivateOnCommit = true);
        IGeometry CreateGeometry();
    }
}