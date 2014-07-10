//// *******************************************************
//// Project: RoboPath.Core
//// File Name: RobotSafetyBuffer.cs
//// By: Frank Perks
//// *******************************************************

//using System;
//using System.Collections.Generic;
//using System.Linq;

//using GeoAPI.Geometries;
//using GeoAPI.Operations.Buffer;

//using NLog;

//using NetTopologySuite.Operation.Buffer;

//using RoboPath.Core.Geometry;
//using RoboPath.Core.Robot;
//using RoboPath.Core.Robot.Geometric;

//namespace RoboPath.Core.Algorithms
//{
//    public class RobotSafetyBuffer : IRobotSafetyBuffer
//    {
//        #region Fields

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        private List<IGeometry> _result;
//        private List<IGeometry> _sourcePolygons; 

//        #endregion Fields

//        #region Properties

//        public IGeometryFactory GeometryFactory { get; private set; }
//        public IRobot Robot { get; private set; }
//        public IList<IGeometry> InputGeometry { get; private set; }

//        public IPolygonDecomposition PolygonDecompositionStrategy { get; private set; }

//        public IList<IGeometry> Result
//        {
//            get { return _result; }
//        }

//        #endregion Properties

//        #region Public Methods

//        public RobotSafetyBuffer(IGeometryFactory geometryFactory, IRobot robot, IList<IGeometry> inputGeometry, IPolygonDecomposition polygonDecompositionStrategy)
//        {
//            GeometryFactory = geometryFactory;
//            Robot = robot;
//            InputGeometry = inputGeometry;
//            PolygonDecompositionStrategy = polygonDecompositionStrategy;
//        }

//        public void Solve()
//        {
//            _result = new List<IGeometry>();
//            _sourcePolygons = new List<IGeometry>();
//            var validInput = FilterInput();

//            // Copy the input polygon
//            foreach(var inputPolygon in validInput)
//            {
//                var copy = GeometryFactory.CreateGeometry(inputPolygon);
//                _sourcePolygons.Add(copy);
//            }

//            switch(Robot.BodyType)
//            {                
//                case RobotBodyType.Point:
//                    ApplyPointBuffer((PointRobot)Robot);
//                    break;
//                case RobotBodyType.Circle:
//                    ApplyCircularBuffer((CircularRobot)Robot);
//                    break;
//                case RobotBodyType.Polygon:
//                    ApplyPolygonalBuffer((PolygonalRobot)Robot);
//                    break;
//                default:
//                    throw new InvalidOperationException(string.Format("Unsupported Robot Body Type [ {0} ]", Robot.BodyType));
//            }

//            throw new NotImplementedException();
//        }

//        #endregion Public Methods

//        #region Internal Methods

//        private void ApplyPointBuffer(PointRobot robot)
//        {
//            // Applying a point buffer does nothing, the robot is a point, so it doesn't change
//            _result = _sourcePolygons.ToList();
//        }

//        private void ApplyCircularBuffer(CircularRobot robot)
//        {

//        }

//        private void ApplyPolygonalBuffer(PolygonalRobot robot)
//        {
//            // We apply Minowksi Sums using the input robot's body as Q, and P as the input geometry
            
//        }

//        private List<IGeometry> FilterInput()
//        {
//            var result = new List<IGeometry>();
//            foreach(var geometry in InputGeometry)
//            {
//                var polygons = geometry.Get();
//                foreach(var polygon in polygons)
//                {
//                    // Make a copy
//                    var copy = GeometryFactory.CreateGeometry(polygon);
//                    result.Add(copy);
//                }
//            }
//            return result;
//        }

//        #endregion Internal Methods

//    }
//}