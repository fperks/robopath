//// *******************************************************
//// Project: RoboPath.Core
//// File Name: GeometrySettingsManager.cs
//// By: Frank Perks
//// *******************************************************

//using System;
//using System.Diagnostics;

//using GeoAPI.Geometries;

//using NLog;

//using NetTopologySuite.Geometries;

//namespace RoboPath.Core
//{
//    public sealed class GeometrySettingsManager
//    {
//        #region Fields

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
//        private static readonly Lazy<GeometrySettingsManager> UniqueInstance = new Lazy<GeometrySettingsManager>(() => new GeometrySettingsManager());

//        private readonly IPrecisionModel _defaultPrecisionModel;
//        private readonly IGeometryFactory _defaultGeometryFactory;

//        private IGeometryFactory _geometryFactory;
        
//        #endregion Fields

//        #region Properties

//        public static GeometrySettingsManager Instance
//        {
//            get { return UniqueInstance.Value; }
//        }

//        public IGeometryFactory GeometryFactory
//        {
//            get
//            {
//                Debug.Assert(_geometryFactory != null);
//                return _geometryFactory;
//            }
//            set { _geometryFactory = value; }
//        }

//        public IGeometryFactory DefaultGeometryFactory
//        {
//            get { return _defaultGeometryFactory; }
//        }

//        public IPrecisionModel DefaultPrecisionModel
//        {
//            get { return _defaultPrecisionModel; }
//        }


//        #endregion Properties

//        #region Public Methods

//        private GeometrySettingsManager()
//        {
//            _defaultPrecisionModel = new PrecisionModel(Constants.DefaultPrecisionModelType);
//            _defaultGeometryFactory = new GeometryFactory(_defaultPrecisionModel);
//        }

//        #endregion Public Methods

//        #region Internal Methods

//        #endregion Internal Methods
//    }
//}