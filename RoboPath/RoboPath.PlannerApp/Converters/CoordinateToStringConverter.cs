// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: CoordinateToStringConverter.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Globalization;
using System.Windows.Data;

using GeoAPI.Geometries;

namespace RoboPath.PlannerApp.Converters
{
    [ValueConversion(typeof(Coordinate), typeof(string))]
    public class CoordinateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value == null)
            {
                return null;
            }

            var coordinate = (Coordinate)value;

            return string.Format("({0:0.00},{1:0.00})", coordinate.X, coordinate.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}