// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerValidationException.cs
// By: Frank Perks
// *******************************************************

using System;

namespace RoboPath.PlannerApp.PathPlanning
{
    public class PathPlannerValidationException : Exception
    {
         public PathPlannerValidationException(string message)
             : base(message)
         {             
         }
    }
}