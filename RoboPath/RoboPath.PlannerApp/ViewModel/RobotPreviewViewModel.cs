// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: RobotPreviewViewModel.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Windows.Controls;
using System.Windows.Media;

using GalaSoft.MvvmLight.Ioc;

using GeoAPI.Geometries;

using NetTopologySuite.Windows.Media;

using RoboPath.Core.Robot;
using RoboPath.Core.Robot.Geometric;
using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.RobotPreview;
using RoboPath.PlannerApp.PathPlanning.Robot;
using RoboPath.PlannerApp.Properties;
using RoboPath.UI.Controls;
using RoboPath.UI.Drawing;

using WPFPoint = System.Windows.Point;
using WPFGeometry = System.Windows.Media.Geometry;

using GalaSoft.MvvmLight;

using NLog;

namespace RoboPath.PlannerApp.ViewModel
{
    public class RobotPreviewViewModel : ViewModelBase
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private ClientServiceLocator _serviceLocator;

        #endregion Fields

        #region Properties

        public ClientServiceLocator ServiceLocator
        {
            get
            {
                if(_serviceLocator == null)
                {
                    _serviceLocator = new ClientServiceLocator();
                }
                return _serviceLocator;
            }
        }

        public ZoomAndPanControl ZoomControl { get; private set; }
        public DrawingLayerHost LayerHost { get; private set; }
        
        #endregion Properties

        #region Public Methods

        public RobotPreviewViewModel()
        {            
        }

        public void Initialize(ZoomAndPanControl zoomControl, DrawingLayerHost layerHost)
        {
            ZoomControl = zoomControl;
            LayerHost = layerHost;

            //SimpleIoc.Default.Register<RobotPreviewRenderer>(() =>
            //                                                     {
            //                                                         return new RobotPreviewRenderer(LayerHost, zoomControl);
            //                                                     }, true);

            ZoomControl.MaxContentScale = 10.0;
            UpdateRobotPreview(ServiceLocator.RobotManager.CurrentRobot);
        }

        public void UpdateRobotPreview(IRobot robot)
        {
            ServiceLocator.RobotPreviewRenderer.Draw();
        }

        #endregion Public Methods

        #region Internal Methods
    
        #endregion Internal Methods
    }
}