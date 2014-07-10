// *******************************************************
// Project: RoboPath.UI
// File Name: IGeometrySketchFactory.cs
// By: Frank Perks
// *******************************************************
namespace RoboPath.UI.Sketch
{
    public interface IGeometrySketchFactory
    {
        IGeometrySketch CreateSketch(SketchType type);
    }
}