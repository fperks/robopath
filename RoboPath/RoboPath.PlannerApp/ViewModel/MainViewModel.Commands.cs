// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: MainViewModel.Commands.cs
// By: Frank Perks
// *******************************************************

using System.IO;
using System.Linq;
using System.Windows.Input;

using GalaSoft.MvvmLight.Command;

using GeoAPI.Geometries;

using Microsoft.Win32;

using RoboPath.Core.PathPlanning;
using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.PathPlanning;
using RoboPath.PlannerApp.Tools;

namespace RoboPath.PlannerApp.ViewModel
{
    public partial class MainViewModel
    {
        #region Fields

        #endregion Fields

        #region Properties

        #region Commands

        public RelayCommand CommandEmpty { get; private set; }

        public RelayCommand CommandNewDocument { get; private set; }
        public RelayCommand CommandOpenDocument { get; private set; }
        public RelayCommand CommandSaveDocument { get; private set; }
        public RelayCommand CommandExportDocument { get; private set; }

        public RelayCommand<MouseEventArgs> CommandMouseMove { get; private set; }
        public RelayCommand<MouseButtonEventArgs> CommandMouseDown { get; private set; }
        public RelayCommand<MouseButtonEventArgs> CommandMouseUp { get; private set; }
        public RelayCommand<KeyEventArgs> CommandKeyPressed { get; private set; }
        public RelayCommand<MouseWheelEventArgs> CommandMouseWheelScroll { get; private set; }

        public RelayCommand<ToolType> CommandExecuteTool { get; private set; }

        public RelayCommand CommandZoomIn { get; private set; }
        public RelayCommand CommandZoomOut { get; private set; }
        public RelayCommand CommandMapZoomToExtent { get; private set; }

        public RelayCommand CommandValidate { get; private set; }
        public RelayCommand CommandComputeConfigurationSpace { get; private set; }
        public RelayCommand CommandClearConfigurationSpace { get; private set; }

        public RelayCommand CommandUsePolygonalRobotBody { get; private set; }
        public RelayCommand<MapRenderState> CommandSetMapRenderState { get; private set; }

        public RelayCommand CommandFillWorkspace { get; private set; }

        //public RelayCommand CommandToggleShowVertices { get; private set; }

        #endregion Commands

        #endregion Properties

        #region Public Methods

        public void InitializeCommands()
        {
            Log.Debug("Initializing Commands");
            CommandEmpty = new RelayCommand(() => { });
            
            InitializeInputCommands();
            InitializeDocumentCommands();
            InitializeToolCommands();

            CommandZoomIn = new RelayCommand(() =>
                                                 {
                                                     ZoomControl.ContentScale += 0.2;
                                                 });

            CommandZoomOut = new RelayCommand(() =>
                                                  {
                                                      ZoomControl.ContentScale -= 0.2;
                                                  });

            CommandComputeConfigurationSpace = new RelayCommand(ComputeConfigurationSpace, CanComputeConfigurationSpace);
            CommandClearConfigurationSpace = new RelayCommand(ClearConfigurationSpace, CanClearConfigurationSpace);

            CommandUsePolygonalRobotBody = new RelayCommand(() =>
                                                                {
                                                                    // Do nothing
                                                                },
                                                            () =>
                                                                {
                                                                    return ServiceLocator.RobotManager.PolygonalBodyGeometry != null;
                                                                });

            CommandSetMapRenderState = new RelayCommand<MapRenderState>(state =>
                                                                      {
                                                                          ServiceLocator.MapRenderer.RenderState = state;
                                                                      });

            CommandFillWorkspace = new RelayCommand(FillWorkspace);

        }

        #endregion Public Methods

        #region Internal Methods

        private void InitializeInputCommands()
        {
            CommandMouseMove = new RelayCommand<MouseEventArgs>(OnMouseMove, args => true);
            CommandMouseDown = new RelayCommand<MouseButtonEventArgs>(OnMouseDown, args => true);
            CommandMouseUp = new RelayCommand<MouseButtonEventArgs>(OnMouseUp, args => true);
            CommandMouseWheelScroll = new RelayCommand<MouseWheelEventArgs>(OnMouseWheelScroll, args => true);
            CommandKeyPressed = new RelayCommand<KeyEventArgs>(OnKeyPressed, args => true);
        }

        private void InitializeDocumentCommands()
        {
            CommandNewDocument = new RelayCommand(
                OnNewDocument,
                () => true);

            CommandOpenDocument = new RelayCommand(
                () =>
                    {
                        var dialog = new OpenFileDialog()
                                         {
                                             Filter =
                                                 "XML File (*.xml)|*.xml|Show All Files (*.*)|*.*",
                                             Title = "Open Workspace",
                                             FileName = "Untitled"
                                         };

                        if(dialog.ShowDialog() != true)
                        {
                            return;
                        }

                        var filepath = Path.GetFullPath(dialog.FileName);
                        OnOpenDocument(filepath);
                    },
                () => true);

            CommandSaveDocument = new RelayCommand(
                () =>
                    {
                        var dialog = new SaveFileDialog()
                                         {
                                             Filter = "XML File (*.xml)|*.xml|Show All Files (*.*)|*.*",
                                             Title = "Save Workspace",
                                             FileName = "Untitled"
                                         };
                        if(dialog.ShowDialog() != true)
                        {
                            return;
                        }
                        var filepath = Path.GetFullPath(dialog.FileName);
                        OnSaveDocument(filepath);
                    },
                () => true);

            CommandExportDocument = new RelayCommand(
                () =>
                    {
                        var dialog = new SaveFileDialog()
                        {
                            Filter = "PNG File (*.png)|*.png|Show All Files (*.*)|*.*",
                            Title = "Save As",
                            FileName = "Untitled"
                        };
                        if(dialog.ShowDialog() != true)
                        {
                            return;
                        }
                        var filepath = Path.GetFullPath(dialog.FileName);
                        OnExportDocument(filepath);
                    },
                () => true);
        }

        private void InitializeToolCommands()
        {
            CommandExecuteTool = new RelayCommand<ToolType>(type =>
                                                                {
                                                                    ServiceLocator.ToolManager.ExecuteTool(type);
                                                                });
        }

        #region Command Delegates

        private void ComputeConfigurationSpace()
        {
            PlannerSetup.ComputeConfigurationSpace();
        }

        private bool CanComputeConfigurationSpace()
        {
            if(PlannerSetup == null)
            {
                return false;
            }

            return PlannerSetup.Robot != null && PlannerSetup.Workspace.Obstacles.Any();
        }

        private void ClearConfigurationSpace()
        {
            PlannerSetup.ClearConfigurationSpace();
        }

        private bool CanClearConfigurationSpace()
        {
            if(PlannerSetup == null)
            {
                return false;
            }

            return PlannerSetup.ConfigurationSpace != null;
        }

        private void FillWorkspace()
        {
            var workspace = ServiceLocator.PathPlannerSetup.CurrentSetup.Workspace;
            if(workspace == null)
            {
                return;
            }

            var polygon = GeometryFactory.CreatePolygon(new[]
                                                            {
                                                                new Coordinate(0.0, 0.0),
                                                                new Coordinate(workspace.Bounds.Width, 0.0),
                                                                new Coordinate(workspace.Bounds.Width, workspace.Bounds.Height),
                                                                new Coordinate(0.0, workspace.Bounds.Height),
                                                                new Coordinate(0.0, 0.0),
                                                            });
            workspace.AddOccupiedRegion(polygon);
        }

        #endregion Command Delegates

        #endregion Internal Methods
    }
}