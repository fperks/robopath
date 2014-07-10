// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerManager.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

using NLog;

using RoboPath.Core.PathPlanning;

namespace RoboPath.PlannerApp.PathPlanning
{
    public class PathPlannerManager
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private IPathPlanner _pathPlanner;

        #endregion Fields

        #region Properties

        public event EventHandler<EventArgs> PathPlannerChanged;

        public Dictionary<PathPlannerAlgorithmType, Func<IPathPlanner>> PlannerDefinitions { get; private set; }

        public IPathPlanner PathPlanner
        {
            get { return _pathPlanner; }
            private set
            {
                _pathPlanner = value;
                NotifyPathPlannerChanged();
            }
        }

        public PathPlannerAlgorithmType PathPlannerAlgorithm
        {
            get
            {
                if(_pathPlanner == null)
                {
                    return PathPlannerAlgorithmType.None;
                }
                return _pathPlanner.AlgorithmType;
            }
        }

        #endregion Properties

        #region Public Methods

        public PathPlannerManager()
        {
            PlannerDefinitions = new Dictionary<PathPlannerAlgorithmType, Func<IPathPlanner>>
                                     {
                                         {PathPlannerAlgorithmType.None, () => null}
                                     };
        }

        public void Register(PathPlannerAlgorithmType type, Func<IPathPlanner> callback)
        {
            Log.Debug("Registering Path Planner Algorithm [ {0} ]", type);
            PlannerDefinitions[type] = callback;
        }

        public IPathPlanner Create(PathPlannerAlgorithmType type)
        {
            if(!PlannerDefinitions.ContainsKey(type))
            {
                throw new ArgumentException(string.Format("Unknown Algorithm Type [ {0} ]", type));
            }
            var planner = PlannerDefinitions[type]();
            PathPlanner = planner;
            return planner;
        }

        public void Clear()
        {
            Create(PathPlannerAlgorithmType.None);
        }

        #endregion Public Methods

        #region Internal Methods

        private void NotifyPathPlannerChanged()
        {
            if(PathPlannerChanged != null)
            {
                PathPlannerChanged(this, new EventArgs());
            }
        }

        #endregion Internal Methods
    }
}