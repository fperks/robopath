// *******************************************************
// Project: RoboPath.Core
// File Name: TrapezoidalDecompositionPlanner.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snapround;
using NetTopologySuite.Operation.Linemerge;
using NetTopologySuite.Operation.Polygonize;

using QuickGraph;

using RoboPath.Core.Geometry;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Geometric.Decomposition
{
    public class TrapezoidalDecompositionPlanner : BasePathPlanner
    {

        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IRoadMapBuilder _graphBuilder;

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.TrapezoidalDecomposition; }
        }

        public IGeometry FreeSpace { get; private set; }
        public IEnumerable<Coordinate> CellOriginVertices { get; private set; }

        public List<IPolygon> Obstacles { get; private set; }

        public Dictionary<Coordinate, List<Coordinate>> CellVertices { get; private set; }
        public Dictionary<Coordinate, IGeometry> CellEdges { get; private set; } 
        
        //public Dictionary<Coordinate, int> CellEdgeIndexMap { get; private set; } 

        //public Dictionary<int, List<LineSegment>> ObstacleSegments { get; private set; }
        //public Dictionary<Coordinate, List<ILineString>> EdgeLines { get; private set; }
        
        //public List<IGeometry> Regions { get; private set; }
        //public List<ILineString> CellEdges { get; private set; }
        //public List<Tuple<Coordinate, ILineString>> CellVertices { get; private set; }
        //public List<IPolygon> Cells { get; private set; } 

        #endregion Properties

        #region Public Methods

        public TrapezoidalDecompositionPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot)
            : base(factory, cspace, robot)
        {
            _graphBuilder = new RoadMapBuilder(factory, cspace.Bounds, cspace.OccupiedSpace);
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            Obstacles = new List<IPolygon>();
            //ObstacleSegments = new Dictionary<int, List<LineSegment>>();
            FreeSpace = CSpace.Bounds.ToPolygon(GeometryFactory).SymmetricDifference(CSpace.OccupiedSpace);
            CellOriginVertices = from vertex in FreeSpace.GetVertices()
                                 orderby vertex.X, vertex.Y
                                 where CSpace.Bounds.Contains(vertex)
                                 select vertex;

            CellVertices = new Dictionary<Coordinate, List<Coordinate>>();
            CellEdges = new Dictionary<Coordinate, IGeometry>();
            //CellEdgeIndexMap = new Dictionary<Coordinate, int>();
            //IndexObstacles();
            CreateGraph();

            Graph = _graphBuilder.Graph;
        }

        protected override PathPlannerAlgorithmState OnSolve()
        {
            Log.Debug("Computing Shortest Path");
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

        private void CreateGraph()
        {

            foreach(var origin in CellOriginVertices)
            {
                ComputeCellEdges(origin);    
            }

            foreach(var kvp in CellVertices)
            {
                var vertices = kvp.Value;

                foreach(var sourceVertex in vertices)
                {
                    AddEdge(sourceVertex);
                }
            }

            _graphBuilder.AddVertex(StartVertex);
            _graphBuilder.AddVertex(GoalVertex);
            
            var startEdge = new UndirectedEdge<Coordinate>(StartVertex, _graphBuilder.GetNearestNeighbour(StartVertex));
            var goalEdge = new UndirectedEdge<Coordinate>(GoalVertex, _graphBuilder.GetNearestNeighbour(GoalVertex));
            AddEdge(StartVertex);
            AddEdge(GoalVertex);
        }

        private void AddEdge(Coordinate sourceVertex)
        {
            var cellEdges = from edge in CellEdges
                                
                                select edge;

            //where originVertex == null || !edge.Key.Equals2D(originVertex)

            foreach(var cellEdge in cellEdges)
            {
                var cellEdgeTemp = cellEdge;
                var targetVertices = CellVertices[cellEdge.Key];
                foreach(var targetVertex in targetVertices)
                {
                    var targetVertexTemp = targetVertex; 
                    var edgeGeometry = cellEdge.Value;
                    var ray = GeometryFactory.CreateLineString(new[]
                                                               {
                                                                   sourceVertex,
                                                                   targetVertex
                                                               });

                    Log.Debug("Crosses [{0}]", ray.Crosses(edgeGeometry));

                    if(!ray.Crosses(edgeGeometry))
                    {
                        var isValid = true;
                        foreach(var edge in from edge in CellEdges
                                                where !cellEdgeTemp.Key.Equals2D(targetVertexTemp)
                                                select edge)
                        {
                            if(ray.Crosses(edge.Value))
                            {
                                isValid = false;
                                break;
                            }                            
                        }
          
                        if(isValid)
                        {
                            var graphEdge = new UndirectedEdge<Coordinate>(sourceVertex, targetVertex);
                            if(_graphBuilder.IsValidEdge(graphEdge))
                            {
                                _graphBuilder.AddEdge(graphEdge);
                            }                            
                        }

                    }
                }
            }
        }


        private IPolygon GetFreeSpaceRegion(Coordinate source)
        {
            var sourcePt = GeometryFactory.CreatePoint(source);
            var freeSpacePolygons = FreeSpace.GetPolygons();
            foreach(var polygon in freeSpacePolygons)
            {
                if(polygon.Contains(sourcePt) || polygon.Boundary.Contains(sourcePt))
                {
                    return polygon;
                }
            }
            throw new PathPlannerException("Point [ {0} ] does not exist in any freespace region", source);
        }

        private void ComputeCellEdges(Coordinate source)
        {
            var region = GetFreeSpaceRegion(source);
            var ray = GeometryFactory.CreateLineString(new[]
                                                            {
                                                                new Coordinate(source.X, 0.0),
                                                                new Coordinate(source.X, CSpace.Bounds.Height),
                                                            });
            var intersection = region.Intersection(ray);
            Log.Debug("Intersection Type [ {0} ]", intersection.OgcGeometryType);
            if(intersection is IPoint)
            {
                return;
            }

            if(intersection is IMultiLineString)
            {
                var edgeVertices = new List<Coordinate>();
                CellVertices[source] = new List<Coordinate>();
                foreach(var line in (GeometryCollection)intersection)
                {
                    if(line.Coordinates.Contains(source) && line is ILineString)
                    {
                        var lineString = (ILineString)line;
                        Log.Debug("Adding Line: {0}", lineString);
                        CellVertices[source].Add(lineString.GetMidpoint());
                        if(!edgeVertices.Any())
                        {
                            edgeVertices.Add(lineString.Coordinates[0]);
                            edgeVertices.Add(lineString.Coordinates[1]);
                        }
                        else
                        {
                            edgeVertices.Add(lineString.Coordinates[1]);
                        }                        
                    }
                }
                CellEdges[source] = GeometryFactory.CreateLineString(edgeVertices.ToArray());
            }
        }


        //private void IndexObstacles()
        //{
        //    var i = 0;
        //    foreach(var obstacle in Obstacles)
        //    {
        //        var copy = (IPolygon) GeometryFactory.CreateGeometry(obstacle);
        //        copy.UserData = i++;
        //        Obstacles.Add(copy);
        //        //ObstacleSegments[i] = new List<LineSegment>(GeometryOperations.ExtractLineSegments(copy));
        //    }
        //}

        //private void ComputeCells()
        //{
        //    //foreach(var origin in CellOriginVertices)
        //    //{
        //    //    ComputeCellEdges(origin);
        //    //}

        //    // Add Edges
            

        //    //// Breakup Free Space
        //    ////var decomposed = GeometryFactory.CreateGeometry(FreeSpace);
            
        //    //var decomposed = GeometryFactory.CreateGeometry(FreeSpace);

        //    //var lines = new List<ILineString>();
        //    //var i = 0;
        //    //foreach(var kvp in EdgeLines)
        //    //{
                
        //    //    var linesStrings = kvp.Value;
        //    //    var source = kvp.Key;

        //    //    Log.Debug("Adding Cell [#{0} => {1}, Value={2}]", i++, source, string.Join("\n", linesStrings));
        //    //    if(!linesStrings.Any())
        //    //    {
        //    //        continue;
        //    //    }

                

        //    //    lines.AddRange(linesStrings);

        //    //    //var line = linesStrings.Count == 2 ? GeometryFactory.CreateLineString(new[]
        //    //    //                                                                          {
        //    //    //                                                                              linesStrings[0].Coordinates[0],
        //    //    //                                                                              linesStrings[1].Coordinates[1],
        //    //    //                                                                          }) : linesStrings[0];
                
        //    //    //GeometryFactory.CreateMultiLineString(linesStrings.ToArray())
        //    //    //decomposed = GeometryOperations.SplitPolygon(decomposed, line);   
        //    //}

        //    //var merger = new LineMerger();
        //    //merger.Add(lines);
        //    //var lines2 = merger.GetMergedLineStrings();
        //    //var multiLine = GeometryFactory.CreateMultiLineString(lines2.Cast<ILineString>().ToArray());
        //    //var noder = new MCIndexNoder(new IntersectionFinderAdder(new RobustLineIntersector()));
        //    //var segments = SegmentStringUtil.ExtractSegmentStrings(multiLine).ToList();
        //    //segments.AddRange(SegmentStringUtil.ExtractSegmentStrings(FreeSpace));
        //    //noder.ComputeNodes(segments);
        //    //var noded = noder.GetNodedSubstrings();
        //    //var resultingLines = GeometryFactory.CreateMultiLineString((from segment in noded
        //    //                     select GeometryFactory.CreateLineString(segment.Coordinates)).ToArray());
        //    //var result = GeometryOperations.SplitPolygon(decomposed, resultingLines).GetPolygons();
            
        //    //Cells.AddRange(result);
            
        //}


        private bool CanExtendRayUp(Coordinate source)
        {
            var upPoint = GeometryFactory.CreatePoint(new Coordinate(source.X, source.Y - 0.1));
            foreach(var obstacle in Obstacles)
            {
                if(obstacle.Contains(upPoint))
                {
                    return false;
                }
            }
            return true;
        }

        private bool CanExtendRayDown(Coordinate source)
        {
            var upPoint = GeometryFactory.CreatePoint(new Coordinate(source.X, source.Y + 0.1));
            foreach(var obstacle in Obstacles)
            {
                if(obstacle.Contains(upPoint))
                {
                    return false;
                }
            }
            return true;
        }


        
        ////private void CreateCells()
        ////{
        ////    CellEdges = new List<LineSegment>();
        ////    var segmentStrings = new List<ISegmentString>(from vertex in CSpace.OccupiedSpace.GetVertices()
        ////                                                  select new NodedSegmentString(new[]
        ////                                                                                    {
        ////                                                                                        new Coordinate(vertex.X, 0.0),
        ////                                                                                        vertex,
        ////                                                                                        new Coordinate(vertex.X, CSpace.Bounds.Height),
        ////                                                                                    }, vertex));
        ////    foreach(var polygon in CSpace.Obstacles)
        ////    {
        ////        segmentStrings.Add(new NodedSegmentString(polygon.Coordinates, null));
        ////    }

        ////    var noder = new MCIndexNoder(new IntersectionFinderAdder(new RobustLineIntersector()));
        ////    noder.ComputeNodes(segmentStrings);
        ////    var lines = new List<ILineString>();
        ////    foreach(var nodedString in noder.GetNodedSubstrings())
        ////    {
        ////        var line = GeometryFactory.CreateLineString(nodedString.Coordinates);
        ////        line.UserData = nodedString.Context;
        ////    }

        ////    foreach(var line in lines)
        ////    {
        ////        if(line.UserData != null)
        ////        {
        ////            continue;
        ////        }

        ////        var sourceVertex = (Coordinate)line.UserData;
        ////        foreach(var segment in line.GetLineSegments())
        ////        {
        ////            if(segment.P0.Equals2D(sourceVertex) || segment.P1.Equals2D(sourceVertex))
        ////            {
                        
        ////            }
        ////        }
        ////    }


        ////    foreach(var line in lines)
        ////    {
        ////        var segments = line.GetLineSegments();
        ////        foreach(var segment in segments)
        ////        {
        ////            //if(segment.P0.Equals2D())
        ////            CellEdges.Add(segment);    
        ////        }

                
        ////    }
        ////}

        //private void CreateGraph()
        //{
        //    CellEdges = new List<LineSegment>();
        //    Coordinate previous = null;
        //    var boundsVertices = CSpace.Bounds.GetVertices();
        //    foreach(var vertex in CSpace.OccupiedSpace.GetVertices())
        //    {
        //        if(boundsVertices.Contains(vertex))
        //        {
        //            continue;
        //        }

        //        var segments = ComputeSweepEdges(vertex);
        //        CellEdges.AddRange(segments);

        //        var cellEdges = (from segment in segments
        //                        select segment.ToGeometry(GeometryFactory)).ToList();

        //        foreach(var cellEdge in cellEdges)
        //        {
        //            var midpoint = cellEdge.GetMidpoint();
        //            _graphBuilder.AddVertex(midpoint);

        //            if(previous == null)
        //            {
        //                previous = midpoint;
        //                continue;
        //            }

        //            var edge = new UndirectedEdge<Coordinate>(previous, midpoint);
        //            if(_graphBuilder.IsValidEdge(edge))
        //            {
        //                _graphBuilder.AddEdge(edge);
        //            }
        //            previous = midpoint;
        //        }
        //    }

        //    CellEdges = (from edge in CellEdges
        //                 orderby edge.MidPoint.X, edge.MidPoint.Y 
        //                 select edge).ToList();

        //    ComputeCells();

        //    Graph = _graphBuilder.Graph;
        //}

        //private void AddGraphEdges()
        //{
        //    var vertices = from vertex in _graphBuilder.Graph.Vertices
        //                   orderby vertex.X
        //                   select vertex;

        //    Coordinate sourceVertex = vertices.First();
        //    foreach(var targetVertex in vertices.Skip(1))
        //    {
        //        var edge = new UndirectedEdge<Coordinate>(sourceVertex, targetVertex);
        //        if(_graphBuilder.IsValidEdge(new UndirectedEdge<Coordinate>(sourceVertex, targetVertex)))
        //        {
        //            _graphBuilder.AddEdge(edge);
        //        }
        //    }
        //}

        //private List<LineSegment> ComputeSweepEdges(Coordinate source)
        //{
        //    var ray = GeometryFactory.CreateLineString(new[]
        //                                                   {
        //                                                       new Coordinate(source.X, 0.0), 
        //                                                       new Coordinate(source.X, CSpace.Bounds.Height), 
        //                                                   });
        //    var result = new List<LineSegment>();
        //    var intersection = CSpace.OccupiedSpace.Intersection(ray);
        //    if(intersection is IPoint)
        //    {
        //        // extreme point where the ray goes both directions and touches the boundary
        //        result.Add(new LineSegment(source.X, 0.0, source.X, source.Y));
        //        result.Add(new LineSegment(source.X, source.Y, source.X, CSpace.Bounds.Height));
        //    }
        //    else if(intersection is ILineString)
        //    {
        //        result.Add(new LineSegment(source, GetClosestVertex(source, (ILineString)intersection)));
        //    }
        //    else
        //    {
        //        // If its a collection then ray extends both ways, and is an extreme point
        //        Log.Debug("{0}", intersection.OgcGeometryType);
                
        //        // Get Above
        //        var above = from target in intersection.GetVertices()
        //                       orderby target.Y 
        //                       where target.Y < source.Y
        //                       select target;

        //        var below = from target in intersection.GetVertices()
        //                    orderby target.Y
        //                    where target.Y > source.Y
        //                    select target;
                
        //        above = above.ToList();
        //        below = below.ToList();

        //        if(above.Any())
        //        {
        //            var target = above.Last();
        //            result.Add(new LineSegment(source, target));
        //        }

        //        if(below.Any())
        //        {
        //            var target = below.First();
        //            result.Add(new LineSegment(source, target));
        //        }
        //    }

        //    var filtered = new List<LineSegment>();
        //    foreach(var segment in result)
        //    {
        //        var line = GeometryFactory.CreateLineString(new[]
        //                                                        {
        //                                                            segment.P0,
        //                                                            segment.P1
        //                                                        });
        //        if(!CSpace.OccupiedSpace.Contains(line) && 
        //            CSpace.OccupiedSpace.LocateVertex(line.GetMidpoint()) != Location.Interior)
        //        {
        //            filtered.Add(segment);
        //        }
        //    }

        //    return filtered;
        //}

        //private void ComputeCells()
        //{
        //    Cells = new List<IGeometry>();
        
            
        //    //var lines = GeometryFactory.CreateMultiLineString((from cellEdge in cellEdges
        //     //           select cellEdge.ToGeometry(GeometryFactory)).ToArray());
        //    //var merged = GeometryFactory.CreateGeometry(freespace);
        //    var cells = GeometryFactory.CreateGeometry(freespace);

        //    Log.Debug("Cell Edges Count = {0}", CellEdges.Count);

        //    var cellEdgeGeom = CellEdges[0].ToGeometry(GeometryFactory);

        //    var splitGeometry = GeometryOperations.SplitPolygon(freespace, cellEdgeGeom);
        //    Cells.Add(splitGeometry);
        //    foreach(var cellEdge in from edge in CellEdges
        //                            select edge.ToGeometry(GeometryFactory))
        //    {
        //        var split = GeometryOperations.SplitPolygon(cells, cellEdge);
        //        cells = split;

        //    }
        //    Cells.Add(cells);
            

        //    //var segmentStrings = new List<ISegmentString>(from cellEdge in CellEdges
        //    //                                              select new NodedSegmentString(new[]
        //    //                                                                                {
        //    //                                                                                    cellEdge.P0,
        //    //                                                                                    cellEdge.P1
        //    //                                                                                }, null));

        //    //////                                                select new NodedSegmentString(new[]
        //    //////                                                                                {
        //    //////                                                                                    new Coordinate(vertex.X, 0.0),
        //    //////                                                                                    vertex,
        //    //////                                                                                    new Coordinate(vertex.X, CSpace.Bounds.Height),
        //    //////                                                                                }, vertex));
        //    //////segmentStrings.AddRange(SegmentStringUtil.ExtractSegmentStrings(CSpace.Bounds.ToLineString(GeometryFactory)));
        //    ////foreach(var polygon in CSpace.Obstacles)
        //    ////{
        //    //segmentStrings.AddRange(SegmentStringUtil.ExtractSegmentStrings(freespace));
        //    ////}

        //    //var noder = new MCIndexNoder(new IntersectionFinderAdder(new RobustLineIntersector()));
        //    //noder.ComputeNodes(segmentStrings);
        //    //var nodedStrings = noder.GetNodedSubstrings();
        //    //////noder.ComputeNodes(segmentStrings);

        //    //var lines = from nodedString in nodedStrings 
        //    //            select GeometryFactory.CreateLineString(nodedString.Coordinates);

        //    //var merged = GeometryOperations.Union(lines.ToList());


        //    //var polygonizer = new Polygonizer();
        //    //polygonizer.Add(lines.ToArray());

        //    //Cells = polygonizer.GetPolygons().ToList();

        //    //foreach(var nodeString in noder.GetNodedSubstrings())
        //    //{
        //    //    if(nodeString.Coordinates.Count() < 2)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    lines.Add(GeometryFactory.CreateLineString(nodeString.Coordinates));
        //    //}

        //    ////var lines = from nodeString in noder.GetNodedSubstrings()
        //    ////            select GeometryFactory.CreateLineString(nodeString.Coordinates);

        //    //var geometry = GeometryFactory.CreateMultiLineString((from cellEdge in CellEdges
        //    //                                              select GeometryFactory.CreateLineString(new[]
        //    //                                                                                {
        //    //                                                                                    cellEdge.P0,
        //    //                                                                                    cellEdge.P1
        //    //                                                                                })).ToArray());

        //    ////geometry.AddRange(CSpace.Obstacles);
        //    //var geometryCollection = GeometryFactory.CreateGeometryCollection(geometry.ToArray());
        //    //var merged = GeometryOperations.Union(geometry, freespace);

        //    //Cells = new List<IGeometry>(polygonizer.GetPolygons());

        //}

        //private Coordinate GetClosestVertex(Coordinate source, ILineString line)
        //{
        //    var vertices = from vertex in line.GetVertices()
        //                   where !vertex.Equals2D(source)
        //                   select vertex;
        //    var target = vertices.First();
        //    foreach(var vertex in line.GetVertices())
        //    {
        //        if(target.Distance(source) > vertex.Distance(source))
        //        {
        //            target = vertex;
        //        }               
        //    }
        //    return target;
        //}

        //private List<Coordinate> GetAdjacentGraphVertices(Coordinate vertex)
        //{
        //    var result = new List<Coordinate>();



        //    return result;
        //}

        #endregion Internal Methods
    }
}