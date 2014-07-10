// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ZoomToRegionTool.cs
// By: Frank Perks
// *******************************************************

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using GeoAPI.Geometries;

using NLog;

using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.UI;
using RoboPath.UI.Controls;

namespace RoboPath.PlannerApp.Tools.Canvas
{
    public class ZoomToRegionTool : CanvasTool
    {

        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public Coordinate StartPosition { get; set; }
        public Coordinate CurrentPosition { get; set; }

        public override string Name
        {
            get { return typeof(ZoomToExtentTool).Name; }
        }

        public override bool IsInteractive
        {
            get { return true; }
        }

        #endregion Properties

        #region Public Methods

        public ZoomToRegionTool(ZoomAndPanControl zoomControl)
            : base(zoomControl)
        {
            OnMouseUp = HandleMouseUp;
            OnMouseDown = HandleMouseDown;
            OnMouseMove = HandleMouseMove;
        }

        public override void Draw(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                if(CurrentPosition == null || StartPosition == null)
                {
                    return;
                }

                var envelope = new Envelope(StartPosition, CurrentPosition);
                context.DrawRectangle(Brushes.Transparent, MapRenderer.SelectionRegionPen, envelope.ToWPFRect());
            }
        }

        #endregion Public Methods

        #region Internal Methods

        private void ApplyZoom()
        {
            var serviceLocator = new ClientServiceLocator();
            if(serviceLocator.MapRenderer != null && StartPosition != null && CurrentPosition != null)
            {
                serviceLocator.MapRenderer.ZoomToRegion(StartPosition, CurrentPosition);    
            }
            StartPosition = null;
            CurrentPosition = null;
            RedrawToolLayer();
        }

        protected void HandleMouseDown(Coordinate position, MouseButtonEventArgs args)
        {
            switch(args.ChangedButton)
            {
                case MouseButton.Left:
                    StartPosition = position;
                    break;
                case MouseButton.Right:
                    Deactivate();
                    break;
            }
        }

        protected void HandleMouseUp(Coordinate position, MouseButtonEventArgs args)
        {
            switch(args.ChangedButton)
            {
                case MouseButton.Left:
                    CurrentPosition = position;
                    ApplyZoom();
                    break;
            }
        }

        protected void HandleMouseMove(Coordinate position)
        {
            CurrentPosition = position;
            RedrawToolLayer();
        }

        #endregion Internal Methods
    }
}