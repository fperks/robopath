// *******************************************************
// Project: RoboPath.PlannerApp
// File Name: PathPlannerSetupXmlData.cs
// By: Frank Perks
// *******************************************************

using System.Xml.Serialization;

namespace RoboPath.PlannerApp.IO
{
    [XmlRoot("PathPlannerSetup")]
    public class PathPlannerSetupXmlData
    {
        #region Fields

        #endregion Fields

        #region Properties

        [XmlElement("StartVertex")]
        public string StartVertexWKT { get; set; }

        [XmlElement("GoalVertex")]
        public string GoalVertexWKT { get; set; }

        [XmlElement("BoundsWidth")]
        public double BoundsWidth { get; set; }

        [XmlElement("BoundsHeight")]
        public double BoundsHeight { get; set; }

        [XmlElement("Obstacles")]
        public string ObstaclesWKT { get; set; }


        //[XmlElement("RobotBodyGeometry")]
        //public string RobotBodyWKT { get; set; }

        //[XmlElement("RobotBodyType")]
        //public RobotBodyType RobotBodyType { get; set; }

        #endregion Properties

        #region Public Methods

        #endregion Public Methods

        #region Internal Methods

        #endregion Internal Methods
    }
}