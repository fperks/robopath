// *******************************************************
// Project: RoboPath.Core
// File Name: PathPlannerAlgorithmType.cs
// By: Frank Perks
// *******************************************************
namespace RoboPath.Core.PathPlanning
{
    public enum PathPlannerAlgorithmType
    {
        // Default Value
        None,

        // Geometry Based
        VisibilityGraph,
        Triangulation,
        GeneralizedVoronoiDiagram,
        TrapezoidalDecomposition,
        BoustraphedonDecomposition,
        CannySilhouetteDecomposition,

        // Sampling Based
        GridSampling,
        ProbalisticRoadMap,
        RRT,
        GreedyRRT,
        DualRRT
    }
}