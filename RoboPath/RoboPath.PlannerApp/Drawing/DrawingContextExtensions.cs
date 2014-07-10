// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DrawingContextExtensions.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WPFPoint = System.Windows.Point;

using GeoAPI.Geometries;

using NetTopologySuite.Windows.Media;

using RoboPath.Core;
using RoboPath.Core.Geometry;
using RoboPath.Core.Robot;
using RoboPath.Core.Robot.Geometric;
using RoboPath.UI;

namespace RoboPath.PlannerApp.Drawing
{
    public static class DrawingContextExtensions
    {
        #region Fields

        #endregion Fields

        #region Properties

        private static IDictionary<GeometricShape, Func<double, IPointToStreamGeometryFactory>> PointStreamFactoryDefinitions { get; set; }

        #endregion Properties

        #region Public Methods

        static DrawingContextExtensions()
        {
            PointStreamFactoryDefinitions = new Dictionary<GeometricShape, Func<double, IPointToStreamGeometryFactory>>()
                                                {
                                                    {GeometricShape.Point, size => new Dot()},
                                                    {GeometricShape.Circle, size => new Circle(size)},
                                                    {GeometricShape.Triangle, size => new Triangle(size)},
                                                    {GeometricShape.Square, size => new Square(size)},
                                                    {GeometricShape.Cross, size => new Cross(size)},
                                                    {GeometricShape.X, size => new X(size)},
                                                };
        }

        public static void DrawRobotConfiguration(this DrawingContext context, IRobotConfiguration configuration, Brush brush, Pen pen, GeometricShape vertexShape, double size)
        {
            if(configuration.Robot.IsPoint)
            {
                DrawCoordinate(context, configuration.Position, brush, pen, vertexShape, size);
            }
            else if(configuration.Robot.IsCircular)
            {
                var circleRobot = (CircularRobot)configuration.Robot;
                DrawCircle(context, configuration.Position, circleRobot.Radius, brush, pen);
            }
            else if(configuration.Robot.IsPolygonal)
            {
                context.DrawGeometry(configuration.Geometry, brush, pen);
                context.DrawGeometryVertices(configuration.Geometry, brush, pen, GeometricShape.X, size);
            }
        }

        public static void DrawEnvelope(this DrawingContext context, Envelope envelope, Pen pen)
        {
            var ulPoint = new WPFPoint(envelope.MinX, envelope.MinY);
            var urPoint = new WPFPoint(envelope.MaxX, envelope.MinY);
            var lrPoint = new WPFPoint(envelope.MaxX, envelope.MaxY);
            var llPoint = new WPFPoint(envelope.MinX, envelope.MaxY);

            context.DrawLine(pen, ulPoint, urPoint);
            context.DrawLine(pen, urPoint, lrPoint);
            context.DrawLine(pen, lrPoint, llPoint);
            context.DrawLine(pen, llPoint, ulPoint);
        }

        public static void DrawGeometry(this DrawingContext context, IGeometry geometry, Brush brush, Pen pen)
        {
            if(geometry == null || geometry.IsEmpty)
            {
                return;
            }

            var writer = new WpfStreamGeometryWriter();
            var shape = writer.ToShape(geometry);
            context.DrawGeometry(brush, pen, shape);
        }

        public static void DrawGeometry(this DrawingContext context, IEnumerable<IGeometry> geometries, Brush brush, Pen pen)
        {
            foreach(var geometry in geometries)
            {
                context.DrawGeometry(geometry, brush, pen);
            }
        }

        public static void DrawCoordinates(this DrawingContext context, IEnumerable<Coordinate> coordinates, Brush brush, Pen pen, GeometricShape shapeType, double size)
        {
            var wpfGeometryFactory = PointStreamFactoryDefinitions[shapeType](size);
            var geomtryPoints = new GeometryGroup();
            foreach(var coordinate in coordinates)
            {
                geomtryPoints.Children.Add(wpfGeometryFactory.CreatePoint(coordinate.ToWPFPoint()));
            }
            context.DrawGeometry(brush, pen, geomtryPoints);
        }

        public static void DrawCoordinate(this DrawingContext context, Coordinate coordinate, Brush brush, Pen pen, GeometricShape shapeType, double size)
        {
            var wpfGeometryFactory = PointStreamFactoryDefinitions[shapeType](size);
            context.DrawGeometry(brush, pen, wpfGeometryFactory.CreatePoint(coordinate.ToWPFPoint()));
        }

        public static void DrawGeometryVertices(this DrawingContext context, IGeometry geometry, Brush brush, Pen pen, GeometricShape shapeType, double size)
        {
            if(geometry == null || geometry.IsEmpty)
            {
                return;
            }
            var vertices = geometry.GetVertices();
            context.DrawCoordinates(vertices, brush, pen, shapeType, size);
        }

        public static void DrawCircle(this DrawingContext context, Coordinate position, double radius, Brush brush, Pen pen)
        {
            context.DrawEllipse(brush, pen, position.ToWPFPoint(), radius, radius);
        }

        public static void DrawVerticesAsEllipses(this DrawingContext context, IEnumerable<Coordinate> vertices, Brush brush, Pen pen, double size)
        {
            foreach(var vertex in vertices)
            {
                DrawVertexAsEllipse(context, vertex, brush, pen, size);
            }
        }

        public static void DrawVertexAsEllipse(this DrawingContext context, Coordinate vertex, Brush brush, Pen pen, double size)
        {
            context.DrawEllipse(brush, pen, vertex.ToWPFPoint(), size, size);
        }

        public static void DrawScaledVertexAsImage(this DrawingContext context, ImageSource sourceImage, Coordinate center, double zoomScale)
        {
            var dx = sourceImage.Width / zoomScale;
            var dy = sourceImage.Height / zoomScale;
            var rect = new Rect(center.X - dx / 2.0, center.Y - dy / 2.0, dx, dy);
            context.DrawImage(sourceImage, rect);
        }

        public static void DrawVertexAsImage(this DrawingContext context, ImageSource sourceImage, Coordinate center)
        {
            var width = sourceImage.Width;
            var height = sourceImage.Height;
            var rect = new Rect(center.X - sourceImage.Width / 2.0, center.Y - sourceImage.Height / 2.0, width, height);
            context.DrawImage(sourceImage, rect);
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}