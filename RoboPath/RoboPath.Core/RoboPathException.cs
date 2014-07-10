// *******************************************************
// Project: RoboPath.Core
// File Name: RoboPathException.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Runtime.Serialization;

namespace RoboPath.Core
{
    [Serializable]
    public class RoboPathException : Exception
    {
        #region Public Methods

        public RoboPathException()
        {
        }

        public RoboPathException(string message, params object[] formatArgs)
            : this(string.Format(message, formatArgs))
        {
        }

        public RoboPathException(string message)
            : base(message)
        {
        }

        public RoboPathException(string message, Exception inner)
            : base(message, inner)
        {
        }

        #endregion Public Methods

        #region Internal Methods

        protected RoboPathException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Internal Methods
    }
}