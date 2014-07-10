// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: DrawingOperationException.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Runtime.Serialization;

using RoboPath.Core;

namespace RoboPath.UI.Drawing
{
    [Serializable]
    public class DrawingOperationException : RoboPathException
    {
        #region Properties

        #endregion Properties

        #region Public Methods

        public DrawingOperationException()
        {
        }

        public DrawingOperationException(string message, params object[] formatArgs)
            : this(string.Format(message, formatArgs))
        {
        }

        public DrawingOperationException(string message)
            : base(message)
        {
        }

        public DrawingOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion Public Methods

        #region Internal Methods

        protected DrawingOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Internal Methods
    }
}