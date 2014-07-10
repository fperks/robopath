// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: MapRenderer.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GalaSoft.MvvmLight.Ioc;

using GeoAPI.Geometries;

using Microsoft.Practices.ServiceLocation;

using NLog;

using NetTopologySuite.Geometries;

using RoboPath.Core;
using RoboPath.Core.Geometry;
using RoboPath.Core.Graph;
using RoboPath.Core.PathPlanning;
using RoboPath.Core.PathPlanning.Geometric;
using RoboPath.Core.PathPlanning.Geometric.Decomposition;
using RoboPath.PlannerApp.PathPlanning;
using RoboPath.PlannerApp.Properties;
using RoboPath.PlannerApp.Tools;
using RoboPath.UI;
using RoboPath.UI.Controls;
using RoboPath.UI.Drawing;

namespace RoboPath.PlannerApp.Drawing.Map
{
    public class MapRenderer : BaseRenderer
    {
        public enum VertexType
        {
            Start,
            Goal,
            Geometry,
            Graph,
            ShortestPath,
            PathPlanner
        }

        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private MapRenderState _renderState;
        private ClientServiceLocator _serviceLocator;

        #endregion Fields

        #region Properties

        #region Resources

        public static ImageSource GenericStarGreenSymbol16 { get; set; }
        public static ImageSource GenericStarGreenSymbol32 { get; set; }

        public static ImageSource GraphStartSymbol16 { get; set; }
        public static ImageSource GraphStartSymbol32 { get; set; }

        public static ImageSource GraphGoalSymbol16 { get; set; }
        public static ImageSource GraphGoalSymbol32 { get; set; }

        public static ImageSource BlueVertexSymbol16 { get; set; }
        public static ImageSource BlueVertexSymbol32 { get; set; }

        public static ImageSource OrangeVertexSymbol16 { get; set; }
        public static ImageSource OrangeVertexSymbol32 { get; set; }

        public static ImageSource BlackVertexSymbol16 { get; set; }
        public static ImageSource BlackVertexSymbol32 { get; set; }

        public static ImageSource VertexGraphSymbol32 { get; set; }
        public static ImageSource VertexShortestPathSymbol32 { get; set; }
        public static ImageSource VertexPathPlannerSymbol32 { get; set; }

        public static ImageSource RedVertexSymbol32 { get; set; }
        public static ImageSource PinkVertexSymbol32 { get; set; }

        public static Pen SelectionRegionPen { get; set; }
        public static Pen GeometryOutlinePen { get; set; }
        public static Pen BoundaryPen { get; set; }
        public static Pen RobotConfigurationPen { get; set; }
        public static Pen DecomposedObstaclesPen { get; set; }
        public static Pen AddObstacleHolePen { get; set; }

        public static Pen GraphEdgePen { get; set; }
        //public static Pen GraphVertexPen { get; set; }
        public static Pen GraphShortestPathEdgePen { get; set; }

        public static Pen PlannerTriangulationInvalidTrianglePen { get; set; }
        public static Pen PlannerTriangulationValidTrianglePen { get; set; }

        public static Brush RobotConfigurationBrush { get; set; }
        public static Brush WorkspaceGeometryBrush { get; set; }
        public static Brush WorkspaceVertexBrush { get; set; }

        public static Brush CSpaceGeometryBrush { get; set; }
        public static Brush CSpaceVertexBrush { get; set; }

        public static Brush StartPositionBrush { get; set; }
        public static Brush GoalPositionBrush { get; set; }

        public static Brush ToolDefinePolygonGeometryBrush { get; set; }
        public static Brush ToolDefinePolygonVertexBrush { get; set; }

        public static Brush SelectionRegionBrush { get; set; }
        public static Brush DecomposedObstaclesBrush { get; set; }

        public static Brush GraphVertexBrush { get; set; }
        public static Brush GraphShortestPathVertexBrush { get; set; }

        public static Brush PlannerTriangulationValidTriangleBrush { get; set; }

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

        public MapRenderState RenderState
        {
            get { return _renderState; }
            set { OnRenderStateChanged(value); }
        }

        #endregion Properties

        #region Public Methods

        public MapRenderer(DrawingLayerHost layerHost, ZoomAndPanControl zoomControl)
            : base(layerHost, zoomControl)
        {
            _renderState = MapRenderState.Initial;
        }

        public override void Initialize()
        {
            _renderState = MapRenderState.Initial;
            LoadDrawingResources();
            LoadDefaultLayers();

            ZoomControl.ContentScaleChanged += OnContentScaleChanged;

            ServiceLocator.RobotManager.CurrentRobotChanged += (sender, args) =>
                                                                   {
                                                                       Clear();
                                                                       Draw();
                                                                   };

        }

        public void RescaleResources()
        {
            GeometryOutlinePen = RescalePen(GeometryOutlinePen);
            BoundaryPen = RescalePen(BoundaryPen);
            RobotConfigurationPen = RescalePen(DecomposedObstaclesPen);
            GraphEdgePen = RescalePen(GraphEdgePen);
            //AddObstacleHolePen = RescalePen(AddObstacleHolePen);
            GraphShortestPathEdgePen = RescalePen(GraphShortestPathEdgePen);
            PlannerTriangulationInvalidTrianglePen = RescalePen(PlannerTriangulationInvalidTrianglePen);
            PlannerTriangulationValidTrianglePen = RescalePen(PlannerTriangulationValidTrianglePen);
        }

        public override void Draw()
        {
            //RescaleResources();
            base.Draw();
        }

        public override BitmapSource ToBitmap()
        {
            Log.Debug("Generating Bitmap");

            var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
            // Write it to the bitmaps
            Draw();

            var bitmap = new RenderTargetBitmap((int)plannerSetup.Bounds.Width, (int)plannerSetup.Bounds.Height, 96, 96, PixelFormats.Pbgra32);

            // Grab the visibile layers
            foreach(var visual in from layer in LayerHost.Layers
                                  orderby layer.ZIndex
                                  select layer)
            {
                bitmap.Render(visual);
            }
            return bitmap;
        }

        #region Drawing Helpers

        public void DrawVertex(DrawingContext context, Coordinate position, VertexType vertexType, bool applyScale = false)
        {
            var sourceImage = GetVertexImage(vertexType);
            if(applyScale)
            {
                context.DrawScaledVertexAsImage(sourceImage, position, ZoomScale);
            }
            else
            {
                context.DrawVertexAsImage(sourceImage, position);
            }
        }

        public void DrawVertexRange(DrawingContext context, IEnumerable<Coordinate> vertices, VertexType vertexType, bool applyScale = false)
        {
            foreach(var vertex in vertices)
            {
                var sourceImage = GetVertexImage(vertexType);
                if(applyScale)
                {
                    context.DrawScaledVertexAsImage(sourceImage, vertex, ZoomScale);
                }
                else
                {
                    context.DrawVertexAsImage(sourceImage, vertex);    
                }                
            }
        }
        
        #endregion Drawing Helpers

        #endregion Public Methods

        #region Internal Methods

        protected override void LoadDefaultLayers()
        {
            LayerHost.AddLayer(MapLayers.ToolLayer, DrawTool);
            LayerHost.AddLayer(MapLayers.StartAndGoalLayer, DrawStartAndGoalPositions);
            LayerHost.AddLayer(MapLayers.GraphShortestPathLayer, DrawShortestPath);
            LayerHost.AddLayer(MapLayers.GraphVerticesLayer, DrawGraphVertices);
            LayerHost.AddLayer(MapLayers.GraphEdgesLayer, DrawGraphEdges);
            LayerHost.AddLayer(MapLayers.PathPlannerLayer, DrawPathPlanner);
            LayerHost.AddLayer(MapLayers.VertexLayer, DrawVertices);
            LayerHost.AddLayer(MapLayers.BoundsLayer, DrawBounds);
            LayerHost.AddLayer(MapLayers.RobotConfigurationsLayer, DrawRobotConfigurations, false);
            LayerHost.AddLayer(MapLayers.DecomposedPolygonsLayer, DrawDecomposedPolygons);
            LayerHost.AddLayer(MapLayers.WorkspaceLayer, DrawWorkspace);
            LayerHost.AddLayer(MapLayers.CSpaceLayer, DrawCSpace);
        }

        #region Resources

        private static ImageSource GetVertexImage(VertexType type)
        {
            switch(type)
            {
                case VertexType.Start:
                    return GraphStartSymbol32;
                case VertexType.Goal:
                    return GraphGoalSymbol32;
                case VertexType.Geometry:
                    return BlueVertexSymbol32;
                case VertexType.Graph:
                    return VertexGraphSymbol32;
                case VertexType.ShortestPath:
                    return VertexShortestPathSymbol32;
                case VertexType.PathPlanner:
                    return VertexPathPlannerSymbol32;
                //case VertexType.Start:
                //    return GraphStartSymbol32;
                //case VertexType.Goal:
                //    return GraphGoalSymbol32;
                //case VertexType.Geometry:
                //    return BlackVertexSymbol32;
                //case VertexType.Graph:
                //    return VertexGraphSymbol32;
                //case VertexType.ShortestPath:
                //    return VertexShortestPathSymbol32;
                //case VertexType.PathPlanner:
                //    return VertexPathPlannerSymbol32;
            }
            throw new DrawingOperationException("Invalid Vertex Type");
        }



        private void LoadDrawingResources()
        {
            Log.Debug("Loading Drawing Resources");

            // Images
            GraphStartSymbol16 = GetDrawingResource<ImageSource>("GenericStarGreenSymbol16");
            GraphStartSymbol32 = GetDrawingResource<ImageSource>("GenericStarGreenSymbol32");

            GraphStartSymbol16 = GetDrawingResource<ImageSource>("GraphStartSymbol16");
            GraphStartSymbol32 = GetDrawingResource<ImageSource>("GraphStartSymbol32");

            GraphGoalSymbol16 = GetDrawingResource<ImageSource>("GraphGoalSymbol16");
            GraphGoalSymbol32 = GetDrawingResource<ImageSource>("GraphGoalSymbol32");

            BlueVertexSymbol16 = GetDrawingResource<ImageSource>("BlueVertexSymbol16");
            BlueVertexSymbol32 = GetDrawingResource<ImageSource>("BlueVertexSymbol32");

            OrangeVertexSymbol16 = GetDrawingResource<ImageSource>("OrangeVertexSymbol16");
            OrangeVertexSymbol32 = GetDrawingResource<ImageSource>("OrangeVertexSymbol32");

            BlackVertexSymbol16 = GetDrawingResource<ImageSource>("BlackVertexSymbol16");
            BlackVertexSymbol32 = GetDrawingResource<ImageSource>("BlackVertexSymbol32");

            VertexGraphSymbol32 = GetDrawingResource<ImageSource>("VertexGraph32");
            VertexShortestPathSymbol32 = GetDrawingResource<ImageSource>("VertexShortestPath32");
            VertexPathPlannerSymbol32 = GetDrawingResource<ImageSource>("VertexPathPlanner32");
            
            // Pens
            GeometryOutlinePen = GetDrawingResource<Pen>("GeometryOutlinePen");
            BoundaryPen = GetDrawingResource<Pen>("BoundaryPen");
            RobotConfigurationPen = GetDrawingResource<Pen>("RobotConfigurationPen");
            SelectionRegionPen = GetDrawingResource<Pen>("SelectionRegionPen");
            DecomposedObstaclesPen = GetDrawingResource<Pen>("DecomposedObstaclesPen");
            GraphEdgePen = GetDrawingResource<Pen>("GraphEdgePen");
            AddObstacleHolePen = GetDrawingResource<Pen>("AddObstacleHolePen");

            GraphShortestPathEdgePen = GetDrawingResource<Pen>("GraphShortestPathEdgePen");

            PlannerTriangulationInvalidTrianglePen = GetDrawingResource<Pen>("PlannerTriangulationInvalidTrianglePen");
            PlannerTriangulationValidTrianglePen = GetDrawingResource<Pen>("PlannerTriangulationValidTrianglePen");
            
            // Brushes
            RobotConfigurationBrush = GetDrawingResource<SolidColorBrush>("RobotConfigurationBrush");

            WorkspaceGeometryBrush = GetDrawingResource<SolidColorBrush>("WorkspaceGeometryBrush");
            WorkspaceVertexBrush = GetDrawingResource<SolidColorBrush>("WorkspaceVertexBrush");

            CSpaceGeometryBrush = GetDrawingResource<SolidColorBrush>("CSpaceGeometryBrush");
            CSpaceVertexBrush = GetDrawingResource<SolidColorBrush>("CSpaceVertexBrush");

            StartPositionBrush = GetDrawingResource<RadialGradientBrush>("StartPositionBrush");
            GoalPositionBrush = GetDrawingResource<RadialGradientBrush>("GoalPositionBrush");

            ToolDefinePolygonGeometryBrush = GetDrawingResource<SolidColorBrush>("ToolDefinePolygonGeometryBrush");
            ToolDefinePolygonVertexBrush = GetDrawingResource<SolidColorBrush>("ToolDefinePolygonVertexBrush");

            SelectionRegionBrush = GetDrawingResource<SolidColorBrush>("SelectionRegionBrush");
            DecomposedObstaclesBrush = GetDrawingResource<SolidColorBrush>("DecomposedObstaclesBrush");

            GraphVertexBrush = GetDrawingResource<RadialGradientBrush>("GraphVertexBrush");
            GraphShortestPathVertexBrush = GetDrawingResource<RadialGradientBrush>("GraphShortestPathVertexBrush");

            //PlannerTriangulationInvalidTriangleBrush = GetDrawingResource<SolidColorBrush>("PlannerTriangulationInvalidTriangleBrush");
            PlannerTriangulationValidTriangleBrush = GetDrawingResource<SolidColorBrush>("PlannerTriangulationValidTriangleBrush");

            RescaleResources();
        }

        private Pen RescalePen(Pen pen)
        {
            if(ZoomControl == null)
            {
                return pen;
            }

            //return new Pen(pen.Brush, pen.Thickness);
            //return pen.Thickness;

            var thickness = pen.Thickness / ZoomScale;
            if(thickness < 0.5)
            {
                thickness = MinPenThickness;
            }
            //if(thickness > 2.0)
            //{
            //    thickness = 2.0;
            //}

            ////if(thickness > 1.0)
            ////{
            ////    thickness = 1.0;
            ////}

            return new Pen(pen.Brush, thickness);
        }

        #endregion Resources

        #region Draw Methods

        private void DrawNothing(DrawingVisual visual)
        {
            // Do nothing
        }

        private void DrawTool(DrawingVisual visual)
        {
            var toolManager = ServiceLocator.ToolManager;
            toolManager.Draw(visual);
        }

        private void DrawBounds(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
                if(plannerSetup == null)
                {
                    return;
                }

                context.DrawEnvelope(plannerSetup.Bounds, BoundaryPen);
                DrawVertexRange(context, plannerSetup.Bounds.GetVertices(), VertexType.Geometry);
            }
        }

        private void DrawStartAndGoalPositions(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
                if(plannerSetup == null)
                {
                    return;
                }

                var startConfiguration = plannerSetup.StartConfiguration;
                var startPosition = plannerSetup.StartPosition;
                var goalConfiguration = plannerSetup.GoalConfiguration;
                var goalPosition = plannerSetup.GoalPosition;

                if(startConfiguration != null)
                {
                    context.DrawRobotConfiguration(
                        startConfiguration,
                        RobotConfigurationBrush,
                        RobotConfigurationPen,
                        GeometricShape.X,
                        Settings.Default.DrawingStartVertexRadius);
                }

                if(startPosition != null)
                {
                    DrawVertex(context, startPosition, VertexType.Start);
                }
                
                if(goalConfiguration != null)
                {
                    context.DrawRobotConfiguration(
                        goalConfiguration,
                        RobotConfigurationBrush,
                        RobotConfigurationPen,
                        GeometricShape.X,
                        Settings.Default.DrawingStartVertexRadius);
                }

                if(goalPosition != null)
                {
                    DrawVertex(context, goalPosition, VertexType.Goal);
                }
            }
        }

        private void DrawCSpace(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
                if(plannerSetup == null)
                {
                    return;
                }

                var cspace = plannerSetup.ConfigurationSpace;
                if(cspace == null)
                {
                    return;
                }

                context.DrawGeometry(cspace.Obstacles, CSpaceGeometryBrush, GeometryOutlinePen);
                DrawLayer(MapLayers.VertexLayer);
            }
        }

        private void DrawWorkspace(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
                if(plannerSetup == null)
                {
                    return;
                }
                var workspace = plannerSetup.Workspace;
                if(workspace == null)
                {
                    return;
                }

                context.DrawGeometry(workspace.Obstacles, WorkspaceGeometryBrush, GeometryOutlinePen);
                DrawLayer(MapLayers.VertexLayer);
            }
        }

        public void DrawVertices(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
                if(plannerSetup == null)
                {
                    return;
                }

                var cspace = plannerSetup.ConfigurationSpace;
                var workspace = plannerSetup.Workspace;

                // Get CSpace/Workspace
                if(cspace != null)
                {
                    DrawVertexRange(context, cspace.OccupiedSpace.GetVertices(), VertexType.Geometry);
                }

                if(workspace != null && RenderState == MapRenderState.Initial)
                {
                    DrawVertexRange(context, workspace.OccupiedSpace.GetVertices(), VertexType.Geometry);
                }      
            }
        }

        private void DrawDecomposedPolygons(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var plannerSetup = ServiceLocator.PathPlannerSetup.CurrentSetup;
                if(plannerSetup == null)
                {
                    return;
                }

                if(plannerSetup.DecomposedObstaclePolygons != null)
                {
                    context.DrawGeometry(plannerSetup.DecomposedObstaclePolygons, DecomposedObstaclesBrush, DecomposedObstaclesPen);
                }
            }
        }

        private void DrawGraphEdges(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var pathPlanner = ServiceLocator.PathPlannerManager.PathPlanner;
                if(RenderState != MapRenderState.Graph || pathPlanner == null)
                {
                    return;
                }

                var roadmap = pathPlanner.Graph;
                foreach(var edge in roadmap.Edges)
                {
                    context.DrawLine(GraphEdgePen, edge.Source.ToWPFPoint(), edge.Target.ToWPFPoint());
                }
            }
        }

        private void DrawGraphVertices(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                if(RenderState != MapRenderState.Graph)
                {
                    return;
                }

                var pathPlannerManager = ServiceLocator.PathPlannerManager;
                var planner = pathPlannerManager.PathPlanner;
                if(planner == null)
                {
                    return;
                }
                DrawVertexRange(context, planner.Graph.Vertices, VertexType.Graph);
            }            
        }

        private void DrawShortestPath(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var pathPlannerManager = ServiceLocator.PathPlannerManager;
                var planner = pathPlannerManager.PathPlanner;
                if(RenderState != MapRenderState.Graph || planner == null || planner.State != PathPlannerAlgorithmState.ShortestPathFound)
                {
                    return;
                }

                foreach(var edge in planner.ShortestPath)
                {
                    context.DrawLine(GraphShortestPathEdgePen, edge.Source.ToWPFPoint(), edge.Target.ToWPFPoint());
                }
                var shortestPathVertices = planner.ShortestPath.GetEdgeVertices();
                DrawVertexRange(context, shortestPathVertices, VertexType.ShortestPath);
            }        
        }
        
        private void DrawRobotConfigurations(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var robot = ServiceLocator.RobotManager.CurrentRobot;
                var workspace = ServiceLocator.PathPlannerSetup.CurrentSetup.Workspace;
                if(robot == null)
                {
                    return;
                }

                foreach(var obstacle in workspace.Obstacles)
                {
                    var vertices = obstacle.GetVertices();
                    foreach(var vertex in vertices)
                    {
                        var config = robot.GetConfiguration(vertex);
                        context.DrawRobotConfiguration(config, Brushes.Transparent, new Pen(Brushes.Black, 1.0), GeometricShape.X, 5.0 / ZoomScale);
                    }
                }
            }            
        }

        private void DrawPathPlanner(DrawingVisual visual)
        {
            using(var context = visual.RenderOpen())
            {
                var pathPlanner = ServiceLocator.PathPlannerManager.PathPlanner;
                if(RenderState != MapRenderState.Graph || pathPlanner == null)
                {
                    return;
                }

                switch(pathPlanner.AlgorithmType)
                {
                    case PathPlannerAlgorithmType.Triangulation:
                        DrawDelaunayTriangulation(context, (TriangulationPathPlanner) pathPlanner);
                        break;
                    case PathPlannerAlgorithmType.GeneralizedVoronoiDiagram:
                        DrawGVD(context, (GeneralizedVoronoiDiagramPlanner) pathPlanner);
                        break;
                    case PathPlannerAlgorithmType.TrapezoidalDecomposition:
                        DrawTrapezoidalDecompositionEdges(context, (TrapezoidalDecompositionPlanner) pathPlanner);
                        break;
                }
            }      
        }

        private void DrawGVD(DrawingContext context, GeneralizedVoronoiDiagramPlanner planner)
        {
            DrawVertexRange(context, planner.VoronoiSites, VertexType.PathPlanner);
            foreach(var cell in planner.QuadEdgeSubdivision.GetVoronoiCellPolygons(planner.CSpace.GeometryFactory))
            {
                context.DrawGeometry(cell, Brushes.Transparent, new Pen(Brushes.SteelBlue, 1.0));
            }
        }

        private void DrawDelaunayTriangulation(DrawingContext context, TriangulationPathPlanner planner)
        {
            // Draw Sleeve
            foreach(var triangle in planner.ValidTriangles.Select(t => t.GetGeometry((GeometryFactory)planner.GeometryFactory)))
            {
                context.DrawGeometry(triangle, Brushes.Transparent, PlannerTriangulationValidTrianglePen);
                DrawVertexRange(context, triangle.GetVertices(), VertexType.PathPlanner);
            }
        }

        public void DrawTrapezoidalDecompositionEdges(DrawingContext context, TrapezoidalDecompositionPlanner planner)
        {
            foreach(var segment in planner.CellEdges)
            {
                var line = (ILineString) segment.Value;
                context.DrawLine(new Pen(Brushes.Blue, 2.0), line.Coordinates[0].ToWPFPoint(), line.Coordinates.Last().ToWPFPoint());
                foreach(var vertex in line.Coordinates)
                {
                    DrawVertex(context, vertex, VertexType.PathPlanner);    
                }

            }

            //foreach(var cell in planner.Cells)
            //{
            //    context.DrawGeometry(cell, Brushes.Transparent, new Pen(Brushes.Olive, 1.0));
            //}
        }

        #endregion Draw Methods

        #region Value Changed

        protected override void OnZoomScaleChanged(double newScale)
        {
            RescaleResources();
        }

        protected override void OnContentScaleChanged(object source, EventArgs args)
        {
            RescaleResources();
        }

        protected virtual void OnRenderStateChanged(MapRenderState newValue)
        {
            if(RenderState == newValue)
            {
                return;
            }

            _renderState = newValue;

            Clear();
            Draw();

            //GetLayer(MapLayerId.Tool).IsVisible = false;
            //GetLayer(MapLayerId.Bounds).IsVisible = false;
            //GetLayer(MapLayerId.GraphShortestPath).IsVisible = false;
            //GetLayer(MapLayerId.GraphVertices).IsVisible = false;
            //GetLayer(MapLayerId.GraphEdges).IsVisible = false;
            //GetLayer(MapLayerId.StartPosition).IsVisible = false;
            //GetLayer(MapLayerId.GoalPosition).IsVisible = false;
            //GetLayer(MapLayerId.PathPlanner).IsVisible = false;
            //GetLayer(MapLayerId.DecomposedPolygonsLayer).IsVisible = false;
            //GetLayer(MapLayerId.Workspace).IsVisible = false;
            //GetLayer(MapLayerId.CSpace).IsVisible = false;

            //if(RenderState == MapRenderState.Initial)
            //{
            //    GetLayer(MapLayerId.CSpace).IsVisible = false;
            //    GetLayer(MapLayerId.GraphShortestPath).IsVisible = false;
            //    GetLayer(MapLayerId.GraphVertices).IsVisible = false;
            //    GetLayer(MapLayerId.GraphEdges).IsVisible = false;
            //    GetLayer(MapLayerId.PathPlanner).IsVisible = false;
            //}
            //else if(RenderState == MapRenderState.CSpace)
            //{
            //    GetLayer(MapLayerId.Workspace).IsVisible = false;
            //    GetLayer(MapLayerId.GraphShortestPath).IsVisible = false;
            //    GetLayer(MapLayerId.GraphVertices).IsVisible = false;
            //    GetLayer(MapLayerId.GraphEdges).IsVisible = false;
            //    GetLayer(MapLayerId.PathPlanner).IsVisible = false;
            //}
            if(RenderState == MapRenderState.Graph)
            {
                //GetLayer(MapLayers.WorkspaceLayer).IsVisible = false;
                //GetLayer(MapLayers.GraphShortestPathLayer).IsVisible = false;
                //GetLayer(MapLayers.DecomposedPolygonsLayer).IsVisible = false;
                //GetLayer(MapLayers.PathPlannerLayer).IsVisible = true;
            }
            //else if(RenderState == MapRenderState.ShortestPath)
            //{
            //    GetLayer(MapLayerId.Workspace).IsVisible = false;
            //    GetLayer(MapLayerId.DecomposedPolygonsLayer).IsVisible = false;
            //    GetLayer(MapLayerId.PathPlanner).IsVisible = false;
            //}
        }

        #endregion Value Changed

        #endregion Internal Methods
    }
}