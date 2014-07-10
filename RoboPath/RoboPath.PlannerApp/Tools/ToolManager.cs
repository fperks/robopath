// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ToolManager.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

using NLog;

using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.Tools.General;
using RoboPath.UI.Tools;

namespace RoboPath.PlannerApp.Tools
{
    public class ToolManager : INotifyPropertyChanged
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private ITool _currentTool;
        private ToolType _currentToolType;
        private ClientServiceLocator _serviceLocator;

        #endregion Fields

        #region Properties

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

        public event PropertyChangedEventHandler PropertyChanged;
       
        public IDictionary<ToolType, Func<ITool>> ToolFactoryDefinitions { get; private set; }

        public ToolType DefaultToolType
        {
            get { return ToolType.Selection; }
        }

        public ITool CurrentTool
        {
            get { return _currentTool; }
            private set { OnCurrentToolChanged(value); }
        }

        public const string CurrentToolTypePropertyName = "CurrentToolType";
        public ToolType CurrentToolType
        {
            get { return _currentToolType; }
            set { OnCurrentToolTypeChanged(value); }
        }

        #endregion Properties

        #region Public Methods

        public ToolManager()
        {
            ToolFactoryDefinitions = new Dictionary<ToolType, Func<ITool>>();
            _currentToolType = ToolType.Selection;
            _currentTool = new SelectionTool();
        }

        public void Draw(DrawingVisual visual)
        {
            if(CurrentTool != null)
            {
                CurrentTool.Draw(visual);
            }
        }

        public void RegisterTool(ToolType key, Func<ITool> callback)
        {
            ToolFactoryDefinitions[key] = callback;
        }

        public void SetCurrentTool(ToolType key)
        {
            CurrentToolType = key;
        }

        public void ExecuteTool(ToolType key)
        {
            var tool = CreateTool(key);
            if(tool.IsInteractive)
            {
                throw new InvalidOperationException(string.Format("Tool [ {0} ] is Interactive Cannot Execute Interactive Tool", key));
            }

            tool.Activate();
            if(tool.IsActive)
            {
                Log.Warn("Tool [ {0} ], was excuted but did not finish on execute", key);
            }
        }

        public void ClearCurrentTool()
        {
            // Clear the current layer

            var renderer = ServiceLocator.MapRenderer;
            renderer.ClearLayer(MapLayers.ToolLayer);

            CurrentToolType = DefaultToolType;            
        }

        #endregion Public Methods

        #region Internal Methods

        private ITool CreateTool(ToolType type)
        {
            return ToolFactoryDefinitions[type]();
        }

        private void OnCurrentToolChanged(ITool newTool)
        {
            Log.Debug("CurrentTool Changed From [ {0} -> {1} ]", CurrentTool.Name, newTool.Name);

            if(CurrentTool != null)
            {
                CurrentTool.Deactivated -= OnCurrentToolDeactivated;
            }

            if(CurrentTool.IsActive)
            {
                CurrentTool.Deactivate();
            }

            _currentTool = newTool;
            CurrentTool.Deactivated += OnCurrentToolDeactivated;
        }

        private void OnCurrentToolDeactivated(object src, ToolDeactivatedEventArgs args)
        {
            Log.Debug("ActiveTool [ {0} ] finished", args.Source);
            ClearCurrentTool();
        }

        protected virtual void OnCurrentToolTypeChanged(ToolType newValue)
        {
            if(CurrentToolType == newValue)
            {
                return;
            }
            _currentToolType = newValue;
            CurrentTool = CreateTool(CurrentToolType);
            NotifyPropertyChanged(CurrentToolTypePropertyName);
        }

        protected virtual void NotifyPropertyChanged(string property)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion Internal Methods
    }
}