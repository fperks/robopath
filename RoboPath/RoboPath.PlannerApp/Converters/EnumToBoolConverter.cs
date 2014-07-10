// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: EnumToBoolConverter.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Globalization;
using System.Windows.Data;

namespace RoboPath.PlannerApp.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}