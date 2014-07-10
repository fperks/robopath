// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DefineRobotGeometryTool.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core;
using RoboPath.Core.Geometry;
using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.PathPlanning;
using RoboPath.PlannerApp.PathPlanning.Robot;
using RoboPath.PlannerApp.Properties;
using RoboPath.UI;
using RoboPath.UI.WPF;

namespace RoboPath.PlannerApp.Tools.Robot
{
    public class DefineRobotGeometryTool : DrawableTool
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; set; }

        public override bool IsInteractive
        {
            get { return true; }
        }

        public override string Name
        {
            get { return typeof(DefineRobotGeometryTool).Name; }
        }

        public bool IsValidRobotBody
        {
            get { return IsValid(); }
        }

        public List<Coordinate> Vertices { get; private set; }

        #endregion Properties

        #region Public Methods

        public DefineRobotGeometryTool(IGeometryFactory geometryFactory)
        {
            Vertices = new List<Coordinate>();
            GeometryFactory = geometryFactory;

            OnMouseMove = HandleMouseMove;
            OnMouseDown = HandleMouseClick;
            OnMouseDoubleClick = HandleDoubleClick;
        }

        public void AddVertex(Coordinate position)
        {         
            Vertices.Add(position);
        }

        public void CommitRobotGeometry()
        {
            if(!IsValidRobotBody)
            {
                Log.Warn("Attempting to Commit Invalid Robot Geometry");
                Deactivate();
                return;
            }

            var geometry = CreateRobotGeometry();
            
            // Update the our manager with the new robot body
            var serviceLocator = new ClientServiceLocator();
            var robotManager = serviceLocator.RobotManager;
            var setup = serviceLocator.PathPlannerSetup.CurrentSetup;
            if(setup.StartPosition == null)
            {
                setup.StartPosition = geometry.Centroid.Coordinate;
            }

            robotManager.PolygonalBodyGeometry = geometry;
            robotManager.CurrentRobotType = RobotType.Polygon;
            
            // Deactivate this tool
            Deactivate();
        }

        public override void Draw(DrawingVisual drawingVisual)
        {
            var serviceLocator = new ClientServiceLocator();
            var renderer = serviceLocator.MapRenderer;
            using(var context = drawingVisual.RenderOpen())
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
                    context.DrawGeometry(MapRenderer.ToolDefinePolygonGeometryBrush, MapRenderer.GeometryOutlinePen, geometry);
                }

                // Draw Vertices
                renderer.DrawVertexRange(context, Vertices, MapRenderer.VertexType.Geometry);                
            }
        }
        
        #endregion Public Methods

        #region Internal Methods

        protected virtual bool IsValid()
        {
            if(Vertices.Count < 3)
            {
                return false;
            }
            return true;
        }

        protected virtual IGeometry CreateRobotGeometry()
        {
            if(!IsValidRobotBody)
            {
                return null;
            }

            var vertices = Vertices.ToClosedRing();
            var polygonBuilder = new ComplexPolygonBuilder(GeometryFactory, vertices);
            polygonBuilder.BuildPolygons();
            var body = polygonBuilder.Polygons.ToGeometryCollection();

            // If we want to force the user to create a convex body then we use the convex hull
            if(Settings.Default.EnforceConvexRobotBody)
            {
                return body.ConvexHull();    
            }
            return GeometryOperations.Union(body);
        }

        protected void HandleMouseMove(Coordinate position)
        {
            Vertices.Add(position);
            RedrawToolLayer();
            Vertices.RemoveAt(Vertices.Count - 1);
        }

        protected void HandleMouseClick(Coordinate position, MouseButtonEventArgs args)
        {
            switch(args.ChangedButton)
            {
                case MouseButton.Left:
                    AddVertex(position);
                    break;
                case MouseButton.Right:
                    Deactivate();
                    break;
                case MouseButton.Middle:
                    CommitRobotGeometry();
                    break;
            }
        }

        protected void HandleDoubleClick(Coordinate position, MouseButtonEventArgs args)
        {
            switch(args.ChangedButton)
            {
                case MouseButton.Left:
                    CommitRobotGeometry();
                    break;
            }
        }

        #endregion Internal Methods
    }
}