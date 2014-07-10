// *******************************************************
// Project: RoboPath.UI
// File Name: DrawingLayer.cs
// By: Frank Perks
// *******************************************************

using System;
using System.ComponentModel;
using System.Windows.Media;

namespace RoboPath.UI.Drawing
{
    public class DrawingLayer : DrawingVisual, INotifyPropertyChanged
    {
        #region Fields

        private readonly string _name;
        private bool _isVisible;
        private int _zindex;
        private Action<DrawingVisual> _renderCallback;

        #endregion Fields

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get { return _name; }
        }

        public Action<DrawingLayer> RenderCallback
        {
            get { return _renderCallback; }
        }

        public const string ZIndexPropertyName = "ZIndex";
        public int ZIndex
        {
            get { return _zindex; }
            set { OnZIndexChanged(value); }
        }

        public const string IsVisiblePropertyName = "IsVisible";
        public bool IsVisible
        {
            get { return _isVisible; }
            set { OnLayerVisibilityChanged(value); }
        }

        #endregion Properties

        #region Public Methods

        public DrawingLayer(string name, int priority, Action<DrawingVisual> renderCallback, bool isVisible = true)
        {
            _name = name;
            _isVisible = isVisible;
            _zindex = priority;
            _renderCallback = renderCallback;
        }

        public void Draw()
        {
            if(!IsVisible)
            {
                Clear();
                return;
            }
            RenderCallback(this);
        }

        public void Clear()
        {
            var context = RenderOpen();
            context.Close();
        }

        #endregion Public Methods

        #region Internal Methods

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnLayerVisibilityChanged(bool newValue)
        {
            if(IsVisible == newValue)
            {
                return;
            }

            _isVisible = newValue;
            if(!IsVisible)
            {
                Clear();
            }

            NotifyPropertyChanged(IsVisiblePropertyName);
        }

        protected virtual void OnZIndexChanged(int value)
        {
            if(ZIndex != value)
            {
                return;
            }

            _zindex = value;
            NotifyPropertyChanged(ZIndexPropertyName);
        }

        #endregion Internal Methods




    }
}