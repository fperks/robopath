// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: MainWindow.xaml.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Fluent;

using GalaSoft.MvvmLight.Ioc;

using NLog;

using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.Drawing.RobotPreview;
using RoboPath.PlannerApp.ViewModel;

namespace RoboPath.PlannerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        #region Public Methods
        
        public MainWindow()
        {
            InitializeComponent();

            var locator = new ClientServiceLocator();
            
            // Inject our Zoomable ZoomControl Control
            locator.Main.MapContentCanvas = MapContent;
            locator.Main.ZoomControl = MapZoomControl;

            MapContent.Loaded += (sender, args) =>
                                     {
                                         SimpleIoc.Default.Register<MapRenderer>(() =>
                                                                                     {
                                                                                         return new MapRenderer(MapLayerHost, MapZoomControl);
                                                                                     });

                                         SimpleIoc.Default.Register<RobotPreviewRenderer>(() =>
                                                                                              {
                                                                                                  return new RobotPreviewRenderer(RobotPreviewLayerHost, RobotPreviewZoomControl);
                                                                                              });

                                         //MapZoomControl.ScaleToFit();
                                         locator.Main.Initialize();
                                         locator.RobotPreviewViewModel.Initialize(RobotPreviewZoomControl, RobotPreviewLayerHost);
                                         locator.PathPlannerViewModel.Initialize();
                                         locator.DrawingLayersViewModel.Initialize();

                                         locator.MapRenderer.Draw();
                                     };

        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods

    }
}
