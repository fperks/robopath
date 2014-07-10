// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: CanvasTool.cs
// By: Frank Perks
// *******************************************************

using RoboPath.UI.Controls;
using RoboPath.UI.Tools;

namespace RoboPath.PlannerApp.Tools.Canvas
{
    public abstract class CanvasTool : DrawableTool, ICanvasTool
    {
        #region Fields

        #endregion Fields

        #region Properties

        public ZoomAndPanControl ZoomControl { get; protected set; }

        #endregion Properties

        #region Public Methods

        protected CanvasTool(ZoomAndPanControl zoomControl)
        {
            ZoomControl = zoomControl;
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}