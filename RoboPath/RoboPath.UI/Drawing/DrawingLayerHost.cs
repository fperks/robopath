// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DrawingLayerHost.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

using NLog;

namespace RoboPath.UI.Drawing
{
    public class DrawingLayerHost : FrameworkElement
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly VisualCollection _children;
        private readonly Dictionary<string, DrawingLayer> _layers;

        #endregion Fields

        public IEnumerable<DrawingLayer> Layers
        {
            get { return _layers.Values; }
        }

        public int LayerCount
        {
            get { return _layers.Count; }
        }

        #region Public Methods

        public DrawingLayerHost()
        {
            _children = new VisualCollection(this);
            _layers = new Dictionary<string, DrawingLayer>();
        }

        public void DrawAll()
        {
            foreach(var layer in Layers)
            {
                layer.Draw();
            }
        }

        public void AddLayer(DrawingLayer layer)
        {
            Log.Debug("Adding Layer [ {0} ]", layer);
            if(HasLayer(layer.Name))
            {
                throw new DrawingOperationException(string.Format("Layer [ {0} ] Already Exists", layer));
            }

            _layers.Add(layer.Name, layer);
            ReorderChildren();
        }

        public void AddLayer(string name, Action<DrawingVisual> callback, bool isVisible = true)
        {
            var layer = new DrawingLayer(name, LayerCount, callback, isVisible);
            AddLayer(layer);            
        }

        public void AddLayerRange(IEnumerable<DrawingLayer> layers)
        {
            foreach(var layer in layers)
            {
                if(HasLayer(layer.Name))
                {
                    throw new DrawingOperationException(string.Format("Layer [ {0} ] Already Exists", layer));
                }
                Log.Debug("Adding Layer [ {0} ]", layer);
                _layers.Add(layer.Name, layer);
            }
            ReorderChildren();
        }

        public DrawingLayer GetLayer(string layerName)
        {
            if(!HasLayer(layerName))
            {
                throw new DrawingOperationException(string.Format("No Layer with Name [ {0} ] exists", layerName));
            }
            return _layers[layerName];
        }

        public bool HasLayer(string layerName)
        {
            return _layers.ContainsKey(layerName);
        }

        public void RemoveLayer(string layerName)
        {
            Log.Debug("Removing Drawing Layer [ {0} ]", layerName);
            if(!HasLayer(layerName))
            {
                throw new DrawingOperationException("Attempting to Remove Non Existant Layer [ {0} ]", layerName);
            }
            _layers.Remove(layerName);
            ReorderChildren();
        }

        #endregion Public Methods

        #region Internal Methods

        private void ReorderChildren()
        {
            _children.Clear();
            var layers = _layers
                .OrderByDescending(pair => pair.Value.ZIndex)
                .Select(pair => pair.Value)
                .ToList();
            layers.ForEach(layer => _children.Add(layer));
        }

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            if(index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return _children[index];
        }

        #endregion Internal Methods
    }
}