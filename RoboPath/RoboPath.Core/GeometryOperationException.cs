// *******************************************************
// Project: RoboPath.Core
// File Name: GeometryOperationException.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Runtime.Serialization;

using GeoAPI.Geometries;

namespace RoboPath.Core
{
    [Serializable]
    public class GeometryOperationException : RoboPathException
    {
        #region Properties

        public IGeometry SourceGeometry { get; private set; }

        #endregion Properties

        #region Public Methods

        public GeometryOperationException()
        {
        }

        public GeometryOperationException(string message, params object[] formatArgs)
            : this(string.Format(message, formatArgs))
        {
        }

        public GeometryOperationException(IGeometry source, string message)
            : base(message)
        {
            SourceGeometry = source;
        }

        public GeometryOperationException(IGeometry source, string message, params object[] formatArgs)
            : this(source, string.Format(message, formatArgs))
        {
        }

        public GeometryOperationException(string message)
            : base(message)
        {
        }

        public GeometryOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public override string ToString()
        {
            if(SourceGeometry != null)
            {
                return string.Format("{0} [Geometry={1}]", Message, SourceGeometry.AsText());
            }
            return Message;
        }

        #endregion Public Methods

        #region Internal Methods

        protected GeometryOperationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion Internal Methods
 
    }
}