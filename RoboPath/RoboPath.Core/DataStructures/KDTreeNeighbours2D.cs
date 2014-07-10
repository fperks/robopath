// *******************************************************
// Project: RoboPath.Core
// File Name: KDTreeNeighbours2D.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;
namespace RoboPath.Core.DataStructures
{
    public class KDTreeNeighbours2D
    {
        public Coordinate SearchVertex { get; private set; }
        public int Count { get; private set; }
        public double LargestDistance { get; set; }
        public List<Tuple<Coordinate, double>> Best { get; set; } 

        public KDTreeNeighbours2D(Coordinate searchVertex, int count)
        {
            SearchVertex = searchVertex;
            Count = count;
            LargestDistance = 0;
            Best = new List<Tuple<Coordinate, double>>();
        }

        public void Add(Coordinate vertex)
        {
            var distance = vertex.Distance(SearchVertex);
            for(var i = 0; i < Best.Count; i++)
            {
                var currentBest = Best[i];

                if(i == Count)
                {
                    return;
                }

                if(currentBest.Item2 > distance)
                {
                    Best.Insert(i, Tuple.Create(vertex, distance));
                    LargestDistance = Best.Last().Item2;
                }                   
            }
            Best.Add(Tuple.Create(vertex, distance));
            LargestDistance = Best.Last().Item2;
        }
    }
}