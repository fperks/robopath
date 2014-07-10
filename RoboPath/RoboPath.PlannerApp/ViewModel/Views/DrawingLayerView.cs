// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DrawingLayerView.cs
// By: Frank Perks
// *******************************************************

using System.ComponentModel;

using NLog;

using RoboPath.UI.Drawing;

namespace RoboPath.PlannerApp.ViewModel.Views
{
    public class DrawingLayerView : INotifyPropertyChanged
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;

        public DrawingLayer SourceLayer { get; private set; }

        public string Name
        {
            get { return SourceLayer.Name; }
        }

        public const string IsVisiblePropertyName = "IsVisible";
        public bool IsVisible
        {
            get { return SourceLayer.IsVisible; }
            set
            {
                SourceLayer.IsVisible = value;
                if(value)
                {
                    SourceLayer.Draw();
                }
                NotifyPropertyChanged(IsVisiblePropertyName);
            }
        }

        #endregion Properties

        #region Public Methods

        public DrawingLayerView(DrawingLayer layer)
        {
            SourceLayer = layer;
            layer.PropertyChanged += (sender, args) =>
                                         {
                                             if(args.PropertyName == DrawingLayer.IsVisiblePropertyName)
                                             {
                                                 IsVisible = ((DrawingLayer) sender).IsVisible;
                                             }
                                         };
        }

        #endregion Public Methods

        #region Internal Methods
        
        private void NotifyPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Internal Methods
    }
}