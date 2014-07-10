// *******************************************************
// Project: RoboPath.Core
// File Name: RoadMapBuilder.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Index.Strtree;

using QuickGraph;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Graph.Builders
{
    public class RoadMapBuilder : IRoadMapBuilder
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; protected set; }
        public Envelope Bounds { get; protected set; }
        public IRoadMap Graph { get; protected set; }
        public IGeometry InvalidRegions { get; set; }
        
        #endregion Properties

        #region Public Methods

        public RoadMapBuilder(IGeometryFactory geometryFactory, Envelope bounds = null, IGeometry invalidRegions = null)
        {
            GeometryFactory = geometryFactory;
            Bounds = bounds;
            InvalidRegions = invalidRegions;
            Graph = new RoadMap(GeometryFactory);
        }

        #region Geometry Methods

        public List<IEdge<Coordinate>> CreateValidEdges(Envelope envelope)
        {
            // add vertices
            var vertices = envelope.GetVertices();
            
            // add edges
            var edges = CreateEdges(vertices.ToVertexEdgePairs());
            return edges;
        }

        public List<IEdge<Coordinate>> CreateValidEdges(IGeometry geometry, bool includeHoles = true)
        {
            Log.Debug("Adding Geometry to UndirectedGraph [ {0} ]", geometry.AsText());
            List<IEdge<Coordinate>> result;
            if(geometry is IPolygon)
            {
                result = CreateValidEdges((IPolygon)geometry, includeHoles);
            }
            else if(geometry is ILinearRing)
            {
                result = CreateValidEdges((ILinearRing)geometry);
            }
            else if(geometry is ILineString)
            {
                result = CreateValidEdges((ILineString)geometry);
            }
            else
            {
                throw new RoadMapOperationException("Cannot Add Geometry [ {0} ], due to unsupported type", geometry.AsText());
            }
            return result;
        }

        public List<IEdge<Coordinate>> CreateValidEdges(IPolygon polygon, bool includeHoles = true)
        {
            var result = new List<IEdge<Coordinate>>();
            result.AddRange(CreateValidEdges(polygon.Shell));
            if(includeHoles)
            {
                foreach(var hole in polygon.Holes)
                {
                    result.AddRange(CreateValidEdges(hole));
                }
            }

            return result;
        }

        public List<IEdge<Coordinate>> CreateValidEdges(ILinearRing ring)
        {
            if(GeometryOperations.IsLinearRingConvex(ring))
            {
                return CreateValidEdges(ring, true);    
            }
            return CreateConcaveRingEdges(ring);
        }

        public List<IEdge<Coordinate>> CreateConcaveRingEdges(ILinearRing source)
        {
            var result = new List<IEdge<Coordinate>>();
            var vertices = source.GetVertices();

            // compute all permuations
            var shellEdges = from sourceVertex in vertices
                             from targetVertex in vertices
                             where !sourceVertex.Equals2D(targetVertex)
                             select new UndirectedEdge<Coordinate>(sourceVertex, targetVertex);

            // Grab only the non intersecting lines
            var validEdges = from edge in shellEdges
                             let line = edge.ToLineString(GeometryFactory)
                             where IsValidEdge(edge) && !Graph.ContainsEdge(edge)
                             select edge;

            foreach(var newEdge in validEdges)
            {
                if(Graph.AddEdge(newEdge))
                {
                    result.Add(newEdge);
                }
            }
            return result;
        }

        public List<IEdge<Coordinate>> CreateValidEdges(ILineString lineString, bool isRing = false)
        {
            // Add Vertices
            var vertices = lineString.GetVertices();
            
            var edges = CreateEdges(vertices.ToVertexEdgePairs(isRing));
            return edges;
        }

        public Coordinate GetNearestNeighbour(Coordinate source)
        {
            var result = from vertex in Graph.Vertices
                         orderby source.Distance(vertex)
                         select vertex;

            if(result.First().Equals2D(source))
            {
                return result.ToList()[1];
            }

            return result.First();

            //var pt = GeometryFactory.CreatePoint(source);
            //var tree = new STRtree();

            //foreach(var vertex in Graph.Vertices)
            //{

            //    tree.Insert(pt.EnvelopeInternal, pt);    
            //}
            //var result = (IPoint)(tree.NearestNeighbour(pt.EnvelopeInternal, pt, new GeometryItemDistance()));
            //return result.Coordinate;
        }

        #endregion Geometry Methods

        #region Validation

        public virtual bool IsValidVertex(Coordinate vertex)
        {
            var isValid = true;
            if(Bounds != null)
            {
                isValid &= Bounds.Contains(vertex);
            }

            // Check to make sure it does not intersect the invalid region outlined
            if(InvalidRegions != null)
            {
                var location = InvalidRegions.LocateVertex(vertex);

                // the vertex is located inside the invalid regions
                isValid &= location != Location.Interior;

            }

            return isValid;
        }

        public virtual bool IsValidEdge(IEdge<Coordinate> edge)
        {
            var isValid = true;
            if(InvalidRegions != null)
            {
                var linestring = edge.ToLineString(GeometryFactory);
                if(linestring.Intersects(InvalidRegions))
                {
                    isValid &= linestring.Touches(InvalidRegions);    
                }
                
                // If the edge is collinear that is okay
                //isValid &= InvalidRegions.LocateVertex(edge.Source) != Location.Interior && InvalidRegions.LocateVertex(edge.Target) != Location.Interior;
            }

            return isValid && IsValidVertex(edge.Source) && IsValidVertex(edge.Target);
        }

        public virtual bool IsNewEdgeValid(IEdge<Coordinate> edge)
        {
            var isValid = true;
            if(InvalidRegions != null)
            {
                var linestring = edge.ToLineString(GeometryFactory);
                if(linestring.Intersects(InvalidRegions))
                {
                    isValid &= linestring.Touches(InvalidRegions);
                }

                // If the edge is collinear that is okay
                isValid &= InvalidRegions.LocateVertex(edge.Source) != Location.Interior && InvalidRegions.LocateVertex(edge.Target) != Location.Interior;
            }

            return isValid;
        }

        #endregion Validation

        public virtual bool AddVertex(Coordinate vertex)
        {
            if(!IsValidVertex(vertex))
            {
                //Log.Warn("Attempting to Add Invalid Vertex [ {0} ] ignoring", vertex);
                return false;
            }

            if(Graph.ContainsVertex(vertex))
            {
                //Log.Warn("Vertex [ {0} ] already exists in UndirectedGraph ignoring", vertex);
                return false;
            }
            Graph.AddVertex(vertex);
            return true;
        }

        public virtual bool AddEdge(IEdge<Coordinate> edge)
        {
            var source = edge.Source;
            var target = edge.Target;
            if(!Graph.ContainsVertex(source))
            {
                if(!IsValidVertex(source))
                {
                    //Log.Warn("Cannot Add Edge [ {0} ] because Vertex is invalid [ {1} ]", edge, source);
                    return false;
                }
                AddVertex(source);
            }

            if(!Graph.ContainsVertex(target))
            {
                if(!IsValidVertex(target))
                {
                    //Log.Warn("Cannot Add Edge [ {0} ] because Vertex is invalid [ {1} ]", edge, target);
                    return false;
                }
                AddVertex(target);
            }

            if(!IsValidEdge(edge))
            {
                //Log.Warn("Cannot Add Edge [ {0} -> {1} ] because it is Invalid", edge.Source, edge.Target);
                return false;
            }

            Graph.AddEdge(edge);
            return true;
        }

        public List<Coordinate> AddVertices(IEnumerable<Coordinate> vertices)
        {
            var invalid = new List<Coordinate>();
            foreach(var vertex in vertices)
            {
                if(!AddVertex(vertex))
                {
                    invalid.Add(vertex);
                }
            }
            return invalid;
        }

        public List<IEdge<Coordinate>> AddEdges(IEnumerable<IEdge<Coordinate>> edges)
        {
            var invalid = new List<IEdge<Coordinate>>();
            foreach(var edge in edges)
            {
                if(!AddEdge(edge))
                {
                    invalid.Add(edge);
                }
            }
            return invalid;
        }

        #endregion Public Methods

        #region Internal Methods

        protected virtual List<IEdge<Coordinate>> CreateEdges(List<Tuple<Coordinate, Coordinate>> source)
        {
            var edges = new List<IEdge<Coordinate>>();
            foreach(var pair in source)
            {
                edges.Add(new UndirectedEdge<Coordinate>(pair.Item1, pair.Item2));
            }
            return edges;
        }
        
        #endregion Internal Methods

    }
}