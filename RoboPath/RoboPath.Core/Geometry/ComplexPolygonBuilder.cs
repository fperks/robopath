// *******************************************************
// Project: RoboPath.Core
// File Name: ComplexPolygonBuilder.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Algorithm;
using NetTopologySuite.Noding;
using NetTopologySuite.Operation.Polygonize;

using RoboPath.Core.Utility;

namespace RoboPath.Core.Geometry
{
    public class ComplexPolygonBuilder
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; set; }
        public IList<Coordinate> Vertices { get; private set; }
        public IList<IPolygon> Polygons { get; private set; }
       
        #endregion Properties

        #region Public Methods

        public ComplexPolygonBuilder(IGeometryFactory factory, IList<Coordinate> vertices)
        {
            GeometryFactory = factory;
            Vertices = vertices;
            Validate();
        }

        public void BuildPolygons()
        {
            Polygons = new List<IPolygon>();
            Log.Debug("Creating Polygons from Vertex Collection [ {0} ]", string.Join(",", Vertices));
            var linestring = GeometryFactory.CreateLineString(Vertices.ToArray());
            var nodedLineStrings = new List<ILineString>();
            if(!linestring.IsSimple)
            {
                Log.Debug("Vertices Represent Complex Geometry");
                nodedLineStrings.AddRange(NodeLineStrings());
            }
            else
            {
                nodedLineStrings.Add(linestring);
            }

            var polygons = PolygonizeLineStrings(nodedLineStrings);
            Polygons.AddRange(polygons.GetPolygons());
        }

        #endregion Public Methods

        #region Internal Methods

        private List<ILineString> NodeLineStrings()
        {
            Log.Debug("Noding Complex Line String");
            var nodeString = new NodedSegmentString(Vertices.ToArray(), null);
            var noder = new MCIndexNoder(new IntersectionFinderAdder(new RobustLineIntersector()));
            noder.ComputeNodes(new List<ISegmentString>(new[]
                                                            {
                                                                nodeString
                                                            }));
            var nodedStrings = noder.GetNodedSubstrings();
            var strings = nodedStrings.Select(segment => GeometryFactory.CreateLineString(segment.Coordinates)).ToList();
            return strings;
        }

        private List<IPolygon> PolygonizeLineStrings(List<ILineString> linestrings)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(linestrings.ToArray());
            var polygons = polygonizer.GetPolygons();

            // Try and fix any problem polygons
            return polygons.Select(geometry => (IPolygon) geometry.Buffer(0)).ToList();
        }

        private List<IGeometry> PolygonizeLineString(ILineString linestring)
        {
            var polygonizer = new Polygonizer();
            polygonizer.Add(linestring);
            var polygons = polygonizer.GetPolygons().ToList();
            return polygons;
        }

        private void Validate()
        {
            if(Vertices.Count < 3)
            {
                throw new ArgumentException("3 or More Vertices are Required");
            }

            var isClosedRing = Vertices.IsClosedRing();
            if(!isClosedRing)
            {
                Log.Debug("Vertex Collection is Not Closed Ring, Adding Vertex");
                Vertices = Vertices.ToClosedRing();
            }

            if(Vertices.Count <= 3)
            {
                throw new ArgumentException(string.Format("3 Unique Vertices are Required [ {0} ]", Vertices));
            }
        }

        #endregion Internal Methods 
    }
}