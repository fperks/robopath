// *******************************************************
// Project: RoboPath.Core
// File Name: Workspace.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Geometries;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Space
{
    [Serializable]
    public class Workspace : ConfigurationSpace, IWorkspace
    {
        #region Fields

        [XmlIgnore]
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        #endregion Fields

        #region Properties

        [XmlIgnore]
        public bool ShouldSimplifyGeometry { get; set; }

        [XmlIgnore]
        public double SimplifyTolerance { get; set; }

        #endregion Properties

        #region Public Methods

        public Workspace(IGeometryFactory geometryFactory, Envelope bounds, IList<IPolygon> obstacles=null)
            : base(geometryFactory, bounds, obstacles)
        {
            SimplifyTolerance = Constants.SimplifyTolerance;
            ShouldSimplifyGeometry = true;
        }

        #region Collection Methods

        private Workspace()
            : base(new GeometryFactory(), new Envelope(0.0, 0.0, 100.0, 100.0))
        {            
        }

        public void AddOccupiedRegion(IGeometry region)
        {
            if(region.IsEmpty)
            {
                Log.Warn("Input Region is Empty");
                return;
            }

            Log.Debug("Adding Region to Workspace [Geometry={0}]", region.AsText());
            
            // Add the obstacles to the Collection
            var result = region.GetPolygons();
            foreach(var obstacle in Obstacles)
            {
                result.Add(obstacle);
            }

            // Merge
            var mergedPolygons = GeometryOperations.Union(result).GetPolygons().ToList();
            SetObstacles(mergedPolygons);
        }

        public void RemoveOccupiedRegion(IGeometry region)
        {
            if(region.IsEmpty)
            {
                Log.Warn("Input Region is Empty");
                return;
            }

            Log.Debug("Removing Region from Workspace [Geometry={0}]", region.AsText());
            
            

            // Remove the Region Specified, by subrtracting the area
            //var occupiedSpace = OccupiedSpace.Difference(region); GeometryOperations.Union(Obstacles);

            var sourceRegion = region is IGeometryCollection ? GeometryOperations.Union(region) : region;

            var resultPolygons = new List<IPolygon>();
            var inputRegions = region.GetPolygons();
            
            foreach(var obstacle in Obstacles)
            {
                if(sourceRegion.Intersects(obstacle))
                {
                    var diff = obstacle.Difference(sourceRegion).GetPolygons();
                    resultPolygons.AddRange(diff);
                }
                else
                {
                    resultPolygons.Add(obstacle);    
                }
                

                //foreach(var inputRegion in inputRegions)
                //{
                //    if(obstacle.Intersects(inputRegion))
                //    {
                //        // treat as geometrycollection, in case the difference caused the polygon to be split
                //        var difference = obstacle.Difference(inputRegion);
                //        resultPolygons.AddRange(difference.GetPolygons());
                //    }
                //    else
                //    {
                //        resultPolygons.Add(obstacle);
                //    }
                //}
            }

            // Update Obstacle Collection
            SetObstacles(resultPolygons.ToList());
        }

        public void Clear()
        {
            Obstacles = new List<IPolygon>();
        }

        #endregion Collection Methods

        public override string ToString()
        {
            return string.Format("Workspace=[Bounds={0}, Obstacles={1}]", 
                Bounds, 
                string.Join("\n", Obstacles.Select(x => x.AsText())));
        }

        #endregion Public Methods

        #region Internal Methods

        private void SetObstacles(List<IPolygon> obstacles)
        {
            List<IPolygon> result;
            if(ShouldSimplifyGeometry && obstacles.Any())
            {
                var simplifed = GeometryOperations.Simplify(GeometryFactory, obstacles, SimplifyTolerance);
                result = new List<IPolygon>(simplifed.GetPolygons());
            }
            else
            {
                result = obstacles.ToList();
            }

            Obstacles = result;
        }
               
        #endregion Internal Methods
    }
}