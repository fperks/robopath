// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: MapLayerNames.cs
// By: Frank Perks
// *******************************************************
namespace RoboPath.PlannerApp.Drawing.Map
{
    public static class MapLayers
    {
        public const string ToolLayer = "Tools";
        public const string BoundsLayer = "Bounds";

        public const string GraphShortestPathLayer = "Shortest Path";
        public const string GraphVerticesLayer = "Graph Vertices";
        public const string GraphEdgesLayer = "Graph Edges";

        public const string StartAndGoalLayer = "Start & Goal Positions";

        public const string VertexLayer = "Vertices";

        public const string RobotConfigurationsLayer = "Robot Configurations";
        public const string PathPlannerLayer = "Path Planner";

        public const string DecomposedPolygonsLayer = "Polygon Decomposition";

        public const string WorkspaceLayer = "Workspace";
        public const string CSpaceLayer = "Configuration Space";

        static MapLayers()
        {           
        }
    }
}