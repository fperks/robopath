// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ICanvasTool.cs
// By: Frank Perks
// *******************************************************

using RoboPath.UI.Controls;

namespace RoboPath.PlannerApp.Tools.Canvas
{
    public interface ICanvasTool
    {
        ZoomAndPanControl ZoomControl { get; } 
    }
}