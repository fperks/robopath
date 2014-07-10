// *******************************************************
// Project: RoboPath.Core
// File Name: EarClippingTriangulation.cs
// By: Frank Perks
// *******************************************************

using System;
using System.Collections.Generic;
using System.Linq;

using GeoAPI.Geometries;

using NLog;

using NetTopologySuite.Algorithm;

namespace RoboPath.Core.Algorithms.Triangulation.EarClipping
{
    public class EarClippingTriangulation
    {
        #region Fields

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<bool> _isShellVertexAvaliable;
        private List<Coordinate> _shellVertices;

        #endregion Fields

        #region Properties

        public IGeometryFactory GeometryFactory { get; private set; }
        public IPolygon SourcePolygon { get; private set; }

        public IList<EarTriangle> EarList { get; private set; }

        #endregion Properties

        #region Public Methods

        public EarClippingTriangulation(IGeometryFactory geometryFactory, IPolygon sourcePolygon)
        {
            GeometryFactory = geometryFactory;
            SourcePolygon = sourcePolygon;
        }

        public IList<IPolygon> Triangulate(bool improve)
        {
            EarList = new List<EarTriangle>();
            
            // Create the shell of the geometry we are triangulating and add any holes
            CreateShell();

            var shellVertexCount = _shellVertices.Count - 1;
            _isShellVertexAvaliable = Enumerable.Repeat(true, shellVertexCount).ToList();

            var finished = false;
            var found = false;

            var k0 = 0;
            var k1 = 1;
            var k2 = 2;

            var firstK = 0;
            do
            {
                found = false;
                while(CGAlgorithms.ComputeOrientation(_shellVertices[k0], _shellVertices[k1], _shellVertices[k2]) == 0)
                {
                    k0 = k1;
                    if(k0 == firstK)
                    {
                        finished = true;
                        break;
                    }
                    k1 = k2;
                    k2 = GetNextAvaliableShellVertex(k2 + 1);
                }

                if(!finished && IsValidEdge(k0, k2))
                {
                    var ls = GeometryFactory.CreateLineString(new[]
                                                                  {
                                                                      _shellVertices[k0],
                                                                      _shellVertices[k2]
                                                                  });

                    if(SourcePolygon.Covers(ls))
                    {
                        var earPolygon = GeometryFactory.CreatePolygon(
                            GeometryFactory.CreateLinearRing(new[]
                                                                 {
                                                                     _shellVertices[k0],
                                                                     _shellVertices[k1],
                                                                     _shellVertices[k2],
                                                                     _shellVertices[k0],
                                                                 }), null);

                        if(SourcePolygon.Covers(earPolygon))
                        {
                            found = true;
                            var ear = new EarTriangle(k0, k1, k2);
                            EarList.Add(ear);
                            _isShellVertexAvaliable[k1] = false;
                            shellVertexCount--;
                            k0 = GetNextAvaliableShellVertex(0);
                            k1 = GetNextAvaliableShellVertex(k0 + 1);
                            k2 = GetNextAvaliableShellVertex(k1 + 1);
                            firstK = k0;
                            if(shellVertexCount < 3)
                            {
                                finished = true;
                            }
                        }

                    }

                    if(!finished && !found)
                    {
                        k0 = k1;
                        if(k0 == firstK)
                        {
                            finished = true;
                        }
                        else
                        {
                            k1 = k2;
                            k2 = GetNextAvaliableShellVertex(k2 + 1);
                        }
                    }
                }

            } while(!finished);

            // Create output polygons
            var polygons = new List<IPolygon>();
            foreach(var ear in EarList)
            {
                polygons.Add(CreateEarTrianglePolygon(ear));
            }

            return polygons;
        }

        #endregion Public Methods

        #region Internal Methods

        private IPolygon CreateEarTrianglePolygon(EarTriangle triangle)
        {
            var ring = GeometryFactory.CreateLinearRing(new[]
                                                            {
                                                                _shellVertices[triangle.Vertices[0]],
                                                                _shellVertices[triangle.Vertices[1]],
                                                                _shellVertices[triangle.Vertices[2]],
                                                                _shellVertices[triangle.Vertices[0]],
                                                            });

            return GeometryFactory.CreatePolygon(ring, null);
        }

        private void CreateShell()
        {
            var polygon = (IPolygon) SourcePolygon.Clone();
            polygon.Normalize();

            _shellVertices = new List<Coordinate>();
            var orderedHoles = GetOrdereredHoles(polygon);
            var coords = polygon.ExteriorRing.Coordinates;
            _shellVertices.AddRange(coords);

            foreach(var hole in orderedHoles)
            {
                JoinHoleToShell(hole);
            }            
        }

        private bool IsValidEdge(int index0, int index1)
        {
            var lineCoords = new List<Coordinate>
                                 {
                                     _shellVertices[index0],
                                     _shellVertices[index1]
                                 };

            var index = GetNextAvaliableShellVertex(index0 + 1);
            while(index != index0)
            {
                if(index != index1)
                {
                    var c = _shellVertices[index];
                    if(!(c.Equals2D(lineCoords[0]) || c.Equals2D(lineCoords[1])))
                    {
                        if(CGAlgorithms.IsOnLine(c, lineCoords.ToArray()))
                        {
                            return false;
                        }
                    }
                }
                index = GetNextAvaliableShellVertex(index + 1);
            }
            return true;
        }

        private int GetNextAvaliableShellVertex(int index)
        {
            var indexNew = index % _isShellVertexAvaliable.Count;
            while(!_isShellVertexAvaliable[indexNew])
            {
                indexNew = (indexNew + 1) % _isShellVertexAvaliable.Count;
            }
            return indexNew;
        }

        private List<IGeometry> GetOrdereredHoles(IPolygon source)
        {
            var holes = new List<IGeometry>();
            var bounds = new List<IndexedEnvelope>();

            if(source.NumInteriorRings > 0)
            {
                for(var i = 0; i < source.NumInteriorRings; i++)
                {
                    bounds.Add(new IndexedEnvelope(i, source.GetInteriorRingN(i).EnvelopeInternal));
                }

                bounds.Sort();
                for(var i = 0; i < bounds.Count; i++)
                {
                    // Get the corresponding hole for each of the indexed envelopes
                    var holeGeometry = source.GetInteriorRingN(bounds[i].Index);
                    holes.Add(holeGeometry);
                }
            }
            return holes;
        }

        private void JoinHoleToShell(IGeometry holeGeometry)
        {
            var minD2 = double.MaxValue;
            var shellVertexIndex = -1;

            var shellVertexCount = _shellVertices.Count - 1;
            var holeVertexIndex = getLowestVertex(holeGeometry);
            var holeCoords = holeGeometry.Coordinates;
            var ch = holeCoords[holeVertexIndex];
            var distanceList = new List<IndexedDouble>();

            for(var i = shellVertexCount - 1; i >= 0; i--)
            {
                var cs = _shellVertices[i];
                var d2 = (ch.X - cs.X) * (ch.X - cs.X) + (ch.Y - cs.Y) * (ch.Y - cs.Y);
                if(d2 < minD2)
                {
                    minD2 = d2;
                    shellVertexIndex = i;
                }
                distanceList.Add(new IndexedDouble(i, d2));
            }

            var join = GeometryFactory.CreateLineString(new[]
                                                            {
                                                                ch, _shellVertices[shellVertexIndex]
                                                            });
            if(SourcePolygon.Covers(join))
            {
                ApplyJoinHole(shellVertexIndex, holeCoords, holeVertexIndex);
                return;
            }

            distanceList.Sort();
            for(var i = 1; i < distanceList.Count; i++)
            {
                join = GeometryFactory.CreateLineString(new[]
                                                            {
                                                                ch, _shellVertices[distanceList[i].Index]
                                                            });
                if(SourcePolygon.Covers(join))
                {
                    shellVertexIndex = distanceList[i].Index;
                    ApplyJoinHole(shellVertexIndex, holeCoords, holeVertexIndex);
                    return;
                }
            }

            throw new InvalidOperationException("Failed to Join the Hole to Shell");
        }

        private void ApplyJoinHole(int shellVertexIndex, IList<Coordinate> holeVertices, int holeVertexIndex)
        {
            var newCoords = new List<Coordinate>
                                {
                                    new Coordinate(_shellVertices[shellVertexIndex])
                                };

            var n = holeVertices.Count - 1;
            var i = holeVertexIndex;
            do
            {
                newCoords.Add(new Coordinate(holeVertices[i]));
                i = (i + 1) % n;
            } while(i != holeVertexIndex);
            newCoords.Add(new Coordinate(holeVertices[holeVertexIndex]));
            _shellVertices.InsertRange(shellVertexIndex, newCoords);
        }

        private int getLowestVertex(IGeometry geometry)
        {
            var coords = geometry.Coordinates;
            var minY = geometry.EnvelopeInternal.MinY;
            for(var i = 0; i < coords.Length; i++)
            {
                if(Math.Abs(coords[i].Y - minY) < Constants.MachineEpsilon)
                {
                    return i;
                }
            }
            throw new InvalidOperationException("Cannot find Lowest Vertex");
        }

        #endregion Internal Methods  
    }
}