// *******************************************************
// Project: RoboPath.UI
// File Name: WPFExtensions.cs
// By: Frank Perks
// *******************************************************

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using NetTopologySuite.Windows.Media;

using WPFGeometry = System.Windows.Media.Geometry;

using GeoAPI.Geometries;

namespace RoboPath.UI
{
    public static class WPFExtensions
    {
        public static Point ToWPFPoint(this Coordinate position)
        {
            return new Point(position.X, position.Y);
        }

        public static Rect ToWPFRect(this Envelope envelope)
        {
            return new Rect(envelope.MinX, envelope.MinY, envelope.Width, envelope.Height);
        }

        public static Coordinate ToCoordinate(this Point point)
        {
            return new Coordinate(point.X, point.Y);
        }

        public static Coordinate ToCoordinate(this MouseButtonEventArgs args, IInputElement relativeTo)
        {
            return args.GetPosition(relativeTo).ToCoordinate();
        }
    }
}