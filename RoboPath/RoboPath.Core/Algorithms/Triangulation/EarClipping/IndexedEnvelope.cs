// *******************************************************
// Project: RoboPath.Core
// File Name: IndexedEnvelope.cs
// By: Frank Perks
// *******************************************************

using System;

using GeoAPI.Geometries;

namespace RoboPath.Core.Algorithms.Triangulation.EarClipping
{
    public class IndexedEnvelope : IComparable<IndexedEnvelope>
    {
        #region Properties

        public int Index { get; private set; }
        public Envelope Value { get; private set; }

        #endregion Properties

        #region Public Methods
        
        public IndexedEnvelope(int index, Envelope source)
        {
            Index = index;
            Value = source;
        }

        public int CompareTo(IndexedEnvelope other)
        {
            var o1 = this;
            var o2 = other;

            var delta = o1.Value.MinY - o2.Value.MinY;
            if(Math.Abs(delta) < Constants.MachineEpsilon)
            {
                delta = o1.Value.MinX - o2.Value.MinX;
                if(Math.Abs(delta) < Constants.MachineEpsilon)
                {
                    return 0;
                }
            }
            return delta > 0 ? 1 : -1;
        }

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}