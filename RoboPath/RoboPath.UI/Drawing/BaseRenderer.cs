// *******************************************************
// Project: RoboPath.UI
// File Name: Renderer.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GeoAPI.Geometries;

using NLog;

using RoboPath.UI.Controls;

namespace RoboPath.UI.Drawing
{
    public abstract class BaseRenderer
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DrawingLayerHost _layerHost;
        private readonly ZoomAndPanControl _zoomControl;

        #endregion Fields

        #region Properties

        public const double MinZoomRegionArea = 10.0;
        public const double MinPenThickness = 1.0;

        public DrawingLayerHost LayerHost
        {
            get { return _layerHost; }
        }

        public ZoomAndPanControl ZoomControl
        {
            get { return _zoomControl; }
        }

        public double ZoomScale
        {
            get
            {
                return ZoomControl.ContentScale;
            }
            set
            {
                OnZoomScaleChanged(value);
            }
        }

        #endregion Properties

        #region Public Methods

        protected BaseRenderer(DrawingLayerHost layerHost, ZoomAndPanControl zoomControl)
        {
            _layerHost = layerHost;
            _zoomControl = zoomControl;
        }

        public abstract void Initialize();

        public virtual void Draw()
        {
            Clear();
            LayerHost.DrawAll();
        }

        public virtual void Clear()
        {
            foreach(var layer in LayerHost.Layers)
            {
                layer.Clear();
            }
        }

        #region Layer Management

        public void DrawLayer(string layerName)
        {
            LayerHost.GetLayer(layerName).Draw();
        }

        public void RenderLayerRange(IEnumerable<string> layerNames)
        {
            foreach(var name in layerNames)
            {
                LayerHost.GetLayer(name).Draw();
            }
        }

        public DrawingLayer GetLayer(string layerName)
        {
            return LayerHost.GetLayer(layerName);
        }

        public void ClearLayer(string layerName)
        {
            GetLayer(layerName).Clear();
        }

        public void HideLayer(string layerName)
        {
            var layer = LayerHost.GetLayer(layerName);
            layer.IsVisible = false;
        }

        public void ShowLayer(string layerName)
        {
            var layer = LayerHost.GetLayer(layerName);
            layer.IsVisible = true;
        }

        public void ShowAllLayers()
        {
            foreach(var layer in LayerHost.Layers)
            {
                layer.IsVisible = true;
            }
        }

        public void HideAllLayers()
        {
            foreach(var layer in LayerHost.Layers)
            {
                layer.IsVisible = false;
            }
        }

        #endregion Layer Management

        public virtual BitmapSource ToBitmap()
        {
            return null;
        }

        #region Zooming Methods

        public virtual double ScaleValueToZoom(double value)
        {
            return value / ZoomScale;
        }

        public virtual void ZoomToRegion(Envelope envelope)
        {
            if(envelope.Area < MinZoomRegionArea)
            {
                Log.Debug("Envelope too Small for Zooming");
                return;
            }
            ZoomControl.AnimatedZoomTo(envelope.ToWPFRect());
        }

        public virtual void ZoomToRegion(Coordinate p1, Coordinate p2)
        {
            var envelope = new Envelope(p1, p2);
            ZoomToRegion(envelope);
        }

        #endregion Zooming Methods


        #endregion Public Methods

        #region Internal Methods

        protected virtual T GetDrawingResource<T>(string key)
        {
            var resource = (T)Application.Current.Resources[key];
            return resource;
        }

        protected virtual void OnContentScaleChanged(object source, EventArgs args)
        {
            
        }

        protected abstract void LoadDefaultLayers();

        protected abstract void OnZoomScaleChanged(double newScale);

        #endregion Internal Methods
    }
}