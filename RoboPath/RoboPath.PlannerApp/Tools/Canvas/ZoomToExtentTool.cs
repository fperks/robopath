// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ZoomToExtentTool.cs
// By: Frank Perks
// *******************************************************

using RoboPath.UI.Controls;

namespace RoboPath.PlannerApp.Tools.Canvas
{
    public class ZoomToExtentTool : CanvasTool
    {
        #region Properties
        
        public override string Name
        {
            get { return typeof(ZoomToExtentTool).Name; }
        }

        public override bool IsInteractive
        {
            get { return false; }
        }

        #endregion Properties

        #region Public Methods

        public ZoomToExtentTool(ZoomAndPanControl canvas)
            : base(canvas)
        {
            
        }

        public override void Activate()
        {
            ZoomControl.ScaleToFit();
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods 
    }
}