// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ToolType.cs
// By: Frank Perks
// *******************************************************
namespace RoboPath.PlannerApp.Tools
{
    public enum ToolType
    {
        Selection,
        EditDefineObstaclePolygon,
        EditDefineFreeSpacePolygon,
        SetStartPosition,
        SetGoalPosition,

        RobotDefineBodyGeometry,

        ZoomToExtent,
        ZoomToRegion,
    }
}