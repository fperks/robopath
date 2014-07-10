// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DrawableTool.cs
// By: Frank Perks
// *******************************************************

using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.UI.Tools;

namespace RoboPath.PlannerApp.Tools
{
    public abstract class DrawableTool : ToolBase
    {
        #region Fields

        #endregion Fields

        #region Properties

        #endregion Properties

        #region Public Methods

        public override void Deactivate()
        {
            var mapRenderer = new ClientServiceLocator().MapRenderer;
            if(mapRenderer != null)
            {
                mapRenderer.ClearLayer(MapLayers.ToolLayer);
            }
            base.Deactivate();
        }

        #endregion Public Methods

        #region Internal Methods

        protected virtual void RedrawToolLayer()
        {
            var mapRenderer = new ClientServiceLocator().MapRenderer;
            if(mapRenderer != null)
            {
                mapRenderer.DrawLayer(MapLayers.ToolLayer);
            }
        }


        #endregion Internal Methods
    }
}