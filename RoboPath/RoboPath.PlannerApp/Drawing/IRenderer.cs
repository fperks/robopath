//using System.Collections.Generic;
//using System.Drawing;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;

//using GeoAPI.Geometries;

//using RoboPath.Core;
//using RoboPath.Core.Robot;
//using RoboPath.PlannerApp.PathPlanning;
//using RoboPath.UI.Controls;
//using RoboPath.UI.Drawing;

//using Brush = System.Windows.Media.Brush;
//using Pen = System.Windows.Media.Pen;

//namespace RoboPath.PlannerApp.Drawing
//{
//    public interface IRenderer
//    {
//        RenderState RenderState { get; set; }
//        DrawingLayerHost LayerHost { get; }
//        ZoomAndPanControl ZoomControl { get; set; }
//        double ZoomScale { get; set; }

//        PathPlannerSetup PlannerSetup { get; set; }

//        void Draw();
//        void DrawLayerRange(IEnumerable<LayerTypeId> ids);
//        void DrawLayer(LayerTypeId id);
//        DrawingLayer<LayerTypeId> GetLayer(LayerTypeId id);
//        void Clear();
//        void ClearLayer(LayerTypeId id);
//        void HideLayer(LayerTypeId id);
//        void ShowLayer(LayerTypeId id);
//        void ShowAllLayers();

//        BitmapSource ToBitmap();

//        void ZoomToRegion(Envelope envelope);
//        void ZoomToRegion(Coordinate p1, Coordinate p2);

//        void DrawCoordinates(DrawingContext context, IEnumerable<Coordinate> coordinates, Brush brush, Pen pen, GeometricShape shapeType, double size);
//        void DrawCoordinate(DrawingContext context, Coordinate coordinate, Brush brush, Pen pen, GeometricShape shapeType, double size);
//        void DrawGeometryVertices(DrawingContext context, IGeometry geometry, Brush brush, Pen pen, GeometricShape shapeType, double size);        
//    }
//}