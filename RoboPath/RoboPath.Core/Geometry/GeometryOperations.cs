// *******************************************************
// Project: RoboPath.Core
// File Name: GeometryOperations.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Polygonize;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Simplify;

namespace RoboPath.Core.Geometry
{
    public static class GeometryOperations
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        #endregion Fields

        #region Properties

        #endregion Properties

        #region Public Methods

        public static T Clean<T>(T source)
            where T : class, IGeometry
        {
            var result = source.Buffer(0);
            return (T) result;
        }

        public static List<T> Clean<T>(IList<T> sourcePolygons)
            where T : class, IGeometry
        {
            var result = new List<T>();
            foreach(var sourcePolygon in sourcePolygons)
            {
                result.Add((T) sourcePolygon.Buffer(0));
            }
            return result;
        }

        private static IGeometry Polygonize(IGeometry geometry)
        {
            var lines = LineStringExtracter.GetLines(geometry);
            var polygonizer = new Polygonizer();
            polygonizer.Add(lines);
            var polys = polygonizer.GetPolygons();
            var result = GeometryFactory.ToGeometryArray(polys);
            return geometry.Factory.CreateGeometryCollection(result);
        }

        //public static List<IGeometry> SplitPolygon(IGeometry sourcePolygon, IList<ILineString> line)
        //{

        //    //var lines = sourcePolygon.GetLineStrings().Cast<IGeometry>().ToList();
        //    //foreach(var lineString in lineGeometry)
        //    //{
        //    //    lines.AddRange(LineStringExtracter.GetLines(lineString));    
        //    //}
        //    ////lines.AddRange(LineStringExtracter.GetLines(sourcePolygon));
            
        //    //var unionOperator = new UnaryUnionOp(lines);
        //    //var unioned = unionOperator.Union();
        //    //var polygonizer = new Polygonizer();
        //    //var l = LineStringExtracter.GetLines(unioned);
        //    //polygonizer.Add(l);

        //    //var polygons = polygonizer.GetPolygons();
        //    //var result = new List<IGeometry>();
        //    //foreach(var polygon in polygons)
        //    //{
        //    //    if(polygon.Contains(sourcePolygon.InteriorPoint))
        //    //    {
        //    //        result.Add(polygon);
        //    //    }
        //    //}

        //    //return result;

        //    var nodedLine = polygon.Boundary.Union(line);


        //    var output = new List<IGeometry>();
        //    for(var i = 0; i < polygons.NumGeometries; i++)
        //    {
        //        var subPolygon = (Polygon)polygons.GetGeometryN(i);
        //        if(polygon.Contains(subPolygon.InteriorPoint))
        //        {
        //            output.Add(subPolygon);
        //        }
        //    }
        //    return polygon.Factory.BuildGeometry(output);
        //}

        public static IGeometry SplitPolygon(IGeometry sourcePolygon, IGeometry line)
        {
            var polygons = sourcePolygon.GetPolygonsAsGeometry();
            var result = new List<IGeometry>();
            foreach(var polygon in polygons)
            {
                result.Add(SplitPolygonGeometry(polygon, line));
                
            }
            return sourcePolygon.Factory.BuildGeometry(result);
            ////if(!(sourcePolygon is IPolygon))
            ////{
            ////    Debug.Assert(false);
            ////}

            //return sourcePolygon..To
        }
        
        private static IGeometry SplitPolygonGeometry(IGeometry sourcePolygon, IGeometry line)
        {
            var geometryFactory = sourcePolygon.Factory;
            var multiLine = geometryFactory.CreateMultiLineString(sourcePolygon.GetLineStrings().ToArray());
            var nodedLines = multiLine.Union(line);
            var polygons = Polygonize(nodedLines);

            var output = new List<IGeometry>();
            for(var i = 0; i < polygons.NumGeometries; i++)
            {
                var poly = (IPolygon)polygons.GetGeometryN(i);
                output.Add(poly);
                //if(sourcePolygon.Contains(poly.InteriorPoint))
                //{
                    
                //}
            }
            return geometryFactory.BuildGeometry(output);
        }


        #region Merge Geometry

        public static IGeometry Union(IGeometry geometry)
        {
            return Union(geometry.GetPolygons());
        }

        public static IGeometry Union(IGeometry geom1, IGeometry geom2)
        {
            return Union(new List<IGeometry>
                                     {
                                         geom1,
                                         geom2
                                     });
        }

        public static IGeometry Union<T>(IList<T> geometry)
            where T : class, IGeometry
        {
            var validPolygons = new List<IGeometry>();
            if(!geometry.Any())
            {
                return new GeometryFactory().CreateGeometryCollection(null);
            }
            foreach(var geom in geometry)
            {
                if(geom.OgcGeometryType == OgcGeometryType.GeometryCollection)
                {
                    validPolygons.AddRange(geom.GetPolygons());
                }
                else
                {
                    validPolygons.Add(geom);
                }
            }

            if(!validPolygons.Any())
            {
                throw new GeometryOperationException("No valid Polygons for Merge");
            }

            var merged = CascadedPolygonUnion.Union(validPolygons.ToArray());
            return merged;
        }

        #endregion Merge Geometry

        #region Polygon Extraction

        public static List<TResult> ExtractGeometry<TInput, TResult>(TInput geometry)
            where TInput : class, IGeometry
            where TResult : class, IGeometry
        {
            var result = PolygonExtracter.GetPolygons(geometry).Select(g => (TResult)g);
            return result.Where(x => !x.IsEmpty).ToList();
        }

        public static List<TResult> ExtractGeometry<TInput, TResult>(IEnumerable<TInput> geometry)
            where TInput : class, IGeometry
            where TResult : class, IGeometry
        {
            var result = new List<TResult>();
            foreach(var geom in geometry)
            {
                var polygons = PolygonExtracter.GetPolygons(geom).Select(g => (TResult) g);
                result.AddRange(polygons);
            }
            return result.Where(x => !x.IsEmpty).ToList();
        }

        #endregion Polygon Extraction

        public static IGeometry FastPolygonize(IGeometryFactory geometryFactory, IList<Coordinate> vertices, bool isClosed = false)
        {
            var vertexCollection = isClosed ? vertices.ToList() : vertices.ToClosedRing();
            return geometryFactory.CreatePolygon(vertexCollection.ToArray());
        }

        public static IGeometry Simplify(IGeometryFactory geometryFactory, IGeometry geometry, double tolerance=1.0)
        {
            if(geometry.IsEmpty)
            {
                throw new GeometryOperationException("No Valid Polygons to Simplify");
            }
            if(geometry is IGeometryCollection)
            {
                return Simplify(geometryFactory, geometry, tolerance);
            }

            return SimplifyPolygons(geometry, tolerance);
        }

        public static IGeometry Simplify<T>(IGeometryFactory geometryFactory, IList<T> geometry, double tolerance=1.0)
            where T : class, IGeometry
        {
            var valid = geometry.GetPolygons();
            if(!valid.Any())
            {
                throw new GeometryOperationException("No Valid Polygons to Simplify");
            }

            var polygons = geometryFactory.CreateMultiPolygon(valid.ToArray());
            return SimplifyPolygons(polygons, tolerance);
        }

        public static IEnumerable<LineSegment> ExtractLineSegments(IGeometry geometry)
        {
            var segs = new List<LineSegment>();
            var previous = geometry.Coordinates.First();
            foreach(var coordinate in geometry.Coordinates.Skip(1))
            {
                segs.Add(new LineSegment(previous, coordinate));
                previous = coordinate;
            }
            return segs;
        }

        /// <summary>
        /// Determine if the linear ring represents a convex polygon or not
        /// </summary>
        /// <param name="source">Our source Ring</param>
        /// <returns></returns>
        public static bool IsLinearRingConvex(ILinearRing source)
        {
            var sourceVertices = source.Coordinates.ToOpenRing();
            var crossProducts = new List<double>();

            for(var i = 0; i < sourceVertices.Count; i++)
            {
                var a = sourceVertices[i];
                var b = sourceVertices[(i + 1) % sourceVertices.Count];
                var c = sourceVertices[(i + 2) % sourceVertices.Count];

                var x1 = b.X - a.X;
                var x2 = c.X - b.X;
                var y1 = b.Y - a.Y;
                var y2 = c.Y - b.Y;

                crossProducts.Add((x1 * y2) - (y1 * x2));
            }

            // check to see if all the cross products are positive or negative
            return crossProducts.All(x => x > 0.0) || crossProducts.All(x => x < 0.0);
        }

        #endregion Public Methods

        #region Internal Methods



        private static IGeometry SimplifyPolygons(IGeometry geometry, double tolerance)
        {
            var simplified = DouglasPeuckerSimplifier.Simplify(geometry, tolerance);
            return simplified;
        }

        #endregion Internal Methods
    }
}