// *******************************************************
// Project: RoboPath.UI
// File Name: BaseTool.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Windows.Input;
using System.Windows.Media;

using GeoAPI.Geometries;

namespace RoboPath.UI.Tools
{
    public abstract class ToolBase : ITool
    {
        #region Fields

        #endregion Fields

        #region Properties

        public event EventHandler<ToolDeactivatedEventArgs> Deactivated;
        
        public abstract bool IsInteractive { get; }
        public abstract string Name { get; }
        public ToolState State { get; protected set; }
        public bool IsActive
        {
            get { return State == ToolState.Active; }
        }

        public Action<Coordinate, MouseButtonEventArgs> OnMouseDown { get; protected set; }
        public Action<Coordinate, MouseButtonEventArgs> OnMouseUp { get; protected set; }
        public Action<Coordinate, MouseButtonEventArgs> OnMouseDoubleClick { get; protected set; }
        public Action<Coordinate> OnMouseMove { get; protected set; }
        public Action<KeyEventArgs> OnKeyPressed { get; protected set; }
        public Action<MouseWheelEventArgs> OnMouseScrollWheel { get; protected set; }

        #endregion Properties

        #region Public Methods

        protected ToolBase()
        {
            OnMouseDown = (coordinate, args) => { };
            OnMouseUp = (coordinate, args) => { };
            OnMouseDoubleClick = (coordinate, args) => { };
            OnMouseMove = coordinate => { };
            OnKeyPressed = args => { };
            OnMouseScrollWheel = args => { };
            State = ToolState.Deactivated;
        }

        public virtual void Draw(DrawingVisual visual)
        {            
            // Do nothing
        }

        public virtual void Activate()
        {
            State = ToolState.Active;
        }

        public virtual void Deactivate()
        {
            State = ToolState.Deactivated;
            NotifyToolDeactivated();
        }

        public override string ToString()
        {
            return string.Format("UITool=[Name={0}]", Name);
        }

        #endregion Public Methods

        #region Internal Methods

        protected void NotifyToolDeactivated()
        {
            if (Deactivated != null)
            {
                Deactivated(this, new ToolDeactivatedEventArgs(this));
            }
        }

        #endregion Internal Methods
    }
}