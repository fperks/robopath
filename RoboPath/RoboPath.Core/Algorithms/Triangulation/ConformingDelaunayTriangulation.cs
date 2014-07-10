// *******************************************************
// Project: RoboPath.Core
// File Name: ConformingDelaunayTriangulation.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Triangulate.QuadEdge;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Algorithms.Triangulation
{
    public class ConformingDelaunayTriangulation
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IGeometry _siteGeometry;
        private List<Segment> _constraintEdges;

        private Dictionary<Coordinate, Vertex> _constraintVertexMap;
        
        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public Envelope Bounds { get; private set; }
        public List<IPolygon> InputPolygons { get; private set; }
        
        public List<Vertex> Sites { get; private set; }  
        public List<QuadEdgeTriangle> Triangles { get; private set; }
        public QuadEdgeSubdivision QuadEdgeSubdivision { get; private set; }

        #endregion Properties

        #region Public Methods

        public ConformingDelaunayTriangulation(IGeometryFactory geometryFactory, Envelope bounds, List<IPolygon> input)
        {
            GeometryFactory = geometryFactory;
            Bounds = bounds;
            InputPolygons = new List<IPolygon>(from polygon in input
                                               select (IPolygon) GeometryFactory.CreateGeometry(polygon));

            _constraintVertexMap = new Dictionary<Coordinate, Vertex>();
            _siteGeometry = GeometryOperations.Union(InputPolygons);
        }

        public void Triangulate()
        {
            Log.Debug("Triangulating Geometry");

            _constraintVertexMap.Clear();
            Triangles = new List<QuadEdgeTriangle>();

            CreateConstraints();
            CreateSiteVertices();

            var dt = new ConformingDelaunayTriangulator(Sites, 1.0);
            dt.SetConstraints(
                _constraintEdges, 
                _constraintVertexMap.Values.ToList());

            dt.FormInitialDelaunay();
            dt.EnforceConstraints();
            QuadEdgeSubdivision = dt.Subdivision;

            var triangles = QuadEdgeTriangle.CreateOn(QuadEdgeSubdivision).ToList();
            foreach(var item in triangles.Select(triangle => new
                                                                 {
                                                                     Geometry=triangle.GetGeometry((GeometryFactory) GeometryFactory), 
                                                                     Triangle=triangle
                                                                 }))
            {
                var centeroid = item.Geometry.Centroid;
                if(_siteGeometry.LocateVertex(centeroid.Coordinate) != Location.Interior)
                {
                    Triangles.Add(item.Triangle);
                }
            }
        }

        #endregion Public Methods

        #region Internal Methods

        private static List<Segment> GetSegments(List<Coordinate> vertices)
        {
            var result = new List<Segment>();
            foreach(var pair in vertices.ToVertexEdgePairs())
            {
                result.Add(new Segment(pair.Item1, pair.Item2));
            }
            return result;
        }

        private void CreateConstraints()
        {
            _constraintEdges = new List<Segment>();
            _constraintEdges.AddRange(GetSegments(Bounds.GetVertices()));

            foreach(var coord in Bounds.GetVertices())
            {
                _constraintVertexMap[coord] = new ConstraintVertex(coord);
            }
            
            foreach(var polygon in InputPolygons)
            {
                var lines = LinearComponentExtracter.GetLines(polygon).ToList();
                foreach(var line in lines)
                {
                    var lineVertices = line.GetVertices();
                    foreach(var coord in lineVertices)
                    {
                        _constraintVertexMap[coord] = new ConstraintVertex(coord);
                    }
                    _constraintEdges.AddRange(GetSegments(line.GetVertices()));                    
                }
            }
        }

        private void CreateSiteVertices()
        {
            Sites = new List<Vertex>();
            foreach(var polygon in InputPolygons)
            {
                foreach(var fixedCoord in from coord in polygon.GetVertices() 
                                          select new Coordinate(
                                              coord.X - Constants.MachineEpsilon,
                                              coord.Y - Constants.MachineEpsilon))
                {
                    Sites.Add(new ConstraintVertex(fixedCoord));
                }
            }
        }

        //private List<Coordinate> GetSiteCoordinates()
        //{
        //    var result = new List<Coordinate>();
        //    result.AddRange(_siteGeometry.Coordinates);
        //    return result;
        //}

        //private IGeometry CreateConstraints()
        //{
        //    var constraints = new List<ILineString>()
        //                          {
        //                              Bounds.ToLinearRing(GeometryFactory)
        //                          };

        //    foreach(var polygon in InputPolygons)
        //    {
        //        constraints.AddRange(CreateConstraintEdges(polygon));
        //    }
        //    return GeometryFactory.CreateMultiLineString(constraints.ToArray());
        //}

        //private List<ILineString> CreateConstraintEdges(IPolygon polygon)
        //{
        //    // NTS is werid, constraint edges cannot identical to the geometry we are triangulating
        //    var source = (IPolygon)polygon.Buffer(-1.0);
        //    var result = new List<ILineString>()
        //                     {
        //                         CreateConstraintEdge(polygon.Shell)
        //                     };
        //    foreach(var line in polygon.Holes.Select(CreateConstraintEdge))
        //    {
        //        result.Add(line);
        //    }
        //    return result;
        //}

        //private ILineString CreateConstraintEdge(ILinearRing ring)
        //{
        //    var vertices = ring.Coordinates;
        //    vertices[vertices.Count() - 1].X -= Constants.MachineEpsilon;
        //    vertices[vertices.Count() - 1].Y -= Constants.MachineEpsilon;
        //    return GeometryFactory.CreateLineString(vertices.ToArray());
        //}
        
        #endregion Internal Methods
    }
}