//// *******************************************************
//// Project: RoboPath.PlannerApp
//// File Name: ViewModelLocator.cs
//// By: Frank Perks
//// *******************************************************

//using GalaSoft.MvvmLight;
//using GalaSoft.MvvmLight.Ioc;
//using Microsoft.Practices.ServiceLocation;

//namespace RoboPath.PlannerApp.ViewModel
//{
//    /// <summary>
//    /// This class contains static references to all the view models in the
//    /// application and provides an entry point for the bindings.
//    /// </summary>
//    public class ViewModelLocator
//    {
//        /// <summary>
//        /// Initializes a new instance of the ViewModelLocator class.
//        /// </summary>
//        public ViewModelLocator()
//        {
//            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

//            ////if (ViewModelBase.IsInDesignModeStatic)
//            ////{
//            ////    // Create design time view services and models
//            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
//            ////}
//            ////else
//            ////{
//            ////    // Create run time view services and models
//            ////    SimpleIoc.Default.Register<IDataService, DataService>();
//            ////}

//            SimpleIoc.Default.Register<MainViewModel>();
//            SimpleIoc.Default.Register<RobotPreviewViewModel>();
//            SimpleIoc.Default.Register<PathPlannerViewModel>();            
//        }

//        public MainViewModel Main
//        {
//            get { return ServiceLocator.Current.GetInstance<MainViewModel>(); }
//        }

//        public RobotPreviewViewModel RobotPreviewViewModel
//        {
//            get { return ServiceLocator.Current.GetInstance<RobotPreviewViewModel>(); }
//        }

//        public PathPlannerViewModel PathPlannerViewModel
//        {
//            get { return ServiceLocator.Current.GetInstance<PathPlannerViewModel>(); }
//        }

//        public static void Cleanup()
//        {
//        }
//    }
//}