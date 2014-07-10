// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: CoordinateToWPFPointConverter.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Globalization;
using System.Windows.Data;

using RoboPath.Core.Geometry;
using RoboPath.UI;

using WPFPoint = System.Windows.Point;

using GeoAPI.Geometries;

namespace RoboPath.PlannerApp.Converters
{
    [ValueConversion(typeof(Coordinate), typeof(WPFPoint))]
    public class CoordinateToWPFPointConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }
            var coordinate = (Coordinate) value;
            if(coordinate.IsNull())
            {
                return null;
            }
            return coordinate.ToWPFPoint();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}