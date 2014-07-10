// *******************************************************
// Project: RoboPath.Core
// File Name: IndexedDouble.cs
// By: Frank Perks
// *******************************************************

using System;

namespace RoboPath.Core.Algorithms.Triangulation.EarClipping
{
    public class IndexedDouble : IComparable<IndexedDouble>
    {
        #region Properties

        public int Index { get; private set; }
        public double Value { get; private set; }

        #endregion Properties

        #region Public Methods

        public IndexedDouble(int index, double value)
        {
            Index = index;
            Value = value;
        }

        public int CompareTo(IndexedDouble other)
        {
            var o1 = this;
            var o2 = other;

            var delta = o1.Value - o2.Value;
            if(Math.Abs(delta) < Constants.MachineEpsilon)
            {
                return 0;
            }
            return delta > 0 ? 1 : -1;
        }

        #endregion Public Methods
    }
}