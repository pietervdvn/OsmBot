using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using OsmSharp.Complete;
using OsmSharp.Streams.Complete;
using OsmSharp.Tags;

namespace Wikipedia2Street
{
    public class Conflater
    {
        private readonly OsmCompleteStreamSource _osmStreamComplete;
        private readonly OsmCompleteStreamSource _grbStreamComplete;

        private const bool TestOnly = false;

        public Conflater(OsmCompleteStreamSource osmStreamComplete, OsmCompleteStreamSource grbStreamComplete)
        {
            _osmStreamComplete = osmStreamComplete;
            _grbStreamComplete = grbStreamComplete;
        }


        private static Polygon ToPolygon(CompleteWay w)
        {
            var coordinates = new Coordinate[w.Nodes.Length];
            var i = 0;
            foreach (var n in w.Nodes)
            {
                var p = new Coordinate(n.Longitude.Value, n.Latitude.Value);
                coordinates[i] = p;
                i++;
            }

            return new Polygon(new LinearRing(coordinates));
        }
        
        /// <summary>
        /// The actual conflation.
        /// Returns true if conflation is finished and this grbPoly has been matched
        /// </summary>
        /// <param name="grbPoly"></param>
        /// <param name="osmPoly"></param>
        /// <param name="grbTags"></param>
        /// <param name="osmTags"></param>
        /// <param name="cs"></param>
        /// <returns></returns>
        private static bool AttemptConflate(Polygon grbPoly, Polygon osmPoly, TagsCollectionBase grbTags,
            CompleteWay osmObj, EasyChangeset cs)
        {
            // Is there geographical match?

            if (Math.Abs(osmPoly.Centroid.Distance(grbPoly.Centroid)) > 0.000001)
            {
                return false;
            }

            if (osmPoly.Difference(grbPoly).Area > 0.000000001)
            {
                return false;
            }

            if (grbPoly.Difference(osmPoly).Area > 0.000000001)
            {
                return false;
            }


            if (osmObj.Tags.TryGetValue("building", out var bValue))
            {
                if (bValue.Equals("yes"))
                {
                    osmObj.Tags.RemoveKey("building");
                }
                else
                {
                    grbTags.TryGetValue("building", out var grbBuilding);
                    if (bValue != grbBuilding)
                    {
                        Console.WriteLine($"Preferring OSM building-tag '{bValue}' over the grb tag '{grbBuilding}'");
                        grbTags.RemoveKey("building");
                    }
                }
            }

            if (osmObj.Tags.TryGetValue("source:geometry:ref", out var osmSourceRef))
            {
                grbTags.TryGetValue("source:geometry:ref", out var grbRef);
                if (!osmSourceRef.Equals(grbRef))
                {
                    throw new Exception($"MISMATCH for {osmObj}: {osmSourceRef} != {grbRef}");
                }

                osmObj.Tags.RemoveKey("source:geometry:date");
            }


            if (osmObj.Tags.TryGetValue("addr:housenumber", out var osmHouseNumber))
            {
                grbTags.TryGetValue("addr:housenumber", out var grbHouseNumber);
                if (!osmHouseNumber.Equals(grbHouseNumber))
                {
                    Console.WriteLine($"GRB and OSM disagree over {osmObj}. Putting a fixme instead");
                    grbTags.RemoveKey("addr:housenumber");
                    grbTags.Add("fixme", "GRB thinks that this has number " + grbHouseNumber);
                }
            }


            foreach (var grbTag in grbTags)
            {
                try
                {

                    osmObj.AddNewTag(grbTag.Key, grbTag.Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{osmObj}: {e.Message}: skipping match");
                }
            }

            cs.AddChange(osmObj);


            return true;
        }

        public EasyChangeset Conflate()
        {
            var cs = new EasyChangeset();

            var osmBuildings = new List<(Polygon, CompleteWay way)>();

            var count = 0;

            foreach (var osmGeo in _osmStreamComplete)
            {
                if (!(osmGeo is CompleteWay w))
                {
                    continue;
                }

                if (!w.IsClosed())
                {
                    continue;
                }

                if (!w.Tags.TryGetValue("building", out _))
                {
                    continue;
                }

                if (w.Tags.TryGetValue("source:geometry:ref", out _))
                {
                    // Already from GRB or conflated - skip!
                    continue;
                }

                if (TestOnly && w.Id != 293687203)
                {
                    continue;
                }

                osmBuildings.Add((ToPolygon(w), w));
            }

            Console.WriteLine($"Found {osmBuildings.Count} osm buildings");


            var dirty = new HashSet<CompleteWay>();

            foreach (var grbGeo in _grbStreamComplete)
            {
                if (!(grbGeo is CompleteWay w))
                {
                    continue;
                }

                if (!w.IsClosed())
                {
                    continue;
                }

                w.Tags.TryGetValue("source:geometry:ref", out var grbRef);
                if (TestOnly && !grbRef.Equals("Gbg/3135246"))
                {
                    continue;
                }


                var grbTags = w.Tags;
                var grbPoly = ToPolygon(w);

                foreach (var (osmBuilding, osmTags) in osmBuildings)
                {
                    if (dirty.Contains(osmTags))
                    {
                        continue;
                    }

                    if (AttemptConflate(grbPoly, osmBuilding, grbTags, osmTags, cs))
                    {
                        dirty.Add(osmTags);
                        count++;
                        break;
                    }
                }


                if (count > 10)
                {
                    break;
                }
                
            }

            Console.WriteLine("Conflated " + count);

            
            
            return cs;
        }
    }
}