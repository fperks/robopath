// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DrawingLayersViewModel.cs
// By: Frank Perks
// *******************************************************

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

using GalaSoft.MvvmLight;

using NLog;

using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.ViewModel.Views;
using RoboPath.UI.Drawing;

namespace RoboPath.PlannerApp.ViewModel
{
    public class DrawingLayersViewModel : ViewModelBase
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ClientServiceLocator _serviceLocator;
        private ObservableCollection<DrawingLayerView> _layers;
        
        #endregion Fields

        #region Properties

        public static readonly List<string> HiddenLayers;
           

        public ClientServiceLocator ServiceLocator
        {
            get
            {
                if(_serviceLocator == null)
                {
                    _serviceLocator = new ClientServiceLocator();
                }
                return _serviceLocator;
            }
        }

        public ObservableCollection<DrawingLayerView> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                RaisePropertyChanged("Layers");
            }
        }

        #endregion Properties

        #region Public Methods

        static DrawingLayersViewModel()
        {
            HiddenLayers = new List<string>()
                               {
                                   MapLayers.ToolLayer,
                                   MapLayers.BoundsLayer
                               };
        }

        public DrawingLayersViewModel()
        {
            _layers = new ObservableCollection<DrawingLayerView>();
        }

        public void Initialize()
        {
            var mapRenderer = ServiceLocator.MapRenderer;
            AddLayers();
            //LayerCollectionView = CollectionViewSource.GetDefaultView(_layers);

            RaisePropertyChanged("Layers");
        }

        #endregion Public Methods

        #region Internal Methods

        private void AddLayers()
        {
            var mapRenderer = ServiceLocator.MapRenderer;
            foreach(var layer in from layer in mapRenderer.LayerHost.Layers
                                 orderby layer.ZIndex
                                 where !HiddenLayers.Contains(layer.Name)
                                 select layer)
            {
                _layers.Add(new DrawingLayerView(layer));
            }
            
        }

        #endregion Internal Methods
    }
}