//// *******************************************************
//// Project: RoboPath.Core
//// File Name: MinowskiSums.cs
//// By: Frank Perks
//// *******************************************************

//using System;
//using System.Collections.Generic;
//using System.Linq;

//using GeoAPI.Geometries;

//using NLog;

//using RoboPath.Core.Geometry;
//using RoboPath.Core.Robot;

//namespace RoboPath.Core.Algorithms
//{
//    public class MinowskiSums : IMinowskiSums
//    {
//        #region Fields

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        private List<List<IPolygon>> _decomposed;
//        private List<IPolygon> _result;
        
//        #endregion Fields

//        #region Properties

//        public IGeometryFactory GeometryFactory { get; private set; }
//        public IPolygonDecomposition DecompositionStrategy { get; private set; }

//        public IRobot InputRobot { get; private set; }

//        public IList<IGeometry> InputGeometry { get; private set; }

//        public IList<IPolygon> DecomposedPolygons
//        {
//            get
//            {
//                var decomposed = new List<IPolygon>();
//                foreach(var decomposedPolygons in _decomposedPolygons)
//                {
//                    decomposed.AddRange(decomposedPolygons);
//                }
//                return decomposed;
//            }
//        }

//        public IList<IPolygon> Result
//        {
//            get { return _result; }
//        }

//        #endregion Properties

//        #region Public Methods

//        public MinowskiSums(IGeometryFactory geometryfactory, IPolygonDecomposition decompositionStrategy, IList<IGeometry> inputGeometry, IRobot inputRobot)
//        {
//            GeometryFactory = geometryfactory;
//            DecompositionStrategy = decompositionStrategy;
//            InputGeometry = inputGeometry;
//            InputRobot = inputRobot;
//        }

//        public void Solve()
//        {
//            Log.Debug("Computing Minowski Sums of Input Geometry:{0}", string.Join("\n", InputGeometry));

//            var inputPolygons = 
//            if(InputRobot.BodyType == RobotBodyType.Point)
//            {
                
//            }

//            // Build our list of source polygons
//            DecomposeInputPolygons();
//            if(!SourcePolygons.Any())
//            {
//                throw new InvalidOperationException("No Valid Input Geometry was provided");
//            }

//            throw new NotImplementedException();
//        }

//        #endregion Public Methods

//        #region Internal Methods

//        private void DecomposeInputPolygons()
//        {
//            Log.Debug("Creating Source Polygons");
//            _sourcePolygons = new List<List<IPolygon>>();

//            // Filter Input for only polygons
//            var polygons = FilterInput();
//            if(!polygons.Any())
//            {
//                throw new InvalidOperationException("No Valid Input was Provided");
//            }

//            foreach(var polygon in polygons)
//            {
//                if(polygon.IsConvex())
//                {
//                    // Decompose 
//                    _sourcePolygons.Add(DecomposePolygon(polygon));
//                }
//                else
//                {
//                    // We don't need to decompose, so just add it
//                    _sourcePolygons.Add(new List<IPolygon>
//                                            {
//                                                polygon
//                                            });
//                }
//            }

//        }

//        private List<IPolygon> DecomposePolygon(IPolygon polygon)
//        {
//            Log.Debug("Decomposing Polygon [ {0} ]", polygon.AsText());
//            var result = DecompositionStrategy.DecomposePolygon(polygon);

//            if(!result.Any())
//            {
//                throw new InvalidOperationException(string.Format("Decomposition of Polygon [ {0} ] failed", polygon.AsText()));
//            }
//        }

//        private List<IPolygon> FilterInput()
//        {
//            var result = new List<IPolygon>();
//            foreach(var geometry in InputGeometry)
//            {
//                if(geometry.OgcGeometryType == OgcGeometryType.Polygon && !geometry.IsEmpty)
//                {
//                    result.Add((IPolygon) geometry);
//                }
//                else
//                {
//                    Log.Warn("Input Geometry [ {0} ] is not Valid", geometry);
//                }
//            }
//            return result;
//        }

//        #endregion Internal Methods


//    }
//}