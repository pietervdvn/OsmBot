using System;
using OsmBot.Download;

namespace OsmBot
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var i = 0;
            var fullOutline = GenerateOutline(
                "3.164", "3.238", "51.162", "51.213");

            Console.WriteLine(fullOutline);
            for (var lon = 100; lon < 180; lon += 10)
            {
                for (var lat = 150; lat < 225; lat += 25)
                {
                    var activeArea = Rect.FromGeoJson(
                        GenerateOutline($"3.{lon}", $"3.{lon + 10}", $"51.{lat}", $"51.{lat + 25}")
                    );


                    var osmData = Osm.DownloadStream(activeArea);
                    var grb = GrbFetcher.DownloadAsGeojson(activeArea);

                    var grbPolies = GeoJson2Polies.Convert(grb);


                    var cs = Conflater.Conflate(osmData, grbPolies);
                    cs.WriteChangeTo($"Conflation{lon}-{lat}.osc");
                }
            }
        }


        public static string GenerateOutline(string left, string right, string bottom, string top)
        {
            return
                "{ \"type\": \"FeatureCollection\", \"features\": [ { \"type\": \"Feature\", \"properties\": {}, \"geometry\": { \"type\": \"Polygon\", \"coordinates\": [ [ " +
                $"[ {left}, {bottom}], " +
                $"[ {right}, {bottom}], " +
                $"[ {right}, {top}], " +
                $"[ {left}, {top}], " +
                $"[ {left}, {bottom}]" +
                " ] ] } } ]}\n";
        }
    }
}