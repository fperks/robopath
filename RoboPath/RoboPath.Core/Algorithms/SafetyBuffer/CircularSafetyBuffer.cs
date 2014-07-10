// *******************************************************
// Project: RoboPath.Core
// File Name: CircularSafetyBuffer.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;

using NLog;

using NetTopologySuite.Operation.Buffer;

using RoboPath.Core.Geometry;
using RoboPath.Core.Robot.Geometric;

namespace RoboPath.Core.Algorithms.SafetyBuffer
{
    public class CircularSafetyBuffer : ISafetyBuffer<CircularRobot>
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<IPolygon> _result;
        private List<IPolygon> _sourcePolygons; 

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public IPolygonDecomposition PolygonDecompositionStrategy { get; private set; }
        public CircularRobot Robot { get; private set; }

        public IList<IPolygon> InputPolygons { get; private set; }
        public IList<IPolygon> Result
        {
            get { return _result; }
        }

        public int CircleEdgeCount { get; set; }

        #endregion Properties

        #region Public Methods

        public CircularSafetyBuffer(IGeometryFactory geometryFactory, CircularRobot robot, IList<IPolygon> inputPolygons)
        {
            CircleEdgeCount = Constants.DefaultCircleGeometryEdgeCount;

            GeometryFactory = geometryFactory;
            Robot = robot;
            InputPolygons = inputPolygons;
            PolygonDecompositionStrategy = null;
        }

        public void Solve()
        {
            _result = new List<IPolygon>();
            if(!InputPolygons.Any())
            {
                return;
            }

            ComputeSourcePolygons();
            ApplyBuffer();
        }

        #endregion Public Methods

        #region Internal Methods

        private void ComputeSourcePolygons()
        {
            // Generate our source polygons
            _sourcePolygons = new List<IPolygon>();
            foreach(var polygon in InputPolygons)
            {
                // Make a copy
                var copy = (IPolygon) GeometryFactory.CreateGeometry(polygon);
                _sourcePolygons.Add(copy);
            }
        }

        private void ApplyBuffer()
        {
            // Normally we would have to apply Minowski Sums using Convolution, however NTS supports round polygon buffer
            // that deals with non convex polygons, so we use that. 
            // We use a default edge count
            var bufferParameters = new BufferParameters(CircleEdgeCount, EndCapStyle.Round);

            // Buffer occupied space
            var occupied = GeometryOperations.Union(_sourcePolygons);
            var bufferedGeometry = occupied.Buffer(Robot.Radius, bufferParameters);

            // extract resulting polygons
            var polygons = bufferedGeometry.GetPolygons();
            _result = new List<IPolygon>(polygons.ToList());
        }

        #endregion Internal Methods
    }
}