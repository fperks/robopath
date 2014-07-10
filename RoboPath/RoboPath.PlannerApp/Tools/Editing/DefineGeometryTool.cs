// *******************************************************
// Project: RoboPath.UI
// File Name: DefineGeometryTool.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Space;
using RoboPath.PlannerApp.Drawing;
using RoboPath.PlannerApp.PathPlanning;

namespace RoboPath.PlannerApp.Tools.Editing
{
    public abstract class DefineGeometryTool : DrawableTool, IGeometryEditingTool
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        #endregion Fields

        #region Properties

        public override bool IsInteractive
        {
            get { return true; }
        }

        public IGeometryFactory GeometryFactory { get; private set; }
        
        public IWorkspace TargetWorkspace
        {
            get { return new ClientServiceLocator().PathPlannerSetup.CurrentSetup.Workspace; }
        }

        public TargetSpaceType TargetSpaceType { get; set; }
        public IList<Coordinate> Vertices { get; protected set; } 
        
        public bool IsGeometryValid
        {
            get { return ValidateGeometry(); }
        }

        #endregion Properties

        #region Public Methods

        protected DefineGeometryTool(TargetSpaceType spaceType)
        {
            TargetSpaceType = spaceType;
            GeometryFactory = TargetWorkspace.GeometryFactory;
            Vertices = new List<Coordinate>();
        }

        public virtual void CommitGeometry(bool deactivateOnCommit = true)
        {
            Log.Debug("Committing Vertices to Workspace [ {0} ]", string.Join(",", Vertices));
            if(!IsGeometryValid)
            {
                Log.Debug("Geometry is Not Valid Cannot Commit");
                if(deactivateOnCommit)
                {
                    Deactivate();    
                }
                return;
            }

            var geometry = CreateGeometry();
            if(geometry == null || geometry.IsEmpty)
            {
                throw new InvalidOperationException("Commit Geometry is Null");
            }

            Log.Debug("Commiting Geomery [ {0} ]", geometry.AsText());

            // Add
            switch(TargetSpaceType)
            {
                case TargetSpaceType.Obstacle:
                    TargetWorkspace.AddOccupiedRegion(geometry);
                    break;
                case TargetSpaceType.Freespace:
                    TargetWorkspace.RemoveOccupiedRegion(geometry);
                    break;
                default:
                    throw new InvalidOperationException(string.Format("Unknown Target Space Type [ {0} ]", TargetSpaceType));
            }

            Vertices.Clear();
            if (deactivateOnCommit)
            {
                Deactivate();
                RedrawToolLayer();
                return;
            }
        }

        public abstract IGeometry CreateGeometry();

        #endregion Public Methods

        #region Internal Methods

        protected abstract bool ValidateGeometry();
        

        #endregion Internal Methods
    }
}