// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: LayerTypeId.cs
// By: Frank Perks
// *******************************************************
namespace RoboPath.PlannerApp.Drawing
{
    public enum LayerTypeId : int
    {
        Tool,
        Bounds,
        
        GraphShortestPath,
        GraphVertices,
        GraphEdges,

        StartPosition,
        GoalPosition,

        PathPlanner,
        DecomposedPolygons,

        Workspace,
        CSpace,
    }
}