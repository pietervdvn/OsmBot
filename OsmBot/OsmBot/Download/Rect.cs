using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace OsmBot.Download
{
    public struct Rect
    {
        public Point Min, Max;

        public Rect(Point min, Point max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// SPlit the rectangle in smaller pieces of ~0.001 degree
        /// </summary>
        /// <returns></returns>
        public List<Rect> Split()
        {
            var all = new List<Rect>();

            var minLat = (int) Math.Floor(Min.Y * 1000);
            var minLon = (int) Math.Floor(Min.X * 1000);
            var maxLat = (int) Math.Ceiling(Max.Y * 1000);
            var maxLon = (int) Math.Ceiling(Min.X * 1000);

            for (var x = minLon; x <= maxLon; x++)
            {
                for (var y = minLat; y < maxLat; y++)
                {
                    all.Add(new Rect(
                        new Point(x / 1000.0, y / 1000.0),
                        new Point((x + 1) / 1000.0, (y + 1) / 1000.0)
                    ));
                }
            }

            return all;
        }

        public static Rect FromGeoJson(string geoJsonContents)
        {
            var jobj = JObject.Parse(geoJsonContents);
            var coordinates = jobj["features"][0]["geometry"]["coordinates"][0];
            var minLat = Double.MaxValue;
            var maxLat = Double.MinValue;
            var minLon = Double.MaxValue;
            var maxLon = Double.MinValue;
            foreach (var coordinate in coordinates)
            {
                var lon = coordinate[0].Value<double>();
                var lat = coordinate[1].Value<double>();
                if (lon > maxLon)
                {
                    maxLon = lon;
                }

                if (lon < minLon)
                {
                    minLon = lon;
                }

                if (lat > maxLat)
                {
                    maxLat = lat;
                }

                if (lat < minLat)
                {
                    minLat = lat;
                }
            }

            return new Rect(new Point(minLon, minLat), new Point(maxLon, maxLat));
        }
    }
}