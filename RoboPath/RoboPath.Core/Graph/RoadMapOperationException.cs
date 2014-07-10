// *******************************************************
// Project: RoboPath.Core
// File Name: RoadMapOperationException.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Runtime.Serialization;

namespace RoboPath.Core.Graph
{
    [Serializable]
    public class RoadMapOperationException : RoboPathException
    {
        #region Properties

        #endregion Properties

        #region Public Methods

        public RoadMapOperationException()
        {
        }

        public RoadMapOperationException(string message, params object[] formatArgs)
            : this(string.Format(message, formatArgs))
        {
        }

        public RoadMapOperationException(string message)
            : base(message)
        {
        }

        public RoadMapOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion Public Methods

        #region Internal Methods

        protected RoadMapOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Internal Methods
    }
}