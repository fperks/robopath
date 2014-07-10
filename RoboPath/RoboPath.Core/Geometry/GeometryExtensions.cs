// *******************************************************
// Project: RoboPath.Core
// File Name: GeometryExtensions.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.LinearReferencing;

namespace RoboPath.Core.Geometry
{
    public static class GeometryExtensions
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Public Methods

        #region Geometry Extensions

        public static bool IsPolygon(this IGeometry geometry)
        {
            var polygonGeometries = new[]
                                        {
                                            OgcGeometryType.Polygon,
                                            OgcGeometryType.MultiPolygon,
                                        };
            return polygonGeometries.Any(x => geometry.OgcGeometryType == x) && !geometry.IsEmpty;
        }

        public static bool IsValidPolygon(this IGeometry geometry)
        {
            return IsPolygon(geometry) && !geometry.IsEmpty;
        }

        #region Polygon Extraction

        public static List<IPolygon> GetPolygons(this IGeometry geometry)
        {
            return GeometryOperations.ExtractGeometry<IGeometry, IPolygon>(geometry);
        }

        public static List<IPolygon> GetPolygons(this IEnumerable<IGeometry> geometry)
        {
            return GeometryOperations.ExtractGeometry<IGeometry, IPolygon>(geometry);
        }

        public static List<IGeometry> GetPolygonsAsGeometry(this IGeometry geometry)
        {
            return GeometryOperations.ExtractGeometry<IGeometry, IGeometry>(geometry);
        }

        public static List<IGeometry> GetPolygonsAsGeometry(this IEnumerable<IGeometry> geometry)
        {
            return GeometryOperations.ExtractGeometry<IGeometry, IGeometry>(geometry);
        }

        #endregion Polygon Extraction

        public static List<Coordinate> GetVertices(this Envelope source)
        {
            return new List<Coordinate>
                       {
                           new Coordinate(source.MinX, source.MinY),
                           new Coordinate(source.MaxX, source.MinY),
                           new Coordinate(source.MaxX, source.MaxY),
                           new Coordinate(source.MinX, source.MaxY)
                       };
        }

        public static List<Coordinate> GetVertices(this IGeometry geometry)
        {
            return new HashSet<Coordinate>(geometry.Coordinates).ToList();
        }

        public static List<Coordinate> GetVertices(this ILinearRing ring)
        {
            return new List<Coordinate>(ring.Coordinates).ToOpenRing();
        }

        public static List<Coordinate> GetPolygonVertices(this IPolygon polygon, bool includeHoles=true)
        {
            var result = new HashSet<Coordinate>();
            foreach(var vertex in polygon.ExteriorRing.Coordinates)
            {
                result.Add(vertex);
            }
            
            if(includeHoles || polygon.Holes.Any())
            {
                foreach(var hole in polygon.Holes)
                {
                    foreach(var holeVertex in hole.Coordinates)
                    {
                        result.Add(holeVertex);
                    }                    
                }
            }

            return result.ToList();
        }

        public static IPolygon ToPolygon(this Envelope envelope, IGeometryFactory geometryFactory)
        {
            var polygon = geometryFactory.CreatePolygon(ToLinearRing(envelope, geometryFactory));
            return polygon;
        }

        public static ILinearRing ToLinearRing(this Envelope envelope, IGeometryFactory geometryFactory)
        {
            var vertices = envelope.GetVertices();
            vertices.Add(vertices.First());
            return geometryFactory.CreateLinearRing(vertices.ToArray());
        }
        
        public static ILineString ToLineString(this Envelope envelope, IGeometryFactory geometryFactory)
        {
            var vertices = envelope.GetVertices();
            return geometryFactory.CreateLineString(vertices.ToArray());
        }

        public static List<LineSegment> GetLineSegments(this ILinearRing line)
        {
            var result = new List<LineSegment>();
            foreach(var pair in line.GetVertices().ToVertexEdgePairs(true))
            {
                result.Add(new LineSegment(pair.Item1, pair.Item2));
            }
            return result;
        }

        public static List<LineSegment> GetLineSegments(this ILineString line)
        {
            var result = new List<LineSegment>();
            foreach(var pair in line.GetVertices().ToVertexEdgePairs(false))
            {
                result.Add(new LineSegment(pair.Item1, pair.Item2));
            }
            return result;
        }

        public static List<ILineString> GetLineStrings(this IPolygon polygon)
        {
            var result = new List<ILineString>()
                             {
                                 polygon.Shell
                             };
            result.AddRange(polygon.InteriorRings);
            return result;
        }

        public static List<ILineString> GetLineStrings(this IGeometry geometryCollection)
        {
            var result = new List<ILineString>();
            foreach(var polygon in geometryCollection.GetPolygons())
            {
                result.AddRange(GetLineStrings(polygon));
            }
            return result;
        }

        public static Coordinate GetMidpoint(this ILineString line)
        {
            var indexLine = new LengthIndexedLine(line);
            return indexLine.ExtractPoint(line.Length / 2.0);
        }

        public static IGeometryCollection ToGeometryCollection(this IEnumerable<IGeometry> collection)
        {
            return new GeometryCollection(collection.ToArray());
        }

        public static bool IsConvex(this IPolygon polygon, bool shouldTestHoles = true)
        {
            var rings = new List<ILinearRing>
                            {
                                (ILinearRing) polygon.ExteriorRing
                            };

            if(shouldTestHoles)
            {
                rings.AddRange(polygon.Holes);    
            }
            
            foreach(var ring in rings)
            {
                if(!GeometryOperations.IsLinearRingConvex(ring))
                {
                    return false;
                }
            }
            return true;
        }

        public static Location LocateVertex(this IGeometry geometry, Coordinate vertex)
        {
            var locator = new PointLocator();
            return locator.Locate(vertex, geometry);
        }

        public static IList<Tuple<Coordinate, Coordinate, Coordinate>> GetEdgePairVertices(this ILinearRing source)
        {
            var result = new List<Tuple<Coordinate, Coordinate, Coordinate>>();
            var sourceVertices = source.Coordinates.ToOpenRing();
            for(var i = 0; i < sourceVertices.Count; i++)
            {
                var a = sourceVertices[i];
                var b = sourceVertices[(i + 1) % sourceVertices.Count];
                var c = sourceVertices[(i + 2) % sourceVertices.Count];
                result.Add(new Tuple<Coordinate, Coordinate, Coordinate>(a, b, c));
            }
            return result;
        }

        
        #region Geometry Transformations

        public static IGeometry TransformPosition(this IGeometry geometry, Coordinate newPosition)
        {
            //Log.Debug("Transforming Position From [ {0} ] to [ {1} ]", geometry.Centroid, newPosition);
            var source = geometry.Centroid.Coordinate;
            var dx = newPosition.X - source.X;
            var dy = newPosition.Y - source.Y;
            var transformation = new AffineTransformation();
            transformation.SetToTranslation(dx, dy);
            var result = transformation.Transform(geometry);
            //Log.Debug("Resulting Centroid [ {0} ]", result.Centroid.Coordinate);
            return result;
        }

        public static IGeometry ScaleGeometry(this IGeometry geometry, double scaleX, double scaleY)
        {
            var transformation = new AffineTransformation();
            transformation.SetToScale(scaleX, scaleY);
            var result = transformation.Transform(geometry);
            return result;
        }

        #endregion Geometry Transformations

        #endregion Geometry Extensions

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}