using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using OsmBot.Download;
using OsmSharp.Complete;
using OsmSharp.Tags;

namespace OsmBot
{
    public static class Conflater
    {
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
        private static bool AttemptConflate(Geometry grbPoly, Geometry osmPoly, TagsCollectionBase grbTags,
            ICompleteOsmGeo osmObj, EasyChangeset cs)
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

            if (grbTags == null)
            {
                Console.WriteLine("PANIC for grb object " + grbPoly);
                return false;
            }


            if (osmObj.Tags.TryGetValue("building", out var osmBuildingValue))
            {
                if (osmBuildingValue.Equals("yes"))
                {
                    osmObj.Tags.RemoveKey("building");
                }
                else
                {
                    grbTags.TryGetValue("building", out var grbBuildingValue);
                    if (grbBuildingValue == null)
                    {
                        Console.WriteLine("GRB Building value is null");
                    }
                    else if (osmBuildingValue != grbBuildingValue)
                    {
                        if (!grbBuildingValue.Equals("yes"))
                        {
                            Console.WriteLine(
                                $"Preferring OSM building-tag '{osmBuildingValue}' over the grb tag '{grbBuildingValue}'");
                        }

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
                    grbTags.RemoveKey("addr:housenumber");

                    var msg = "GRB thinks that this has number " +
                              (string.IsNullOrEmpty(grbHouseNumber) ? "no number" : grbHouseNumber);

                    Console.WriteLine(
                        $"GRB and OSM disagree over housenumber of {osmObj}. Putting a fixme instead: grb: {grbHouseNumber} != osm {osmHouseNumber}");
                    if (grbTags.ContainsKey("fixme"))
                    {
                        msg += ";" + grbTags.GetValue("fixme");
                    }

                    grbTags.AddOrReplace("fixme", msg);
                }
            }

            if (osmObj.Tags.TryGetValue("addr:street", out var osmStreet))
            {
                grbTags.TryGetValue("addr:street", out var grbStreet);
                if (!string.IsNullOrEmpty(grbStreet) && !osmStreet.Equals(grbStreet))
                {
                    grbTags.RemoveKey("addr:street");

                    var msg = "GRB thinks that this lays in street " + grbStreet;

                    Console.WriteLine(
                        $"GRB and OSM disagree over streetname of {osmObj}. Putting a fixme instead: grb: {grbStreet} != osm {osmStreet}");
                    if (grbTags.ContainsKey("fixme"))
                    {
                        msg += ";" + grbTags.GetValue("fixme");
                    }

                    grbTags.AddOrReplace("fixme", msg);
                }
            }


            try
            {
                foreach (var grbTag in grbTags)
                {
                    if (grbTag.Key.Contains("wrong"))
                    {
                        continue;
                    }

                    osmObj.AddNewTag(grbTag.Key, grbTag.Value);
                }

                cs.AddChange(osmObj);
            }
            catch (Exception e)
            {
                Console.WriteLine($"{osmObj}: {e.Message}: skipping match");
            }


            return true;
        }


        public static EasyChangeset Conflate(IEnumerable<ICompleteOsmGeo> osmStreamComplete,
            List<(Polygon, TagsCollection)> grbBuildings)
        {
            var cs = new EasyChangeset();
            var leftOvers = new List<(Polygon grbPoly, TagsCollection grbTags)>();

            var osmBuildings = new List<(Polygon, CompleteWay way)>();

            var count = 0;

            foreach (var osmGeo in osmStreamComplete)
            {
                if (!(osmGeo is CompleteWay w))
                {
                    continue;
                }

                if (!w.IsClosed())
                {
                    continue;
                }

                if (w.Tags == null)
                {
                    continue;
                }

                if (!w.Tags.TryGetValue("building", out _))
                {
                    continue;
                }


                osmBuildings.Add((ToPolygon(w), w));
            }

            Console.WriteLine($"Found {osmBuildings.Count} osm buildings");


            var dirty = new HashSet<CompleteWay>();

            foreach (var (grbPoly, grbTags) in grbBuildings)
            {
                var conflated = false;

                foreach (var (osmBuilding, osmTags) in osmBuildings)
                {
                    if (dirty.Contains(osmTags))
                    {
                        continue;
                    }

                    if (osmTags.Tags.TryGetValue("source:geometry:ref", out var oldRef))
                    {
                        // Already from GRB or conflated - but might have a wrong format!

                        continue;
                    }


                    if (!AttemptConflate(grbPoly, osmBuilding, grbTags, osmTags, cs))
                    {
                        continue;
                    }

                    count++;
                    conflated = true;
                    dirty.Add(osmTags);

                    continue;
                }

                if (!conflated)
                {
                    leftOvers.Add((grbPoly, grbTags));
                }
            }

            Console.WriteLine("Conflated " + count);

            return cs;
        }
    }
}