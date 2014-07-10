// *******************************************************
// Project: RoboPath.UI
// File Name: ITool.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Windows.Input;

using GeoAPI.Geometries;

using RoboPath.UI.Drawing;

namespace RoboPath.UI.Tools
{
    public interface ITool : IDrawableObject
    {
        event EventHandler<ToolDeactivatedEventArgs> Deactivated;

        bool IsInteractive { get; }
        string Name { get; }
        ToolState State { get; }
        bool IsActive { get; }

        Action<Coordinate, MouseButtonEventArgs> OnMouseDown { get; }
        Action<Coordinate, MouseButtonEventArgs> OnMouseUp { get; }
        Action<Coordinate, MouseButtonEventArgs> OnMouseDoubleClick { get; }
        Action<Coordinate> OnMouseMove { get; }
        Action<KeyEventArgs> OnKeyPressed { get; } 

        void Activate();
        void Deactivate();
    }
}