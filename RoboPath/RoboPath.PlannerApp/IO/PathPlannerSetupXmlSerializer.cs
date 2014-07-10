// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerSetupXmlSerializer.cs
// By: Frank Perks
// *******************************************************

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.IO;

using RoboPath.PlannerApp.PathPlanning;

namespace RoboPath.PlannerApp.IO
{
    public class PathPlannerSetupXmlSerializer
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        
        #endregion Properties

        #region Public Methods

        public static PathPlannerSetupXmlData ToPathPlannerSetupXmlData(IGeometryFactory geometryFactory, PathPlannerSetup source)
        {
            var wktWriter = new WKTWriter();
            var data = new PathPlannerSetupXmlData()
                           {
                               BoundsHeight = source.Workspace.Bounds.Height,
                               BoundsWidth = source.Workspace.Bounds.Width,
                           };

            if(source.StartPosition != null)
            {
                data.StartVertexWKT = WKTWriter.ToPoint(source.StartPosition);
            }

            if(source.GoalPosition != null)
            {
                data.GoalVertexWKT = WKTWriter.ToPoint(source.GoalPosition);
            }

            if(source.Workspace.Obstacles.Any())
            {
                data.ObstaclesWKT = wktWriter.Write(geometryFactory.CreateGeometryCollection(source.Workspace.Obstacles.Cast<IGeometry>().ToArray()));
            }
            return data;
        }

        public PathPlannerSetupXmlSerializer(IGeometryFactory geometryFactory)
        {
            GeometryFactory = geometryFactory;
        }

        public void WriteXMLToFile(string filepath, PathPlannerSetupXmlData data)
        {
            using(var fs = new FileStream(filepath, FileMode.Create))
            {
                Serialize(data, fs);
            }  
        }

        public PathPlannerSetupXmlData ReadXMLFromFile(string filepath)
        {
            using(var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Unserialize(fs);
            }    
        }


        #endregion Public Methods

        #region Internal Methods

        private void Serialize(PathPlannerSetupXmlData data, Stream outputStream)
        {
            var serializer = new XmlSerializer(typeof(PathPlannerSetupXmlData));
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true
            };

            using(var writer = XmlWriter.Create(outputStream, xmlSettings))
            {
                serializer.Serialize(writer, data);
            }
        }

        private PathPlannerSetupXmlData Unserialize(Stream inputStream)
        {
            var serializer = new XmlSerializer(typeof(PathPlannerSetupXmlData));

            // Write it via XML
            using(var reader = XmlReader.Create(inputStream))
            {
                var result = (PathPlannerSetupXmlData)serializer.Deserialize(reader);
                return result;
            }
        }

        #endregion Internal Methods
    }
}