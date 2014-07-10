// *******************************************************
// Project: RoboPath.UI
// File Name: DefinePolygonTool.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.UI;
using RoboPath.UI.WPF;

namespace RoboPath.PlannerApp.Tools.Editing
{
    public class DefinePolygonTool : DefineGeometryTool
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override string Name
        {
            get { return typeof(DefinePolygonTool).Name; }
        }

        #endregion Properties

        #region Public Methods

        public DefinePolygonTool(TargetSpaceType targetSpaceType)
            : base(targetSpaceType)
        {
            OnMouseDown = HandleMouseClick;
            OnMouseDoubleClick = HandleDoubleClick;
            OnMouseMove = HandleMouseMove;
        }

        public void AddPolygonVertex(Coordinate vertex)
        {
            Log.Debug("Adding Polygon Vertex [ {0} ]", vertex);
            if(!TargetWorkspace.Bounds.Contains(vertex))
            {
                Log.Debug("Vertex is outside of Bounds, ignoring");
                return;
            }
            Vertices.Add(vertex);
            RedrawToolLayer();
        }
        
        
        public override IGeometry CreateGeometry()
        {
            if(!IsGeometryValid)
            {
                return null;
            }
            var vertices = Vertices.ToClosedRing();
            var polygonBuilder = new ComplexPolygonBuilder(GeometryFactory, vertices);
            polygonBuilder.BuildPolygons();
            return polygonBuilder.Polygons.ToGeometryCollection();
        }

        public override void Draw(DrawingVisual drawingVisual)
        {
            var serviceLocator = new ClientServiceLocator();
            var renderer = serviceLocator.MapRenderer;

            var pen = TargetSpaceType == TargetSpaceType.Freespace ? MapRenderer.AddObstacleHolePen : MapRenderer.GeometryOutlinePen;
            var brush = TargetSpaceType == TargetSpaceType.Freespace ? Brushes.Transparent : MapRenderer.ToolDefinePolygonGeometryBrush;
            using (var context = drawingVisual.RenderOpen())
            {
                if(Vertices.Count == 2)
                {
                    // Draw a Line
                    context.DrawLine(MapRenderer.GeometryOutlinePen, Vertices.First().ToWPFPoint(), Vertices.Last().ToWPFPoint());
                }
                else if(Vertices.Count >= 3)
                {
                    // Create a WPF Polygon since it is simpler then creating NTS Geometry (due to non simple polygons being jerks)
                    var geometry = WPFGeometryFactory.CreateWPFPolygon(GeometryFactory, Vertices);
                    context.DrawGeometry(brush, pen, geometry);
                }

                // Draw Vertices
                renderer.DrawVertexRange(context, Vertices, MapRenderer.VertexType.Geometry);

            }
        }

        #endregion Public Methods

        #region Internal Methods

        protected void HandleMouseMove(Coordinate position)
        {
            Vertices.Add(position);
            RedrawToolLayer();
            Vertices.RemoveAt(Vertices.Count - 1);            
        }

        protected void HandleMouseClick(Coordinate position, MouseButtonEventArgs args)
        {
            switch (args.ChangedButton)
            {
                case MouseButton.Left:
                    AddPolygonVertex(position);
                    break;
                case MouseButton.Right:
                    Deactivate();
                    break;
                case MouseButton.Middle:
                    CommitGeometry(false);
                    break;
            }
        }

        protected void HandleDoubleClick(Coordinate position, MouseButtonEventArgs args)
        {
            switch (args.ChangedButton)
            {
                case MouseButton.Left:
                    CommitGeometry(false);
                    break;
            }
        }

        protected override bool ValidateGeometry()
        {
            if(Vertices.Count < 3)
            {
                return false;
            }
            return true;
        }

        #endregion Internal Methods
    }
}