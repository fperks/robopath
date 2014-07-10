// *******************************************************
// Project: RoboPath.Core
// File Name: CollectionExtensions.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

namespace RoboPath.Core.Utility
{
    public static class CollectionExtensions
    {
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static string ToPrettyString<T>(this IEnumerable<T> list)
        {
            return string.Format("[{0}]", string.Join(",", list));
        }
    }
}