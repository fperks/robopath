// *******************************************************
// Project: RoboPath.Core
// File Name: GraphExtensions.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;
using QuickGraph;

namespace RoboPath.Core.Graph
{
    public static class GraphExtensions
    {
         public static ILineString ToLineString(this IEdge<Coordinate> edge, IGeometryFactory geometryFactory)
         {
             return geometryFactory.CreateLineString(new[] {edge.Source, edge.Target});
         }

        public static List<Coordinate> GetEdgeVertices(this List<IEdge<Coordinate>> edges)
        {
            var result = new HashSet<Coordinate>();
            foreach(var edge in edges)
            {
                result.Add(edge.Source);
                result.Add(edge.Target);
            }
            return result.ToList();
        }

        public static List<Tuple<Coordinate, Coordinate>> GetAllVertexPairs(this List<Coordinate> vertices)
        {
            var result = from item1 in vertices
                         from item2 in vertices
                         where !item1.Equals2D(item2)
                         select Tuple.Create(item1, item2);
            return result.ToList();
        }

    }
}