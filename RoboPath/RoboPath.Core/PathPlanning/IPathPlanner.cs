// *******************************************************
// Project: RoboPath.Core
// File Name: IPathPlanner.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;

using GeoAPI.Geometries;

using QuickGraph;

using RoboPath.Core.Graph;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning
{
    public interface IPathPlanner
    {
        PathPlannerAlgorithmType AlgorithmType { get; }
        IGeometryFactory GeometryFactory { get; }
        IConfigurationSpace CSpace { get; }
        IGeometry OccupiedRegions { get; }

        IRobot Robot { get; set; }
        Coordinate StartVertex { get; set; }
        Coordinate GoalVertex { get; set; }
        IRobotConfiguration StartConfiguration { get; }
        IRobotConfiguration GoalConfiguration { get; }

        IRoadMap Graph { get; }
        List<IEdge<Coordinate>> ShortestPath { get; }

        PathPlannerAlgorithmState State { get; }
        ShortestPathAlgorithm ShortestPathAlgorithm { get; set; }
        
        void Initialize();
        bool Validate();
        PathPlannerAlgorithmState Solve();
    }
}