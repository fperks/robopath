// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: MainViewModel.cs
// By: Frank Perks
// *******************************************************

using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;

using RoboPath.Core.Algorithms.Decomposition;
using RoboPath.PlannerApp.PathPlanning;
using RoboPath.PlannerApp.PathPlanning.Robot;
using RoboPath.PlannerApp.Properties;
using RoboPath.PlannerApp.Tools;
using RoboPath.PlannerApp.Tools.Canvas;
using RoboPath.PlannerApp.Tools.Editing;
using RoboPath.PlannerApp.Tools.General;
using RoboPath.PlannerApp.Tools.Robot;
using RoboPath.PlannerApp.Utility;
using RoboPath.UI;
using RoboPath.UI.Controls;

namespace RoboPath.PlannerApp.ViewModel
{
    public partial class MainViewModel : ViewModelBase
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ClientServiceLocator _serviceLocator;

        private ToolType _currentToolType;
        private PolygonDecompositionStrategyType _currentDecompositionStrategy;

        private Coordinate _mousePosition;

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

        public Canvas MapContentCanvas { get; set; }
        public Grid MapContentRegion { get; set; }
        public ZoomAndPanControl ZoomControl { get; set; }
        
        public IGeometryFactory GeometryFactory { get; private set; }
        
        public PathPlannerSetup PlannerSetup
        {
            get
            {
                if(!SimpleIoc.Default.ContainsCreated<PathPlannerSetupService>())
                {
                    return null;
                }
                return ServiceLocator.PathPlannerSetup.CurrentSetup;
            }
        }
        
        #region Binding Properties
       
        public const string MapWidthPropertyName = "MapWidth";
        public double MapWidth
        {
            get
            {
                if(PlannerSetup == null)
                {
                    return Settings.Default.WorkspaceWidth;
                }
                return PlannerSetup.Bounds.MaxX;
            }
        }

        public const string MapHeightPropertyName = "MapHeight";
        public double MapHeight
        {
            get
            {
                if(PlannerSetup == null)
                {
                    return Settings.Default.WorkspaceHeight;
                }
                return PlannerSetup.Bounds.MaxY;
            }
        }

        public const string CurrentRobotTypePropertyName = "CurrentRobotType";
        public RobotType CurrentRobotType
        {
            get
            {
                return ServiceLocator.RobotManager.CurrentRobotType;
            }
            set
            {
                if(CurrentRobotType == value)
                {
                    return;
                }

                // Update the Robot Manager
                ServiceLocator.RobotManager.CurrentRobotType = value;
                RaisePropertyChanged(CurrentRobotTypePropertyName);
            }
        }

        public const string CurrentToolTypePropertyName = "CurrentToolType";
        public ToolType CurrentToolType
        {
            get { return _currentToolType; }
            set
            {
                OnCurrentToolTypeChanged(value);
            }
        }

        public const string CurrentMousePositionPropertyName = "CurrentMousePosition";
        public Coordinate CurrentMousePosition
        {
            get { return _mousePosition; }
            set
            {
                _mousePosition = value;
                RaisePropertyChanged(CurrentMousePositionPropertyName);
            }
        }

        //public const string IsGeometryVerticesVisiblePropertyName = "IsGeometryVerticesVisible";
        //public bool IsGeometryVerticesVisible
        //{
        //    get { return Settings.Default.IsGeometryVerticesVisible; }
        //    set
        //    {
        //        if(Settings.Default.IsGeometryVerticesVisible == value)
        //        {
        //            return;
        //        }
        //        Settings.Default.IsGeometryVerticesVisible = value;
        //        if(Renderer != null)
        //        {
        //            Renderer.Draw();
        //        }
        //        RaisePropertyChanged(IsGeometryVerticesVisiblePropertyName);
        //    }
        //}

        public const string CurrentDecompositionStrategyPropertyName = "CurrentDecompositionStrategy";
        public PolygonDecompositionStrategyType CurrentDecompositionStrategy
        {
            get { return _currentDecompositionStrategy; }
            set { OnCurrentDecompositionStrategyChanged(value); }
        }

        #endregion Binding Properties

        #region Event Filters

        public PropertyChangedEventFilter<ToolType> ToolManagerCurrentToolChanged { get; set; }
        
        #endregion Event Filters

        #endregion Properties

        #region Public Methods

        public MainViewModel()
        {
            // Initialize
            GeometryFactory = new GeometryFactory(new PrecisionModel(PrecisionModels.Floating));
                      
            // Default Values
            _currentDecompositionStrategy = PolygonDecompositionStrategyType.EarClippingTriangulation;

            ServiceLocator.PathPlannerSetup.GeometryFactory = GeometryFactory;

            SimpleIoc.Default.Register<RobotManager>(() =>
                                                         {
                                                             return new RobotManager(GeometryFactory);
                                                         }, true);

            SimpleIoc.Default.Register<PathPlannerManager>(() =>
                                                               {
                                                                   return new PathPlannerManager();
                                                               }, true);

            SimpleIoc.Default.Register<ToolManager>(() =>
                                                        {
                                                            return new ToolManager();
                                                        }, true);

            // Setup Commands
            InitializeCommands();
        }

        public void Initialize()
        {
            RegisterTools();
            ServiceLocator.RobotManager.CurrentRobotChanged += OnRobotChanged;
            ServiceLocator.PathPlannerSetup.CurrentPlannerSetupChanged += OnPlannerSetupChanged;

            // Setup Events
            InitializeEventFilters();

            ServiceLocator.MapRenderer.Initialize();
            ServiceLocator.RobotPreviewRenderer.Initialize();

            // Create a new Document
            OnNewDocument(); 
        }

        #endregion Public Methods

        #region Internal Methods
        
        private void UpdateDecompositionStrategy()
        {
            switch(CurrentDecompositionStrategy)
            {
                case PolygonDecompositionStrategyType.EarClippingTriangulation:
                    PlannerSetup.PolygonDecompositionStrategy = new EarClipPolygonDecomposition(GeometryFactory);
                    break;
                default:
                    throw new NotImplementedException("Decomposition Strategy");
            }
        }

        private void RegisterTools()
        {
            var toolManager = ServiceLocator.ToolManager;
            toolManager.RegisterTool(
                ToolType.Selection, 
                () => new SelectionTool());

            toolManager.RegisterTool(
                ToolType.EditDefineObstaclePolygon,
                () => new DefinePolygonTool(TargetSpaceType.Obstacle));

            toolManager.RegisterTool(
                ToolType.EditDefineFreeSpacePolygon,
                () => new DefinePolygonTool(TargetSpaceType.Freespace));

            toolManager.RegisterTool(
                ToolType.SetGoalPosition,
                () => new SetGoalPositionTool());

            toolManager.RegisterTool(
                ToolType.SetStartPosition,
                () => new SetStartPositionTool());

            toolManager.RegisterTool(
                ToolType.ZoomToExtent,
                () => new ZoomToExtentTool(ZoomControl));

            toolManager.RegisterTool(
                ToolType.ZoomToRegion,
                () => new ZoomToRegionTool(ZoomControl));

            toolManager.RegisterTool(
                ToolType.RobotDefineBodyGeometry,
                () => new DefineRobotGeometryTool(GeometryFactory));
        }

        public void InitializeEventFilters()
        {
            ToolManagerCurrentToolChanged = new PropertyChangedEventFilter<ToolType>(ServiceLocator.ToolManager, ToolManager.CurrentToolTypePropertyName, OnToolManagerCurrentToolTypeChanged);
        }

        
        #region Input Handling Methods

        private void OnMouseDown(MouseButtonEventArgs args)
        {
            var toolManager = ServiceLocator.ToolManager;
            MapContentCanvas.Focus();
            Keyboard.Focus(MapContentCanvas);

            var position = args.GetPosition(MapContentCanvas).ToCoordinate();
            if(args.ClickCount == 1)
            {
                toolManager.CurrentTool.OnMouseDown(position, args);
            }
            else if(args.ClickCount >= 2)
            {
                toolManager.CurrentTool.OnMouseDoubleClick(position, args);
            }
            args.Handled = true;
        }

        private void OnMouseUp(MouseButtonEventArgs args)
        {
            var position = args.GetPosition(MapContentCanvas).ToCoordinate();
            if(args.ClickCount == 1)
            {
                ServiceLocator.ToolManager.CurrentTool.OnMouseUp(position, args);
            }
            args.Handled = true;
        }

        private void OnMouseMove(MouseEventArgs args)
        {
            var position = args.GetPosition(MapContentCanvas).ToCoordinate();
            CurrentMousePosition = position;
            ServiceLocator.ToolManager.CurrentTool.OnMouseMove(position);
        }

        private void OnKeyPressed(KeyEventArgs args)
        {
        }

        private void OnMouseWheelScroll(MouseWheelEventArgs args)
        {
        }

        #endregion Input Handling Methods

        private void OnRobotChanged(object source, EventArgs args)
        {
            RaisePropertyChanged(CurrentRobotTypePropertyName);
        }

        #region Document Methods

        private void OnNewDocument()
        {
            ServiceLocator.PathPlannerSetup.CreateEmptySetup(Settings.Default.WorkspaceWidth, Settings.Default.WorkspaceHeight);
        }

        private void OnOpenDocument(string filepath)
        {
            ServiceLocator.PathPlannerSetup.LoadSetupFromFile(filepath);        
        }

        private void OnSaveDocument(string filepath)
        {
            ServiceLocator.PathPlannerSetup.SaveSetupToFile(filepath);
        }

        private void OnExportDocument(string filepath)
        {
            Log.Debug("Exporting Document to [ {0} ]", filepath);
            using(var stream = new FileStream(filepath, FileMode.Create))
            {
                var bmp = ServiceLocator.MapRenderer.ToBitmap();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));    
                encoder.Save(stream);
            }            
        }
        
        #endregion Document Methods

        #region Value Changed Methods

        protected virtual void OnToolManagerCurrentToolTypeChanged(ToolType tool)
        {
            if(CurrentToolType == tool)
            {
                return;
            }

            _currentToolType = tool;
            RaisePropertyChanged(CurrentToolTypePropertyName);
        }

        protected virtual void OnPlannerSetupChanged(object source, EventArgs args)
        {
            RaisePropertyChanged(MapWidthPropertyName);
            RaisePropertyChanged(MapHeightPropertyName);
            
            ZoomControl.ScaleToFit();
            ZoomControl.AnimatedSnapTo(PlannerSetup.Bounds.Centre.ToWPFPoint());

            UpdateDecompositionStrategy();
            RegisterTools();
            ServiceLocator.PathPlannerManager.Clear();
            ServiceLocator.MapRenderer.Draw();
        }

        protected virtual void OnCurrentToolTypeChanged(ToolType newToolType)
        {
            var toolManager = ServiceLocator.ToolManager;
            if(toolManager.CurrentToolType == newToolType)
            {
                return;
            }

            _currentToolType = newToolType;

            // Change to the new tool
            toolManager.CurrentToolType = newToolType;
            RaisePropertyChanged(CurrentToolTypePropertyName);
        }

        protected virtual void OnCurrentDecompositionStrategyChanged(PolygonDecompositionStrategyType newValue)
        {
            if(CurrentDecompositionStrategy == newValue)
            {
                return;
            }

            _currentDecompositionStrategy = newValue;
            if(PlannerSetup != null)
            {
                UpdateDecompositionStrategy();
            }
        }

        #endregion Value Changed Methods

        #endregion Internal Methods
    }
}