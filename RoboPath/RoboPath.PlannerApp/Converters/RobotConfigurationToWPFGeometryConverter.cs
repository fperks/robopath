// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: RobotConfigurationToWPFGeometryConverter.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using RoboPath.Core.Geometry;

using WPFGeometry = System.Windows.Media.Geometry;

using GeoAPI.Geometries;

using RoboPath.Core.Robot;

namespace RoboPath.PlannerApp.Converters
{
    [ValueConversion(typeof(IRobotConfiguration), typeof(WPFGeometry))]
    public class RobotConfigurationToWPFGeometryConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        } 
    }
}