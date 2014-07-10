//// *******************************************************
//// Project: RoboPath.Core
//// File Name: KDTree2D.cs
//// By: Frank Perks
//// *******************************************************

//using System;
//using System.Collections.Generic;

//using GeoAPI.Geometries;

//using NLog;

//namespace RoboPath.Core.DataStructures
//{
//    public class KDTree2D
//    {
//        #region Fields

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        #endregion Fields

//        #region Properties

//        public KDTreeNode2D Root { get; set; }
//        public KDTreeNeighbours2D Neighbours { get; set; }

//        #endregion Properties

//        #region Public Methods

//        public KDTree2D(Coordinate rootVertex)
//        {
//            Root = new KDTreeNode2D(rootVertex);
//        }

//        public KDTreeNode2D Add(Coordinate vertex)
//        {
//            return Add(vertex, Root, 0);
//        }

//        public List<Coordinate> GetNearestNeighbours(Coordinate vertex, int count)
//        {
//            return null;
//        }

//        #endregion Public Methods

//        #region Internal Methods

//        private List<Coordinate> NearestNeighbourSearch(Coordinate vertex, KDTreeNode2D node, int neighbourCount)
//        {
//            if(Root != null)
//            {
//                Neighbours = new KDTreeNeighbours2D(vertex, neighbourCount);
                
//            }
//        }

//        private List<Coordinate> NearestNeighbourSearch(Coordinate vertex, KDTreeNode2D tree, int count, int depth, List<Coordinate> optimal)
//        {
            
//        }

//        private KDTreeNode2D Add(Coordinate vertex, KDTreeNode2D node, int depth)
//        {
//            if(node == null)
//            {
//                return new KDTreeNode2D(vertex);
//            }

//            if(vertex.CompareTo(node.Vertex) < 0)
//            {
//                node.Left = Add(vertex, node.Left, depth + 1);
//            }
//            else if(vertex.CompareTo(node.Vertex) > 0)
//            {
//                node.Right = Add(vertex, node.Right, depth + 1);
//            }
//            else
//            {
//                Log.Error("Duplicate Vertices Added :( for [Vertex={0},Depth={1}]", vertex, depth);
//            }

//            return node;
//        }

//        #endregion Internal Methods
//    }
//}