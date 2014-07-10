// *******************************************************
// Project: RoboPath.Core
// File Name: PolygonalSafetyBuffer.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;
using RoboPath.Core.Robot.Geometric;

namespace RoboPath.Core.Algorithms.SafetyBuffer
{
    public class PolygonalSafetyBuffer : ISafetyBuffer<PolygonalRobot>
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<IPolygon> _result;
        private List<List<IPolygon>> _sourcePolygons;

        #endregion Fields

        #region Properties
        
        public IGeometryFactory GeometryFactory { get; private set; }
        public IPolygonDecomposition PolygonDecompositionStrategy { get; private set; }
        public PolygonalRobot Robot { get; private set; }
        public IList<IPolygon> InputPolygons { get; private set; }
        public IList<IPolygon> Result
        {
            get { return _result; }
        }

        #endregion Properties

        #region Public Methods

        public PolygonalSafetyBuffer(IGeometryFactory geometryFactory, PolygonalRobot robot, IList<IPolygon> inputPolygons, IPolygonDecomposition strategy)
        {
            GeometryFactory = geometryFactory;
            Robot = robot;
            InputPolygons = inputPolygons;
            PolygonDecompositionStrategy = strategy;
        }

        public void Solve()
        {
            var results = new List<IPolygon>();
            _result = new List<IPolygon>();

            // Compute our Source Polygons Decomposing if Nesscary
            ComputeSourcePolygons();
            
            foreach(var sourcePolygon in _sourcePolygons)
            {
                var sum = (IPolygon) ComputeMinowskiSums(sourcePolygon);
                results.Add(sum);
            }
            _result.AddRange(GeometryOperations.Union(results).GetPolygons());
        }

        #endregion Public Methods

        #region Internal Methods

        private void ComputeSourcePolygons()
        {
            _sourcePolygons = new List<List<IPolygon>>();
            foreach(var polygon in InputPolygons)
            {
                if(polygon.IsConvex())
                {
                    // We don't need to decompose, so just add it
                    //Log.Debug("Polygon [ {0} ] is Convex", polygon.AsText());
                    _sourcePolygons.Add(new List<IPolygon>
                                                    {
                                                        polygon
                                                    });
                }
                else
                {
                    // It is a concave or complex polygon so we need to decompose it
                    _sourcePolygons.Add(DecomposePolygon(polygon));
                }
            }
        }

        private List<IPolygon> DecomposePolygon(IPolygon polygon)
        {
            Log.Debug("Decomposing non Convex Polygon [ {0} ]", polygon.AsText());
            var result = PolygonDecompositionStrategy.DecomposePolygon(polygon);
            if(!result.Any())
            {
                throw new GeometryOperationException(polygon, "Decomposition of Polygon [ {0} ] failed", polygon.AsText());
            }

            return result.ToList();
        }

        private IGeometry ComputeMinowskiSums(List<IPolygon> decomposed)
        {
            IGeometry result = null;
            
            // Compute convex hull for each decomposed polygon
            foreach(var polygon in decomposed)
            {
                if(result == null)
                {
                    result = ComputeMinowskiSum(polygon);
                    continue;
                }
                result = GeometryOperations.Union(result, ComputeMinowskiSum(polygon));
            }            
            return GeometryOperations.Union(result);
        }

        private IGeometry ComputeMinowskiSum(IPolygon polygon)
        {
            var verticesA = Robot.Geometry.Coordinates.ToOpenRing();
            var verticesB = polygon.Coordinates.ToOpenRing();
            var resultVertices = new List<Coordinate>();
            foreach(var vertexA in verticesA)
            {
                foreach(var vertexB in verticesB)
                {
                    resultVertices.Add(CoordinateOperations.Add(vertexA, vertexB));
                }                
            }
            var ring = resultVertices.ToClosedRing();
            var shell = GeometryFactory.CreatePolygon(ring.ToArray());
            return shell.ConvexHull();
        }
        
        #endregion Internal Methods
    }
}