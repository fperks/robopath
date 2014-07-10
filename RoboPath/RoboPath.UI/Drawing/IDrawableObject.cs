// *******************************************************
// Project: RoboPath.UI
// File Name: IDrawableObject.cs
// By: Frank Perks
// *******************************************************

using System.Windows.Media;

namespace RoboPath.UI.Drawing
{
    public interface IDrawableObject
    {
        void Draw(DrawingVisual visual);
    }
}