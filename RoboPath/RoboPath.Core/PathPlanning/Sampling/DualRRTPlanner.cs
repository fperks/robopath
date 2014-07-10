// *******************************************************
// Project: RoboPath.Core
// File Name: DualRRTPlanner.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;

using QuickGraph;

using RoboPath.Core.Geometry;
using RoboPath.Core.Graph;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Sampling
{
    public class DualRRTPlanner : BasePathPlanner
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get
            {
                return PathPlannerAlgorithmType.DualRRT;
            }
        }

        public TreeGraphBuilder TreeGoal { get; private set; }
        public TreeGraphBuilder TreeStart { get; private set; }
        
        public int N { get; private set; }
        public double StepSize { get; private set; }
        public Random Generator { get; private set; }
        public double GoalBias { get; private set; }

        #endregion Properties

        #region Public Methods

        public DualRRTPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot, int count, double stepSize, double goalBias)
            : base(factory, cspace, robot)
        {
            N = count;
            StepSize = stepSize;

            GoalBias = goalBias;
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            Graph = new RoadMap(GeometryFactory);
            Generator = new Random();

            TreeGoal = new TreeGraphBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace, StartVertex);
            TreeGoal.AddVertex(GoalVertex);

            TreeStart = new TreeGraphBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace, GoalVertex);
            TreeStart.AddVertex(StartVertex);
            
            CreateTree();            

            
        }

        protected override PathPlannerAlgorithmState OnSolve()
        {
            Graph.ComputeEdgeCosts(edge => edge.Source.Distance(edge.Target));
            ShortestPath = Graph.ComputeShortestPath(StartVertex, GoalVertex, ShortestPathAlgorithm);
            if(!ShortestPath.Any())
            {
                return PathPlannerAlgorithmState.ShortestPathNotFound;
            }
            return PathPlannerAlgorithmState.ShortestPathFound;
        }

        protected override bool OnValidate()
        {
            return true;
        }

        private void CreateTree()
        {
            int count = 0;
            bool merged = false;
            while(count < N && !merged)
            {
                var v1 = GrowTree(TreeStart);
                var n1 = TreeGoal.GetNearestNeighbour(v1);

                var v2 = GrowTree(TreeGoal);
                var n2 = TreeStart.GetNearestNeighbour(v2);

                // check to see if they touch
                if(n1.Distance(n2) <= StepSize)
                {
                    // Merge the Trees
                    var edge = new UndirectedEdge<Coordinate>(n1, n2);
                    if(TreeStart.IsNewEdgeValid(edge))
                    {
                        MergeTrees(TreeStart, TreeGoal, edge);
                        merged = true;
                    }
                }
                count++;
            }
            
            if(!merged)
            {
                Graph.AddVerticesAndEdgeRange(TreeStart.Graph.Edges);
                Graph.AddVerticesAndEdgeRange(TreeGoal.Graph.Edges);
            }
        }

        private void MergeTrees(TreeGraphBuilder left, TreeGraphBuilder right, IEdge<Coordinate> mergeEdge)
        {
            var vertices = left.Graph.Vertices.ToList();
            vertices.AddRange(right.Graph.Vertices);

            var edges = left.Graph.Edges.ToList();
            edges.AddRange(right.Graph.Edges);

            foreach(var vertex in vertices)
            {
                Graph.AddVertex(vertex);
            }

            foreach(var edge in edges)
            {
                Graph.AddEdge(edge);   
            }

            Graph.AddVerticesAndEdge(mergeEdge);

            //Graph.AddVerticesAndEdgeRange(left.Graph.Edges);
            //Graph.AddVerticesAndEdgeRange(right.Graph.Edges);
            //Graph.AddEdge(mergeEdge);
        }

        private Coordinate GrowTree(TreeGraphBuilder tree)
        {
            while(true)
            {
                Coordinate q;
                if(GoalBias < Generator.NextDouble())
                {
                    var x = Generator.Next((int)CSpace.Bounds.Width - 1);
                    var y = Generator.Next((int)CSpace.Bounds.Height - 1);
                    q = new Coordinate(x, y);
                }
                else
                {
                    q = tree.BiasVertex;
                }

                var v = tree.GetNearestNeighbour(q);
                var p = GetPointAlongRay(v, q);
                var edge = new UndirectedEdge<Coordinate>(v, p);


                if(tree.IsNewEdgeValid(edge))
                {
                    if(tree.AddVertex(p))
                    {
                        tree.AddEdge(edge);
                        return p;
                    }
                    
                }
            }
        }

        private Coordinate GetPointAlongRay(Coordinate source, Coordinate target)
        {
            var segment = new LineSegment(source, target);

            if(source.Equals2D(target))
            {
                Log.Debug("Wat");
            }

            var result = segment.PointAlong(StepSize / segment.Length);

            var segment2 = new LineSegment(source, result);


            return result;
        }

        private Coordinate GetRandomVertex()
        {
            var generator = new Random();
            
            while(true)
            {
                var vertex = new Coordinate(generator.NextDouble() * CSpace.Bounds.Width, generator.NextDouble() * CSpace.Bounds.Height);
                var location = OccupiedRegions.LocateVertex(vertex);

                //foreach(var obstacle in CSpace.Obstacles)
                //{
                //    var pt = GeometryFactory.CreatePoint(vertex);
                //    var value = obstacle.Intersects(pt);
                //    Log.Warn(value);

                //}

                if(OccupiedRegions.LocateVertex(vertex) != Location.Interior)
                {
                    return vertex;
                }
            }
        }

        #endregion Internal Methods
    }
}