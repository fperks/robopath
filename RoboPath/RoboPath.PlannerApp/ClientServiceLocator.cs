// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: ClientServiceLocator.cs
// By: Frank Perks
// *******************************************************

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

using Microsoft.Practices.ServiceLocation;

using RoboPath.PlannerApp.Drawing.Map;
using RoboPath.PlannerApp.Drawing.RobotPreview;
using RoboPath.PlannerApp.PathPlanning;
using RoboPath.PlannerApp.PathPlanning.Robot;
using RoboPath.PlannerApp.Tools;
using RoboPath.PlannerApp.ViewModel;

namespace RoboPath.PlannerApp
{
    public class ClientServiceLocator
    {
        static ClientServiceLocator()
        {

        }

        public ClientServiceLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            //if(ViewModelBase.IsInDesignModeStatic)
            //{
            //    // Create design time view services and models
            //    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            //}
            //else
            //{
            //    // Create run time view services and models
            //    SimpleIoc.Default.Register<IDataService, DataService>();
            //}

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PathPlannerSetupService>();
            SimpleIoc.Default.Register<PathPlannerViewModel>();
            SimpleIoc.Default.Register<RobotPreviewViewModel>();
            SimpleIoc.Default.Register<DrawingLayersViewModel>();
        }

        public MainViewModel Main
        {
            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
        }

        public RobotPreviewViewModel RobotPreviewViewModel
        {
            get { return ServiceLocator.Current.GetInstance<RobotPreviewViewModel>(); }
        }

        public PathPlannerViewModel PathPlannerViewModel
        {
            get { return ServiceLocator.Current.GetInstance<PathPlannerViewModel>(); }
        }

        public DrawingLayersViewModel DrawingLayersViewModel
        {
            get { return ServiceLocator.Current.GetInstance<DrawingLayersViewModel>(); }
        }

        public PathPlannerSetupService PathPlannerSetup
        {
            get { return ServiceLocator.Current.GetInstance<PathPlannerSetupService>(); }
        }

        public RobotManager RobotManager
        {
            get { return ServiceLocator.Current.GetInstance<RobotManager>(); }
        }

        public ToolManager ToolManager
        {
            get { return ServiceLocator.Current.GetInstance<ToolManager>(); }
        }

        public PathPlannerManager PathPlannerManager
        {
            get { return ServiceLocator.Current.GetInstance<PathPlannerManager>(); }
        }

        public MapRenderer MapRenderer
        {
            get { return ServiceLocator.Current.GetInstance<MapRenderer>(); }
        }

        public RobotPreviewRenderer RobotPreviewRenderer
        {
            get { return ServiceLocator.Current.GetInstance<RobotPreviewRenderer>(); }
        }

        public static void Cleanup()
        {
        }
    }
}