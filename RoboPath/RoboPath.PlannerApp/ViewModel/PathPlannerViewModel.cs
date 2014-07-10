// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerViewModel.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Windows;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using NLog;

using RoboPath.Core;
using RoboPath.Core.Graph;
using RoboPath.Core.PathPlanning;
using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.PathPlanning;
using RoboPath.PlannerApp.Properties;

namespace RoboPath.PlannerApp.ViewModel
{
    public class PathPlannerViewModel : ViewModelBase
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IPathPlanner _pathPlanner;
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

        public PathPlannerManager PathPlannerManager
        {
            get { return ServiceLocator.PathPlannerManager; }
        }

        public IPathPlanner PathPlanner
        {
            get { return _pathPlanner; }
            set { OnPathPlannerChanged(value); }
        }

        public PathPlannerAlgorithmType PathPlannerAlgorithm
        {
            get
            {
                if(PathPlanner == null)
                {
                    return PathPlannerAlgorithmType.None;
                }
                return _pathPlanner.AlgorithmType;
            }
            set { OnCreatePathPlanner(value); }
        }

        public PathPlannerSetup Setup
        {
            get { return ServiceLocator.PathPlannerSetup.CurrentSetup; }
        }

        public RelayCommand<PathPlannerAlgorithmType> CommandExecuteAlgorithm { get; private set; }
        public RelayCommand<ShortestPathAlgorithm> CommandComputeShortestPath { get; private set; }
        public RelayCommand CommandComputeGridSamplingMinimumDistance { get; private set; }


        #endregion Properties

        #region Public Methods

        public PathPlannerViewModel()
        {
            _pathPlanner = null;
            CommandExecuteAlgorithm = new RelayCommand<PathPlannerAlgorithmType>(OnCreatePathPlanner, x => { return IsPathPlannerSetupValid(); });
            CommandComputeShortestPath = new RelayCommand<ShortestPathAlgorithm>(OnComputeShortestPath, x => { return PathPlanner != null; });
            CommandComputeGridSamplingMinimumDistance = new RelayCommand(ComputeMinimumGridDistance, () => { return PathPlanner != null; });
        }

        public void Initialize()
        {
            PathPlannerManager.PathPlannerChanged += (sender, args) =>
                                                                {
                                                                    PathPlanner = PathPlannerManager.PathPlanner;
                                                                };

            ServiceLocator.RobotManager.CurrentRobotChanged += (sender, args) =>
                                                                   {
                                                                       PathPlannerManager.Clear();                                                                       
                                                                   };
        }

        #endregion Public Methods

        #region Internal Methods

        private void ComputeMinimumGridDistance()
        {
            var minimum = double.MaxValue;

            // Decompose the polygons into convex components
            foreach(var source in PathPlanner.CSpace.Obstacles)
            {
                foreach(var target in PathPlanner.CSpace.Obstacles)
                {
                    var distance = source.Distance(target);
                    if(distance < 10.0)
                    {
                        continue;
                    }
                    minimum = Math.Min(source.Distance(target), minimum);
                }
            }

            Settings.Default.AlgorithmGridSamplingDistance = minimum;
        }

        private bool IsPathPlannerSetupValid()
        {
            if(Setup == null)
            {
                return false;
            }
            var isValid = Setup.StartPosition != null && Setup.GoalPosition != null;
            return isValid;
        }

        protected virtual void OnCreatePathPlanner(PathPlannerAlgorithmType algorithmType)
        {
            Log.Debug("Creating Path Planner [ {0} ]", algorithmType);

            if(Setup.ConfigurationSpace == null)
            {
                Setup.ComputeConfigurationSpace();
            }

            ServiceLocator.MapRenderer.Draw();

            try
            {
                PathPlanner = PathPlannerManager.Create(algorithmType);
                PathPlanner.StartVertex = Setup.StartPosition;
                PathPlanner.GoalVertex = Setup.GoalPosition;
                PathPlanner.Initialize();
                PathPlanner.Solve();
                ServiceLocator.MapRenderer.RenderState = MapRenderState.Graph;
            }
            catch(RoboPathException exception)
            {
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
            ServiceLocator.MapRenderer.Draw();         
        }

        protected virtual void OnComputeShortestPath(ShortestPathAlgorithm algorithm)
        {
            Log.Debug("Computing Shortest Path using Method [ {0} ]", algorithm);
            try
            {
                var state = PathPlanner.Solve();
                if(state == PathPlannerAlgorithmState.ShortestPathNotFound)
                {
                    //MessageBox.Show(
                    //    string.Format("No Valid Path Exists between Vertices [Start={0}, Goal={1}], Sorry", PathPlanner.StartVertex, PathPlanner.GoalVertex), 
                    //    "Shortest Path Not Found",
                    //    MessageBoxButton.OK, 
                    //    MessageBoxImage.Stop);
                }
                else
                {
                    ServiceLocator.MapRenderer.RenderState = MapRenderState.Graph;
                }                
            }
            catch(RoboPathException exception)
            {
                MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        protected virtual void OnPathPlannerChanged(IPathPlanner planner)
        {
            _pathPlanner = planner;
        }

        #endregion Internal Methods
    }
}