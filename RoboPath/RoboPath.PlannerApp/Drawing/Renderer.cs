//// *******************************************************
//// Project: RoboPath.PlannerApp
//// File Name: Renderer.cs
//// By: Frank Perks
//// *******************************************************

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;

//using NetTopologySuite.Geometries;
//using NetTopologySuite.Windows.Media;

//using RoboPath.Core;
//using RoboPath.Core.Geometry;
//using RoboPath.Core.Graph;
//using RoboPath.Core.PathPlanning;
//using RoboPath.Core.PathPlanning.Geometric;
//using RoboPath.Core.Robot;
//using RoboPath.Core.Robot.Geometric;
//using RoboPath.PlannerApp.PathPlanning.Robot;
//using RoboPath.PlannerApp.Properties;
//using RoboPath.PlannerApp.Tools;
//using RoboPath.PlannerApp.Utility;
//using RoboPath.UI;
//using RoboPath.UI.Controls;

//using WPFPoint = System.Windows.Point;

//using GeoAPI.Geometries;

//using NLog;

//using RoboPath.PlannerApp.PathPlanning;
//using RoboPath.UI.Drawing;

//using Brush = System.Windows.Media.Brush;
//using Pen = System.Windows.Media.Pen;

//namespace RoboPath.PlannerApp.Drawing
//{
//    public class Renderer : IRenderer
//    {
//        #region Fields

//        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

//        private ZoomAndPanControl _zoomControl;
//        private PathPlannerSetup _plannerSetup;
//        private RenderState _renderState;
        
//        #endregion Fields

//        #region Properties

//        #region Brushes

//        public static Pen SelectionRegionPen { get; set; }

//        public static Pen GeometryOutlinePen { get; set; }
//        public static Pen BoundaryPen { get; set; }
//        public static Pen RobotConfigurationPen { get; set; }
//        public static Pen DecomposedObstaclesPen { get; set; }
//        public static Pen GraphEdgePen { get; set; }
//        public static Pen GraphVertexPen { get; set; }
//        public static Pen GraphShortestPathEdgePen { get; set; }
        
//        public static Brush RobotConfigurationBrush { get; set; }

//        public static Brush WorkspaceGeometryBrush { get; set; }
//        public static Brush WorkspaceVertexBrush { get; set; }

//        public static Brush CSpaceGeometryBrush { get; set; }
//        public static Brush CSpaceVertexBrush { get; set; }

//        public static Brush StartPositionBrush { get; set; }
//        public static Brush GoalPositionBrush { get; set; }

//        public static Brush ToolDefinePolygonGeometryBrush { get; set; }
//        public static Brush ToolDefinePolygonVertexBrush { get; set; }

//        public static Brush SelectionRegionBrush { get; set; }
//        public static Brush DecomposedObstaclesBrush { get; set; }

//        public static Brush GraphVertexBrush { get; set; }
//        public static Brush GraphShortestPathVertexBrush { get; set; }

//        public static Brush PlannerTriangulationInvalidTriangleBrush { get; set; }
//        public static Brush PlannerTriangulationValidTriangleBrush { get; set; }

//        public static Pen PlannerTriangulationInvalidTrianglePen { get; set; }
//        public static Pen PlannerTriangulationValidTrianglePen { get; set; }

//        #endregion Brushes

//        public const double MinZoomRegionArea = 10.0;
//        public const double MinPenThickness = 0.25;

//        public RenderState RenderState 
//        {
//            get { return _renderState; }
//            set { OnRenderStateChanged(value); }
//        }

//        public DrawingLayerHost LayerHost { get; private set; }
        
//        public ZoomAndPanControl ZoomControl
//        {
//            get { return _zoomControl; }
//            set { _zoomControl = value; }
//        }
        
//        public double ZoomScale
//        {
//            get
//            {
//                if(ZoomControl == null)
//                {
//                    return 1.0;
//                }
//                return ZoomControl.ContentScale;
//            }
//            set { OnZoomScaleChanged(value); }
//        }

//        public PathPlannerSetup PlannerSetup
//        {
//            get { return _plannerSetup; }
//            set { OnPathPlannerChanged(value); }
//        }

//        public IPathPlanner PathPlanner
//        {
//            get { return ServiceManager.Instance.GetService<IPathPlannerManager>().PathPlanner; }
//        }

//        public PropertyChangedEventFilter<bool> IsRobotConfigurationVisibleFilter { get; set; }   

//        #endregion Properties

//        #region Public Methods

//        public Renderer(DrawingLayerHost layerHost, ZoomAndPanControl zoomControl)
//        {
//            _renderState = RenderState.Setup;
//            LayerHost = layerHost;
//            ZoomControl = zoomControl;

//            IsRobotConfigurationVisibleFilter = new PropertyChangedEventFilter<bool>(Settings.Default, "DrawingShowConfigurationAtVertices", b =>
//                                                                                                                                                 {
//                                                                                                                                                     Draw();
//                                                                                                                                                 });

//            LoadDrawingResources();
//            RescalePenResources();

//            // Add Default Layers
//            LayerHost.AddLayerRange(new List<DrawingLayer<LayerTypeId>>
//                                        {
//                                            CreateLayer(LayerTypeId.Tool, DrawTool),
//                                            CreateLayer(LayerTypeId.Bounds, DrawBounds),

//                                            CreateLayer(LayerTypeId.StartPosition, DrawStartPosition),
//                                            CreateLayer(LayerTypeId.GoalPosition, DrawGoalPosition),

//                                            CreateLayer(LayerTypeId.GraphShortestPath, DrawNothing),
//                                            CreateLayer(LayerTypeId.GraphVertices, DrawGraphVertices),
//                                            CreateLayer(LayerTypeId.GraphEdges, DrawGraphEdges),
//                                            CreateLayer(LayerTypeId.PathPlanner, DrawPathPlanner),
//                                            CreateLayer(LayerTypeId.DecomposedPolygonsLayer, DrawDecomposedPolygons),

//                                            CreateLayer(LayerTypeId.Workspace, DrawWorkspace),
//                                            CreateLayer(LayerTypeId.CSpace, DrawCSpace),                                            
//                                        }
//                );

//            ZoomControl.ContentScaleChanged += OnZoomScaleChanged;
//            //ServiceManager.Instance.GetService<IPathPlannerManager>().PathPlannerChanged += (sender, args) =>
//            //                                                                                    {
//            //                                                                                        PathPlanner = ServiceManager.Instance.GetService<IPathPlannerManager>().PathPlanner;
//            //                                                                                    };
//        }

//        #region Layer Helper Methods

//        /// <summary>
//        /// Draw all of the layers
//        /// </summary>
//        public void Draw()
//        {
//            foreach(var layer in LayerHost.Layers)
//            {
//                layer.Draw();
//            }
//        }

//        /// <summary>
//        /// Draw all the layers with the specified Ids
//        /// </summary>
//        /// <param name="ids">An Enumerable of Layer Ids.</param>
//        public void DrawLayerRange(IEnumerable<LayerTypeId> ids)
//        {
//            foreach(var layerId in ids)
//            {
//                DrawLayer(layerId);    
//            }
//        }

//        /// <summary>
//        /// Draw a specific layer, specified by its id
//        /// </summary>
//        /// <param name="id">The Layer Id of a specific layer.</param>
//        public void DrawLayer(LayerTypeId id)
//        {
//            var layer = LayerHost.GetLayer(id);
//            if(!layer.IsVisible)
//            {
//                Log.Warn("Attempting to Draw Hidden Layer [ {0} ]", layer);
//                return;
//            }

//            layer.Draw();
//        }

//        /// <summary>
//        /// Retrieve a specific layer
//        /// </summary>
//        /// <param name="id">The Layer Id of a specific layer.</param>
//        /// <returns>The drawing layer</returns>
//        public DrawingLayer<LayerTypeId> GetLayer(LayerTypeId id)
//        {
//            return LayerHost.GetLayer(id);
//        }


//        /// <summary>
//        /// Clear all of the layers
//        /// </summary>
//        public void Clear()
//        {
//            foreach(var layer in LayerHost.Layers)
//            {
//                layer.Clear();
//            }
//        }

//        /// <summary>
//        /// Clears a layer by opening the rendercontext and closing it without writing anything
//        /// </summary>
//        /// <param name="id">The Layer Id of a specific layer.</param>
//        public void ClearLayer(LayerTypeId id)
//        {
//            var layer = LayerHost.GetLayer(id);
//            layer.Clear();
//        }

//        /// <summary>
//        /// Hides a layer by setting its visiability to false
//        /// </summary>
//        /// <param name="id">The Layer Id of a specific layer.</param>
//        public void HideLayer(LayerTypeId id)
//        {
//            var layer = LayerHost.GetLayer(id);
//            layer.IsVisible = false;
//        }

//        /// <summary>
//        /// Show a layer, by setting its visibility to true
//        /// </summary>
//        /// <param name="id">The Layer Id of a specific layer.</param>
//        public void ShowLayer(LayerTypeId id)
//        {
//            var layer = LayerHost.GetLayer(id);
//            layer.IsVisible = true;
//        }

//        /// <summary>
//        /// Enable visibility for all the layers.
//        /// </summary>
//        public void ShowAllLayers()
//        {
//            foreach(var layer in LayerHost.Layers)
//            {
//                layer.IsVisible = true;
//            }
//        }
//        #endregion Layer Helper Methods

//        #region Zooming Methods

//        public void ZoomToRegion(Envelope envelope)
//        {
//            if(envelope.Area < MinZoomRegionArea)
//            {
//                Log.Debug("Envelope too Small for Zooming");
//                return;
//            }
//            ZoomControl.AnimatedZoomTo(envelope.ToWPFRect());
//        }

//        public void ZoomToRegion(Coordinate p1, Coordinate p2)
//        {
//            var envelope = new Envelope(p1, p2);
//            ZoomToRegion(envelope);
//        }

//        #endregion Zooming Methods

//        #region Drawing Helper Methods

//        public void DrawCoordinates(DrawingContext context, IEnumerable<Coordinate> coordinates, Brush brush, Pen pen, GeometricShape shapeType, double size)
//        {
//            context.DrawCoordinates(coordinates, brush, pen, shapeType, size / ZoomScale);
//        }

//        public void DrawCoordinate(DrawingContext context, Coordinate coordinate, Brush brush, Pen pen, GeometricShape shapeType, double size)
//        {
//            context.DrawCoordinate(coordinate, brush, pen, shapeType, size / ZoomScale);
//        }

//        public void DrawGeometryVertices(DrawingContext context, IGeometry geometry, Brush brush, Pen pen, GeometricShape shapeType, double size)
//        {
//            context.DrawGeometryVertices(geometry, brush, pen, shapeType, size / ZoomScale);
//        }

//        #endregion Drawing Helper Methods

//        public BitmapSource ToBitmap()
//        {
//            Log.Debug("Generating Bitmap");
//            if(PlannerSetup == null)
//            {
//                throw new DrawingOperationException("Cannot create bitmap for renderer");
//            }

//            // Write it to the bitmaps
//            Draw();

//            var bitmap = new RenderTargetBitmap((int)PlannerSetup.Bounds.Width, (int)PlannerSetup.Bounds.Height, 96, 96, PixelFormats.Pbgra32);

//            // Grab the visibile layers
//            foreach(var visual in from layer in LayerHost.Layers
//                                      orderby layer.Id descending 
//                                      select layer.Visual)
//            {
//                bitmap.Render(visual);
//            }
//            return bitmap;
//        }

//        #endregion Public Methods

//        #region Internal Methods



//        #region Resource Loading

//        private T GetDrawingResource<T>(string key)
//        {
//            return (T) Application.Current.Resources[key];
//        }

//        private void LoadDrawingResources()
//        {
//            Log.Debug("Loading Drawing Resources");

//            // Pens
//            GeometryOutlinePen = GetDrawingResource<Pen>("GeometryOutlinePen");
//            BoundaryPen = GetDrawingResource<Pen>("BoundaryPen");
//            RobotConfigurationPen = GetDrawingResource<Pen>("RobotConfigurationPen");
//            SelectionRegionPen = GetDrawingResource<Pen>("SelectionRegionPen");
//            DecomposedObstaclesPen = GetDrawingResource<Pen>("DecomposedObstaclesPen");
//            GraphEdgePen = GetDrawingResource<Pen>("GraphEdgePen");
//            GraphVertexPen = GetDrawingResource<Pen>("GraphVertexPen");

//            GraphShortestPathEdgePen = GetDrawingResource<Pen>("GraphShortestPathEdgePen");



//            // Brushes
//            RobotConfigurationBrush = GetDrawingResource<SolidColorBrush>("RobotConfigurationBrush");

//            WorkspaceGeometryBrush = GetDrawingResource<SolidColorBrush>("WorkspaceGeometryBrush");
//            WorkspaceVertexBrush = GetDrawingResource<SolidColorBrush>("WorkspaceVertexBrush");

//            CSpaceGeometryBrush = GetDrawingResource<SolidColorBrush>("CSpaceGeometryBrush");
//            CSpaceVertexBrush = GetDrawingResource<SolidColorBrush>("CSpaceVertexBrush");

//            StartPositionBrush = GetDrawingResource<RadialGradientBrush>("StartPositionBrush");
//            GoalPositionBrush = GetDrawingResource<RadialGradientBrush>("GoalPositionBrush");

//            ToolDefinePolygonGeometryBrush = GetDrawingResource<SolidColorBrush>("ToolDefinePolygonGeometryBrush");
//            ToolDefinePolygonVertexBrush = GetDrawingResource<SolidColorBrush>("ToolDefinePolygonVertexBrush");

//            SelectionRegionBrush = GetDrawingResource<SolidColorBrush>("SelectionRegionBrush");
//            DecomposedObstaclesBrush = GetDrawingResource<SolidColorBrush>("DecomposedObstaclesBrush");

//            GraphVertexBrush = GetDrawingResource<RadialGradientBrush>("GraphVertexBrush");
//            GraphShortestPathVertexBrush = GetDrawingResource<RadialGradientBrush>("GraphShortestPathVertexBrush");

//            PlannerTriangulationInvalidTriangleBrush = GetDrawingResource<SolidColorBrush>("PlannerTriangulationInvalidTriangleBrush");
//            PlannerTriangulationValidTriangleBrush = GetDrawingResource<SolidColorBrush>("PlannerTriangulationValidTriangleBrush");

//            PlannerTriangulationInvalidTrianglePen = GetDrawingResource<Pen>("PlannerTriangulationInvalidTrianglePen");
//            PlannerTriangulationValidTrianglePen = GetDrawingResource<Pen>("PlannerTriangulationValidTrianglePen");

//            RescalePenResources();
//        }

//        private Pen RescalePen(Pen pen)
//        {
//            var thickness = pen.Thickness / ZoomScale;
//            if(thickness < MinPenThickness)
//            {
//                thickness = MinPenThickness;
//            }

//            if(thickness > 1.0)
//            {
//                thickness = 1.0;
//            }

//            return new Pen(pen.Brush, thickness);
//        }

//        private void RescalePenResources()
//        {
//            GeometryOutlinePen = RescalePen(GeometryOutlinePen);
//            BoundaryPen = RescalePen(BoundaryPen);
//            RobotConfigurationPen = RescalePen(RobotConfigurationPen);
//            SelectionRegionPen = RescalePen(SelectionRegionPen);
//        }

//        #endregion Resource Loading

//        private static DrawingLayer<LayerTypeId> CreateLayer(LayerTypeId id, Action<DrawingVisual> callback)
//        {
//            return new DrawingLayer<LayerTypeId>(id, (int)id, new DrawingVisual(), callback);
//        }

//        #region Drawing Methods

//        private void DrawNothing(DrawingVisual visual)
//        {
//            // Do nothing
//        }

//        private void DrawTool(DrawingVisual visual)
//        {
//            if(PlannerSetup == null)
//            {
//                return;
//            }

//            var toolManager = ServiceManager.Instance.GetService<IToolManager>();
//            toolManager.Draw(visual);
//        }

//        private void DrawBounds(DrawingVisual visual)
//        {
//            if(PlannerSetup == null)
//            {
//                return;
//            }

//            using(var context = visual.RenderOpen())
//            {
//                context.DrawEnvelope(PlannerSetup.Bounds, BoundaryPen);
//            }
//        }

//        private void DrawStartPosition(DrawingVisual visual)
//        {
//            if(PlannerSetup == null)
//            {
//                return;
//            }

//            var startConfiguration = PlannerSetup.StartConfiguration;
//            var startPosition = PlannerSetup.StartPosition;

//            using(var context = visual.RenderOpen())
//            {
//                if(startConfiguration != null)
//                {
//                    context.DrawRobotConfiguration(
//                        startConfiguration,
//                        RobotConfigurationBrush,
//                        RobotConfigurationPen,
//                        GeometricShape.X,
//                        Settings.Default.DrawingStartVertexRadius);
//                }

//                if(startPosition != null)
//                {
//                    context.DrawVertexAsEllipse(startPosition, StartPositionBrush, GeometryOutlinePen, Settings.Default.DrawingStartVertexRadius / ZoomScale);

//                    //DrawCoordinate(
//                    //    context,
//                    //    startPosition,
//                    //    StartPositionBrush,
//                    //    GeometryOutlinePen,
//                    //    GeometricShape.X,
//                    //    Settings.Default.DrawingStartVertexRadius);
//                }                
//            }
//        }

//        private void DrawGoalPosition(DrawingVisual visual)
//        {
//            if(PlannerSetup == null)
//            {
//                return;
//            }

//            var goalConfiguration = PlannerSetup.GoalConfiguration;
//            var goalPosition = PlannerSetup.GoalPosition;

//            using(var context = visual.RenderOpen())
//            {
//                if(goalConfiguration != null)
//                {
//                    context.DrawRobotConfiguration(                        
//                        goalConfiguration,
//                        RobotConfigurationBrush,
//                        RobotConfigurationPen,
//                        GeometricShape.X,
//                        Settings.Default.DrawingGoalVertexRadius);
//                }

//                if(goalPosition != null)
//                {
//                    context.DrawVertexAsEllipse(goalPosition, GoalPositionBrush, GeometryOutlinePen, Settings.Default.DrawingStartVertexRadius / ZoomScale);
//                }
//            }
//        }

//        private void DrawCSpace(DrawingVisual visual)
//        {
//            if(PlannerSetup == null)
//            {
//                return;
//            }

//            var cspace = PlannerSetup.ConfigurationSpace;
//            using(var context = visual.RenderOpen())
//            {
//                if(cspace == null || PlannerSetup.Robot.BodyType == RobotBodyType.Point)
//                {
//                    return;
//                }

//                // Draw CSpace Geometry
//                context.DrawGeometry(cspace.OccupiedSpace, CSpaceGeometryBrush, GeometryOutlinePen);
//                if(Settings.Default.IsGeometryVerticesVisible)
//                {
//                    // Draw Vertices
//                    DrawGeometryVertices(
//                        context,
//                        cspace.OccupiedSpace,
//                        CSpaceVertexBrush,
//                        GeometryOutlinePen,
//                        GeometricShape.Square,
//                        Settings.Default.DrawingVertexRadius);
//                }
//            }
//        }

//        private void DrawWorkspace(DrawingVisual visual)
//        {
//            if(PlannerSetup == null)
//            {
//                return;
//            }

//            var workspace = PlannerSetup.Workspace;
//            using(var context = visual.RenderOpen())
//            {
//                // Inside RenderOpen to clear it
//                if(workspace == null)
//                {
//                    return;
//                }

//                // Draw Geometry
//                context.DrawGeometry(workspace.OccupiedSpace, WorkspaceGeometryBrush, GeometryOutlinePen);

//                if(Settings.Default.IsGeometryVerticesVisible)
//                {
//                    // Draw Vertices
//                    DrawGeometryVertices(
//                        context,
//                        workspace.OccupiedSpace,
//                        WorkspaceVertexBrush,
//                        GeometryOutlinePen,
//                        GeometricShape.Square,
//                        Settings.Default.DrawingVertexRadius);
//                }

//                if(Settings.Default.DrawingShowConfigurationAtVertices)
//                {
//                    var robot = ServiceManager.Instance.GetService<IRobotManager>().CurrentRobot;
//                    if(robot == null)
//                    {
//                        return;
//                    }

//                    foreach(var obstacle in workspace.Obstacles)
//                    {
//                        var vertices = obstacle.GetVertices();
//                        foreach(var vertex in vertices)
//                        {
//                            var config = robot.GetConfiguration(vertex);
//                            context.DrawRobotConfiguration(config, Brushes.Transparent, new Pen(Brushes.Black, 1.0), GeometricShape.X, 5.0 / ZoomScale);
//                        }
//                    }
//                }
//            }
//        }

//        private void DrawDecomposedPolygons(DrawingVisual visual)
//        {
//            using(var context = visual.RenderOpen())
//            {
//                if(!Settings.Default.IsObstacleDecompositionVisible || PlannerSetup == null || PlannerSetup.DecomposedObstaclePolygons == null)
//                {
//                    return;
//                }

//                foreach(var polygon in PlannerSetup.DecomposedObstaclePolygons)
//                {
//                    context.DrawGeometry(polygon, DecomposedObstaclesBrush, DecomposedObstaclesPen);
//                }
//            }
//        }

//        private void DrawGraphEdges(DrawingVisual visual)
//        {
//            using(var context = visual.RenderOpen())
//            {
//                if(PathPlanner == null || RenderState == RenderState.Setup)
//                {
//                    return;
//                }    

//                var roadmap = PathPlanner.Graph;
//                foreach(var edge in roadmap.Edges)
//                {
//                    context.DrawLine(GraphEdgePen, edge.Source.ToWPFPoint(), edge.Target.ToWPFPoint());
//                }

//                if(PathPlanner.State == PathPlannerAlgorithmState.ShortestPathFound)
//                {
//                    foreach(var edge in PathPlanner.ShortestPath)
//                    {
//                        context.DrawLine(GraphShortestPathEdgePen, edge.Source.ToWPFPoint(), edge.Target.ToWPFPoint());
//                    }
//                }
//            }
//        }

//        private void DrawGraphVertices(DrawingVisual visual)
//        {
//            using(var context = visual.RenderOpen())
//            {
//                if(PathPlanner == null || RenderState == RenderState.Setup)
//                {
//                    return;
//                }

//                var roadmap = PathPlanner.Graph;

//                context.DrawVertexAsEllipse(PathPlanner.StartVertex, StartPositionBrush, GraphVertexPen, Settings.Default.DrawingSpecialVertexRadius / ZoomScale);
//                context.DrawVertexAsEllipse(PathPlanner.GoalVertex, GoalPositionBrush, GraphVertexPen, Settings.Default.DrawingSpecialVertexRadius / ZoomScale);

//                context.DrawVerticesAsEllipses(from vertex in roadmap.Vertices
//                                               where !vertex.Equals2D(PathPlanner.GoalVertex) && !vertex.Equals2D(PathPlanner.StartVertex)
//                                               select vertex, 
//                                               GraphVertexBrush, 
//                                               GraphVertexPen,
//                                               Settings.Default.DrawingVertexRadius / ZoomScale);

//                if(PathPlanner.State == PathPlannerAlgorithmState.ShortestPathFound)
//                {
//                    var vertices = PathPlanner.ShortestPath.GetEdgeVertices();
//                    context.DrawVerticesAsEllipses(from vertex in vertices
//                                                   where !vertex.Equals2D(PathPlanner.GoalVertex) && !vertex.Equals2D(PathPlanner.StartVertex)
//                                                   select vertex,
//                                                   GraphShortestPathVertexBrush,
//                                                   GraphVertexPen,
//                                                   Settings.Default.DrawingVertexRadius / ZoomScale);
//                }

//                //var geometry = new GeometryGroup();



//                //foreach(var vertex in roadmap.Vertices)
//                //{
//                //    geometry.Children.Add(new EllipseGeometry(vertex.ToWPFPoint(), Settings.Default.DrawingVertexRadius, Settings.Default.DrawingVertexRadius));    
//                //}
//                //context.DrawGeometry(GraphVertexBrush, GraphVertexPen, geometry);

//                ////geometry.Children.Add(new EllipseGeometry());
//                ////context.DrawCoordinates(roadmap.Vertices, GraphVertexBrush, GraphVertexPen, GeometricShape.Circle, Settings.Default.DrawingVertexRadius);

//                //if(PathPlanner.State == PathPlannerAlgorithmState.ShortestPathFound)
//                //{
//                //    var vertices = PathPlanner.ShortestPath.GetEdgeVertices();
//                //    //context.DrawCoordinates(vertices, GraphShortestPathVertexBrush, GraphVertexPen, GeometricShape.Circle, Settings.Default.DrawingShortestPathVertexRadius);
//                //}
//            }


//        }

//        private void DrawPathPlanner(DrawingVisual visual)
//        {
//            using(var context = visual.RenderOpen())
//            {
//                if(PathPlanner == null || RenderState == RenderState.Setup)
//                {
//                    return;
//                }

//                switch(PathPlanner.AlgorithmType)
//                {
//                    case PathPlannerAlgorithmType.Triangulation:
//                        DrawPathPlannerTriangulation(context, (TriangulationPathPlanner) PathPlanner);
//                        return;

//                    case PathPlannerAlgorithmType.GeneralizedVoronoiDiagram:
//                        DrawGVD(context, (GeneralizedVoronoiDiagramPlanner) PathPlanner);
//                        return;
//                    default:
//                        // Nothing special
//                        return;
//                }
//            }
//        }

//        private void DrawGVD(DrawingContext context, GeneralizedVoronoiDiagramPlanner planner)
//        {
//            var zoomScale = Math.Min(5.0 / ZoomScale, 5.0);

//            context.DrawVerticesAsEllipses(planner.VoronoiSites, Brushes.DarkRed, new Pen(Brushes.Transparent, 0.0), 3.0 / ZoomScale);

//            foreach(var cell in planner.QuadEdgeSubdivision.GetVoronoiCellPolygons(planner.CSpace.GeometryFactory))
//            {
//                context.DrawGeometry(cell, Brushes.Transparent, new Pen(Brushes.SteelBlue, 1.0));
//            }

//         //   context.DrawGeometry((IGeometry)planner.Voronoi.GetDiagram(planner.CSpace.GeometryFactory), Brushes.Transparent, PlannerTriangulationValidTrianglePen);
//        }

//        private void DrawPathPlannerTriangulation(DrawingContext context, TriangulationPathPlanner planner)
//        {

//            //var geometry = (IGeometry) planner.Triangulation.QuadEdgeSubdivision.GetTriangles(planner.GeometryFactory);
//            //context.DrawGeometry(geometry, Brushes.Transparent, new Pen(Brushes.Red, 1.0));

//            //foreach(var polygon in geometry.GetPolygons())
//            //{
//            //    Log.Debug("Polygon [Centroid={0}, Area={1}]", polygon.Centroid, polygon.Area);
//            //}
            
//            // Draw Invalid
//            //foreach(var triangle in planner.InvalidTriangles)
//            //{
//            //    context.DrawGeometry(triangle, PlannerTriangulationInvalidTriangleBrush, PlannerTriangulationInvalidTrianglePen);
//            //}

//            // Draw Valid
//            foreach(var triangle in planner.ValidTriangles.Select(t => t.GetGeometry((GeometryFactory)planner.GeometryFactory)))
//            {
//                context.DrawGeometry(triangle, Brushes.Transparent, PlannerTriangulationValidTrianglePen);
//                //context.DrawGeometry(triangle, PlannerTriangulationValidTriangleBrush, PlannerTriangulationValidTrianglePen);
//            }
//        }

//        #endregion Drawing Methods

//        #region Value Changed Methods

//        private void OnZoomScaleChanged(object source, EventArgs args)
//        {
//            RescalePenResources();
//            Draw();
//        }

//        protected virtual void OnRenderStateChanged(RenderState newState)
//        {
//            if(RenderState == newState)
//            {
//                Draw();
//                return;
//            }

//            _renderState = newState;

//            ClearLayer(LayerTypeId.GraphVertices);
//            ClearLayer(LayerTypeId.GraphEdges);
//            ClearLayer(LayerTypeId.PathPlanner);
            
//            if(RenderState == RenderState.Graph || RenderState == RenderState.Solved)
//            {
//                LayerHost.GetLayer(LayerTypeId.GraphEdges).IsVisible = true;
//                LayerHost.GetLayer(LayerTypeId.GraphVertices).IsVisible = true;
//                LayerHost.GetLayer(LayerTypeId.PathPlanner).IsVisible = true;
//            }
//            else
//            {
//                LayerHost.GetLayer(LayerTypeId.GraphEdges).IsVisible = false;
//                LayerHost.GetLayer(LayerTypeId.GraphVertices).IsVisible = false;
//                LayerHost.GetLayer(LayerTypeId.PathPlanner).IsVisible = false;
//            }
            
//            Draw();
//        }

//        protected virtual void OnZoomScaleChanged(double newScale)
//        {
//            Log.Debug("Zoom Scale Changed from [ {0} -> {1} ]", ZoomControl.ContentScale, newScale);
//            ZoomControl.ContentScale = newScale;
//        }

//        protected virtual void OnPathPlannerChanged(PathPlannerSetup newPlannerSetup)
//        {
//            Log.Debug("Planner Setup Changed");
//            _plannerSetup = newPlannerSetup;
//        }

//        #endregion Value Changed Methods

//        #endregion Internal Methods
//    }
//}