using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OsmBot.Download;
using OsmBot.WikipediaTools;

namespace OsmBot
{
    internal static class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://192.168.1.23:5000")
        //*
                ;
                /*/
                .UseUrls("http://178.116.193.137:5000"); //*/

        public static async Task Main(string[] args)
        {
            if (args[0].Equals("--api"))
            {
                Console.WriteLine("Starting api");

                CreateWebHostBuilder(args).Build().Run();

                return;
            }

            if (args[0].Equals("--conflate"))
            {
                Console.WriteLine("Usage: minLon maxLon minLat maxLat");
                var minLon = double.Parse(args[1]);
                var maxLon = double.Parse(args[2]);
                var minLat = double.Parse(args[3]);
                var maxLat = double.Parse(args[4]);
                ConflateBetween(minLon, maxLon, minLat, maxLat);
                return;
            }
        }

        private static void ConflateBetween(double minLon, double maxLon, double minLat, double maxLat)
        {
            var minLonint = (int) (minLon * 1000);
            var maxLonint = (int) (maxLon * 1000);
            var minLatint = (int) (minLat * 1000);
            var maxLatint = (int) (maxLat * 1000);

            Console.WriteLine("Conflation between: lon ");

            for (var lon = minLonint; lon < maxLonint; lon += 10)
            {
                for (var lat = minLatint; lat < maxLatint; lat += 25)
                {
                    var activeArea = Rect.FromGeoJson(
                        GenerateOutline(ThreeDigits(lon), ThreeDigits(lon + 10),
                            ThreeDigits(lat), ThreeDigits(lat + 25)));


                    var osmData = Osm.DownloadStream(activeArea);
                    var grb = GrbFetcher.DownloadAsGeojson(activeArea);

                    var grbPolies = GeoJson2Polies.Convert(grb);


                    var cs = Conflater.Conflate(osmData, grbPolies);
                    cs.WriteChangeTo($"Conflation{lon}-{lat}.osc");
                }
            }
        }

        private static string ThreeDigits(int coordinate)
        {
            var beforeDot = coordinate / 1000;
            var afterDot = coordinate % 1000;
            return $"{beforeDot}.{afterDot:000}";
        }


        private static string GenerateOutline(string left, string right, string bottom, string top)
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