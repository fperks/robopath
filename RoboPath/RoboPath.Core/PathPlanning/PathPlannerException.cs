// *******************************************************
// Project: RoboPath.Core
// File Name: PathPlanningException.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Runtime.Serialization;

namespace RoboPath.Core.PathPlanning
{
    [Serializable]
    public class PathPlannerException : RoboPathException
    {
        #region Properties

        #endregion Properties

        #region Public Methods

        public PathPlannerException()
        {
        }

        public PathPlannerException(string message, params object[] formatArgs)
            : this(string.Format(message, formatArgs))
        {
        }

        public PathPlannerException(string message)
            : base(message)
        {
        }

        public PathPlannerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion Public Methods

        #region Internal Methods

        protected PathPlannerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Internal Methods
    }
}