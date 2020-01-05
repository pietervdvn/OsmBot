using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;
using OsmSharp.Tags;

namespace OsmBot.Download
{
    public static class GeoJson2Polies
    {
        private static readonly string[] _blacklist =
        {
            "osm_id", "size_grb_building", "detection_method", "auto_target_landuse", "size_source_landuse",
            "auto_building", "source:geometry:entity", "source:geometry:oidn", "source:geometry:uidn",
            "size_shared", "size_source_building", "HN_MAX", "HN_P99","H_DSM_MAX","H_DSM_P99","H_DTM_GEM","H_DTM_MIN"
        };

        public static List<(Polygon poly, TagsCollection)> Convert(string geojson)
        {
            var all = new List<(Polygon poly, TagsCollection)>();

            var jobj = JObject.Parse(geojson);
            foreach (var feature in (JArray) jobj["features"])
            {
                if (!feature["geometry"]["type"].Value<string>().Equals("Polygon"))
                {
                    throw new ArgumentException("Not a polygon!");
                }

                var allCoordinates = (JArray) feature["geometry"]["coordinates"];
                if (allCoordinates.Count > 1)
                {
                    // This is a multipolygon
                    // We don't support those
                    continue;
                }

                var coordinates = (JArray) feature["geometry"]["coordinates"][0];

                var points = new List<Coordinate>();
                foreach (var coordinate in coordinates)
                {
                    points.Add(new Coordinate(
                        coordinate[0].Value<double>(), coordinate[1].Value<double>()));
                }

                var poly = new Polygon(new LinearRing(points.ToArray()));

                var tags = new List<Tag>();
                var properties = (JObject) feature["properties"];

                var wrongSourceTag = properties["source:geometry:entity"].Value<string>() +
                             properties["source:geometry:oidn"].Value<string>() +
                             properties["source:geometry:uidn"].Value<string>();
                tags.Add(new Tag("source:geometry:ref:wrong", wrongSourceTag));
                var srcTag = properties["source:geometry:entity"].Value<string>() + "/" +
                                     properties["source:geometry:oidn"].Value<string>();

                tags.Add(new Tag("source:geometry:ref", srcTag));


                    foreach (var (key, value) in properties)
                    {
                        if (_blacklist.Contains(key))
                        {
                            continue;
                        }

                        tags.Add(new Tag(key, value.Value<string>()));
                    }

                all.Add((poly, new TagsCollection(tags)));
            }

            return all;
        }
    }
}