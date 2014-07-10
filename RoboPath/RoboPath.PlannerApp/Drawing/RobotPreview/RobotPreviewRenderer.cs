// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: RobotPreviewRenderer.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core;
using RoboPath.Core.Geometry;
using RoboPath.PlannerApp.PathPlanning.Robot;
using RoboPath.UI.Controls;
using RoboPath.UI.Drawing;

namespace RoboPath.PlannerApp.Drawing.RobotPreview
{
    public class RobotPreviewRenderer : BaseRenderer
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ClientServiceLocator _serviceLocator;

        #endregion Fields

        #region Properties

        public const string OverlayLayer = "Overlay";
        public const string RobotBodyLayer = "BodyGeometry";
        public const string Background = "Background";

        #region Resources

        public static ImageSource OriginSymbol { get; set; }
        public static ImageSource VertexSymbol { get; set; }
        public static Pen RobotPreviewGenericPen { get; set; }
        public static Pen RobotPreviewBodyPen { get; set; }
        public static Pen RobotPreviewGridPen { get; set; }
        public static Brush RobotPreviewBodyBrush { get; set; }
        
        #endregion Resources

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

        public Coordinate CanvasCenter
        {
            get { return new Coordinate(ZoomControl.ContentViewportWidth / 2.0, ZoomControl.ContentViewportHeight / 2.0); }
        }

        #endregion Properties

        #region Public Methods

        public RobotPreviewRenderer(DrawingLayerHost layerHost, ZoomAndPanControl zoomControl)
            : base(layerHost, zoomControl)
        {
            LoadResources();

        }

        public override void Initialize()
        {
            LoadResources();
            LoadDefaultLayers();
            ServiceLocator.RobotManager.CurrentRobotChanged += (sender, args) =>
            {
                Draw();
            };
        }

        #endregion Public Methods

        #region Internal Methods

        protected override void LoadDefaultLayers()
        {
            LayerHost.AddLayer(OverlayLayer, DrawOverlay);
            LayerHost.AddLayer(RobotBodyLayer, DrawRobotBody);
            LayerHost.AddLayer(Background, DrawBackground);
        }

        protected void LoadResources()
        {
            RobotPreviewBodyPen = GetDrawingResource<Pen>("GeometryOutlinePen");
            RobotPreviewGenericPen = GetDrawingResource<Pen>("GeometryOutlinePen");
            RobotPreviewGridPen = GetDrawingResource<Pen>("RobotPreviewGridPen");

            RobotPreviewBodyBrush = GetDrawingResource<SolidColorBrush>("RobotPreviewBodyBrush");
            
            OriginSymbol = GetDrawingResource<ImageSource>("OrangeVertexSymbol32");
            VertexSymbol = GetDrawingResource<ImageSource>("BlueVertexSymbol16");
        }

        protected override void OnZoomScaleChanged(double newScale)
        {
        }

        private void DrawOverlay(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                // Center Vertex
                context.DrawVertexAsImage(OriginSymbol, CanvasCenter);

                // Origin Label
                var text = new FormattedText(
                    string.Format("({0},{1})", Constants.RobotBodyCenter.X, Constants.RobotBodyCenter.Y), 
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    9 / ZoomControl.ContentScale,
                    Brushes.Black);
                var origin = new Point(CanvasCenter.X + 5.0, CanvasCenter.Y - 10.0);
                context.DrawText(text, origin);
            }            
        }

        private void DrawRobotBody(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var robotManager = ServiceLocator.RobotManager;
                if(robotManager.CurrentRobot == null || robotManager.CurrentRobotType == RobotType.Point)
                {
                    return;
                }

                var geometry = ServiceLocator.RobotManager.CurrentRobot.GetConfiguration(CanvasCenter).Geometry;
                context.DrawGeometry(geometry, Brushes.Transparent, RobotPreviewBodyPen);
                foreach(var vertex in geometry.GetVertices())
                {
                    context.DrawVertexAsImage(VertexSymbol, vertex);
                }
            }   
        }

        private void DrawBackground(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                //var width = (int)ZoomControl.ActualWidth / 5;
                //var height = (int)ZoomControl.ActualHeight;

                //for(var y = 0; y <= height; y+=10)
                //{
                //    for(var x = 0; x <= width; x+=10)
                //    {
                        
                //    }
                //}
                //context.DrawRectangle(null, RobotPreviewGridPen, );
            }
        }

        #endregion Internal Methods
    }
}