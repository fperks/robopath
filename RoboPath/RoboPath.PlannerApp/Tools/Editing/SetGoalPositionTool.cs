// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: SetGoalPositionTool.cs
// By: Frank Perks
// *******************************************************

using System.Windows.Input;

using GeoAPI.Geometries;

using NLog;

using RoboPath.PlannerApp.PathPlanning;
using RoboPath.UI.Tools;

namespace RoboPath.PlannerApp.Tools.Editing
{
    public class SetGoalPositionTool : ToolBase
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public override string Name
        {
            get { return typeof(SetGoalPositionTool).Name; }
        }

        public override bool IsInteractive
        {
            get { return true; }
        }

        #endregion Properties

        #region Public Methods

        public SetGoalPositionTool()
        {
            OnMouseDown = HandleMouseInput;
        }

        #endregion Public Methods

        #region Internal Methods

        private void SetGoalPosition(Coordinate position)
        {
            Log.Debug("Setting Start Position Position [ {0} ]", position);
            var plannerSetup = new ClientServiceLocator().PathPlannerSetup.CurrentSetup;
            var space = plannerSetup.CurrentEnvironment;
            if(!space.Bounds.Contains(position))
            {
                return;
            }
            plannerSetup.GoalPosition = position;
            Deactivate();
        }

        private void HandleMouseInput(Coordinate position, MouseButtonEventArgs args)
        {
            switch(args.ChangedButton)
            {
                case MouseButton.Left:
                    SetGoalPosition(position);
                    break;
                case MouseButton.Right:
                    Deactivate();
                    break;
            }
        }

        #endregion Internal Methods
    }
}