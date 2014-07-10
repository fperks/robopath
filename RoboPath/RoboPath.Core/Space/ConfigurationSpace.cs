// *******************************************************
// Project: RoboPath.Core
// File Name: CSpace.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

using GeoAPI.Geometries;

using NLog;

using RoboPath.Core.Geometry;

namespace RoboPath.Core.Space
{
    [Serializable]
    public class ConfigurationSpace : IConfigurationSpace
    {
        #region Fields

        [XmlIgnore]
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        [XmlIgnore]
        private Envelope _bounds;

        [XmlIgnore]
        private List<IPolygon> _obstacles;

        [XmlIgnore]
        private IGeometry _occupiedSpace;

        #endregion Fields

        #region Properties

        public event EventHandler<EventArgs> GeometryChanged;

        [XmlIgnore]
        public IGeometryFactory GeometryFactory { get; private set; }
        
        public Envelope Bounds
        {
            get { return _bounds; }
            private set { _bounds = value; }
        }

        public List<IPolygon> Obstacles
        {
            get { return _obstacles; }
            protected set{ OnObstaclesChanged(value); }
        }
        
        [XmlIgnore]
        public IGeometry OccupiedSpace
        {
            get { return _occupiedSpace; }
        }

        #endregion Properties

        #region Public Methods

        public ConfigurationSpace(IGeometryFactory geometryFactory, Envelope bounds)
        {
            GeometryFactory = geometryFactory;
            _bounds = bounds;
            Obstacles = new List<IPolygon>();
        }

        public ConfigurationSpace(IGeometryFactory geometryFactory, Envelope bounds, IList<IPolygon> obstacles)
        {
            GeometryFactory = geometryFactory;

            _bounds = bounds;
            Obstacles = obstacles == null ? null : obstacles.ToList();
        }

        public List<T> GetObstacleRegions<T>(bool copy=false) 
            where T : class, IGeometry
        {
            var result = new List<T>();
            foreach(var region in Obstacles)
            {
                if(copy)
                {
                    var copiedRegion = GeometryFactory.CreateGeometry(region);
                    result.Add((T) copiedRegion);
                }
                else
                {
                    result.Add((T) region);
                }                
            }
            return result;
        }

        #region Query Methods

        public SpaceLocationType QueryLocationType(Coordinate position)
        {
            return QueryLocationType(GeometryFactory.CreatePoint(position));
        }

        public SpaceLocationType QueryLocationType(IPoint position)
        {
            if(!Bounds.Intersects(position.Coordinate))
            {
                return SpaceLocationType.OutsideBounds;
            }

            if(OccupiedSpace.Intersects(position))
            {
                return SpaceLocationType.Obstacle;
            }
            return SpaceLocationType.FreeSpace;
        }

        public IGeometry QueryObstacle(Coordinate position)
        {
            return QueryObstacle(GeometryFactory.CreatePoint(position));
        }

        public IGeometry QueryObstacle(IPoint position)
        {
            foreach(var obstacle in Obstacles)
            {
                if(obstacle.Intersects(position))
                {
                    return obstacle;
                }
            }
            return null;
        }

        #endregion Query Methods

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Private constructor for serialization
        /// </summary>
        private ConfigurationSpace()
        {            
        }

        protected virtual void NotifyGeometryChanged()
        {
            if(GeometryChanged != null)
            {
                GeometryChanged(this, new EventArgs());
            }
        }

        protected virtual void UpdateOccupiedSpace()
        {
            if(!Obstacles.Any())
            {
                _occupiedSpace = GeometryFactory.CreateGeometryCollection(null);
            }
            else
            {                
                _occupiedSpace = GeometryOperations.Union(Obstacles);
            }
            NotifyGeometryChanged();
        }

        protected virtual void OnObstaclesChanged(IList<IPolygon> newValue)
        {
            if(newValue == null)
            {
                _obstacles = new List<IPolygon>();
            }
            else
            {
                _obstacles = new List<IPolygon>(newValue);
            }
            UpdateOccupiedSpace();
        }

        #endregion Internal Methods
    }
}