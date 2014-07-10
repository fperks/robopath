// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerSetupService.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;

using GeoAPI.Geometries;
using NetTopologySuite.IO;

using NLog;

using RoboPath.Core.Geometry;
using RoboPath.Core.Space;
using RoboPath.PlannerApp.IO;
using RoboPath.PlannerApp.Properties;
using RoboPath.UI;

namespace RoboPath.PlannerApp.PathPlanning
{
    public class PathPlannerSetupService
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ClientServiceLocator _serviceLocator;
        private PathPlannerSetup _plannerSetup;

        #endregion Fields

        #region Properties

        public event EventHandler<EventArgs> CurrentPlannerSetupChanged;

        public ClientServiceLocator ServiceLocator
        {
            get
            {
                if(_serviceLocator == null)
                {
                    _serviceLocator = new ClientServiceLocator();
                }
                return _serviceLocator;
            }
        }

        public IGeometryFactory GeometryFactory { get; set; }

        public const string CurrentSetupPropertyName = "CurrentSetup";
        public PathPlannerSetup CurrentSetup
        {
            get
            {
                return _plannerSetup;
            }
            set { OnPlannerSetupChanged(value); }
        }

        #endregion Properties

        #region Public Methods

        public PathPlannerSetupService()
        {            
        }

        public void CreateEmptySetup(int width, int height)
        {
            Log.Debug("New Document Created");
            CurrentSetup = new PathPlannerSetup(GeometryFactory)
            {
                Workspace = CreateEmptyWorkspace(width, height),
            };
            CurrentSetup.Initialize();

            var generator = new Random();
            CurrentSetup.StartPosition = new Coordinate(generator.Next(10, width - 10), generator.Next(10, height - 10));
            CurrentSetup.GoalPosition = new Coordinate(generator.Next(10, width - 10), generator.Next(10, height - 10));
        }

        public void LoadSetupFromFile(string filepath)
        {
            Log.Debug("Opening Document [ {0} ] ", filepath);

            var wktReader = new WKTReader(GeometryFactory);
            var reader = new PathPlannerSetupXmlSerializer(GeometryFactory);
            var data = reader.ReadXMLFromFile(filepath);
            var obstacles = data.ObstaclesWKT != null ? wktReader.Read(data.ObstaclesWKT).GetPolygons() : new List<IPolygon>();
            var workspace = new Workspace(GeometryFactory,
                new Envelope(0.0, data.BoundsWidth, 0.0, data.BoundsHeight),
                obstacles);

            CurrentSetup = new PathPlannerSetup(GeometryFactory)
            {
                Workspace = workspace,
            };

            CurrentSetup.Initialize();
            CurrentSetup.StartPosition = data.StartVertexWKT != null ? wktReader.Read(data.StartVertexWKT).Coordinate : null;
            CurrentSetup.GoalPosition = data.GoalVertexWKT != null ? wktReader.Read(data.GoalVertexWKT).Coordinate : null;
        }

        public void SaveSetupToFile(string filepath)
        {
            Log.Debug("Saving Document to [ {0} ]", filepath);

            var xmlData = PathPlannerSetupXmlSerializer.ToPathPlannerSetupXmlData(GeometryFactory, CurrentSetup);
            var writer = new PathPlannerSetupXmlSerializer(GeometryFactory);

            writer.WriteXMLToFile(filepath, xmlData);
        }

        #endregion Public Methods

        #region Internal Methods

        private IWorkspace CreateEmptyWorkspace(int width, int height)
        {
            var bounds = new Envelope(
                new Coordinate(0.0, 0.0),
                new Coordinate(width, height));
            return new Workspace(GeometryFactory, bounds);
        }
        
        private void NotifyCurrentPlannerSetupChanged()
        {
            if(CurrentPlannerSetupChanged != null)
            {
                CurrentPlannerSetupChanged(this, new EventArgs());
            }
        }

        protected virtual void OnPlannerSetupChanged(PathPlannerSetup setup)
        {
            _plannerSetup = setup;

            setup.Workspace.GeometryChanged += OnWorkspaceGeometryChanged;

            NotifyCurrentPlannerSetupChanged();
        }

        protected virtual void OnWorkspaceGeometryChanged(object source, EventArgs args)
        {
            var pathPlannerManager = ServiceLocator.PathPlannerManager;
            if(pathPlannerManager.PathPlanner != null)
            {
                pathPlannerManager.Clear();
            }

            ServiceLocator.MapRenderer.Draw();
        }

        #endregion Internal Methods 
    }
}