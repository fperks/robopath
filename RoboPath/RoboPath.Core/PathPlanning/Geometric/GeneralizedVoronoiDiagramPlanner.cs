// *******************************************************
// Project: RoboPath.Core
// File Name: GeneralizedVoronoiDiagramPlanner.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;

using QuickGraph;

using RoboPath.Core.Geometry;
using RoboPath.Core.Graph.Builders;
using RoboPath.Core.Robot;
using RoboPath.Core.Space;

namespace RoboPath.Core.PathPlanning.Geometric
{
    public class GeneralizedVoronoiDiagramPlanner : BasePathPlanner 
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IRoadMapBuilder _graphBuilder;

        #endregion Fields

        #region Properties

        public override PathPlannerAlgorithmType AlgorithmType
        {
            get { return PathPlannerAlgorithmType.GeneralizedVoronoiDiagram; }
        }

        public double SamplingDistance { get; private set; }

        public VoronoiDiagramBuilder Voronoi { get; private set; }
        public QuadEdgeSubdivision QuadEdgeSubdivision { get; private set; }
        public List<Coordinate> VoronoiSites { get; private set; }

        public List<IPolygon> ValidVoronoiCells { get; private set; } 

        #endregion Properties

        #region Public Methods

        public GeneralizedVoronoiDiagramPlanner(IGeometryFactory factory, IConfigurationSpace cspace, IRobot robot, double samplingDistance)
            : base(factory, cspace, robot)
        {
            SamplingDistance = samplingDistance;
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void OnInitialization()
        {
            _graphBuilder = new RoadMapBuilder(GeometryFactory, CSpace.Bounds, CSpace.OccupiedSpace);
            ComputeGVD();

            CreateGraph();

            Graph = _graphBuilder.Graph;
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
            return SamplingDistance >= 1.0;
        }

        private void Sample(IPolygon polygon)
        {
            var lines = new List<ILineString>()
                            {
                                polygon.Shell
                            };
            lines.AddRange(polygon.InteriorRings);
            foreach(var line in lines)
            {
                if(!CSpace.Bounds.ToPolygon(GeometryFactory).Contains(line))
                {
                    continue;
                }

                SampleLine(line);
            }            
        }

        private void SampleLine(ILineString line)
        {

            var vertices = new List<Coordinate>(line.Coordinates.ToList());
            var segments = GeometryOperations.ExtractLineSegments(line);
            foreach(var segment in segments)
            {
                if(segment.Length > SamplingDistance)
                {
                    vertices.AddRange(SampleSegment(segment));
                }                
            }

            

            Log.Debug("Added [ {0} ] Sample Points to [ {1} ]", vertices.Count, line.AsText());
            VoronoiSites.AddRange(vertices);
        }

        private List<Coordinate> SampleSegment(LineSegment segment)
        {
            var result = new List<Coordinate>();
            var line = segment.ToGeometry(GeometryFactory);
            var sampledLength = SamplingDistance;
            var indexedLine = new LengthIndexedLine(line);
            while(sampledLength < line.Length)
            {
                result.Add(indexedLine.ExtractPoint(sampledLength));
                sampledLength += SamplingDistance;
            }
            return result;
        }
        

        private void ComputeGVD()
        {
            Log.Debug("Computing Voronoi Sites");
            VoronoiSites = new List<Coordinate>();

            SampleLine(CSpace.Bounds.ToLinearRing(GeometryFactory));
            foreach(var obstacle in CSpace.Obstacles)
            {
                Sample(obstacle);
            }

            Log.Debug("Computing Voronoi with [# of Sites={0}]", VoronoiSites.Count);
            Voronoi = new VoronoiDiagramBuilder()
                                        {
                                            ClipEnvelope = CSpace.Bounds,
                                            Tolerance = 1.0
                                        };
            Voronoi.SetSites(VoronoiSites);
            QuadEdgeSubdivision = Voronoi.GetSubdivision();

            ComputeMedialAxisSegments();

            //ExtractValidVoronoiCells();
        }

        private void CreateGraph()
        {
            var axisSegements = ComputeMedialAxisSegments();

            foreach(var axis in axisSegements)
            {
                _graphBuilder.AddVertex(axis.P0);
                _graphBuilder.AddVertex(axis.P1);
                _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(axis.P0, axis.P1));
            }
            
            var nnstart = _graphBuilder.Graph.GetNearestNeighbours(StartVertex, 1);
            _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(StartVertex, nnstart.First()));

            var nngoal = _graphBuilder.Graph.GetNearestNeighbours(GoalVertex, 1);
            _graphBuilder.AddEdge(new UndirectedEdge<Coordinate>(GoalVertex, nngoal.First()));

        }

        private List<LineSegment> ComputeMedialAxisSegments()
        {
            var axisSegments = new List<LineSegment>();
            var startVertex = GeometryFactory.CreatePoint(StartVertex);
            var goalVertex = GeometryFactory.CreatePoint(GoalVertex);

            foreach(var voronoiCell in from cell in QuadEdgeSubdivision.GetVoronoiCellPolygons(GeometryFactory)
                                       select cell as IPolygon)
            {
                if(voronoiCell == null)
                {
                    continue;
                }

                var segments = voronoiCell.Shell.GetLineSegments();
                foreach(var segment in segments)
                {
                    var geometrySeg = segment.ToGeometry(GeometryFactory);
                    if(!geometrySeg.Intersects(CSpace.OccupiedSpace))
                    {
                        axisSegments.Add(segment);
                    }
                    
                }

                //axisSegments.AddRange(
                //    from segment in segments where segment.ToGeometry(GeometryFactory));
            }
            
                
            //    QuadEdgeSubdivision.GetVoronoiCellPolygons(GeometryFactory). => ))
            //{
            //    var edges = 

            //    //var edgeSegment = voronoiCell.ToLineSegment();
            //    //var edgeGeom = edgeSegment.ToGeometry(GeometryFactory);

            //    //if(!edgeGeom.Intersects(CSpace.OccupiedSpace))
            //    //{
            //    //    axisSegments.Add(edgeSegment);
            //    //}
            //}
            return axisSegments;

        }


        private void ExtractValidVoronoiCells()
        {
            ValidVoronoiCells = new List<IPolygon>();
            foreach(var cell in QuadEdgeSubdivision.GetVoronoiCellPolygons(GeometryFactory))
            {
                if(cell == null || !(cell is IPolygon))
                {
                    continue;
                }

                var cellPolygon = (IPolygon) cell;
                ValidVoronoiCells.Add(cellPolygon);

                if(CSpace.OccupiedSpace.Contains(cellPolygon))
                {
                    continue;
                }                
            }
        }
        

        #endregion Internal Methods
    }
}