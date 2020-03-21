using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Itinero.LocalGeo;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OsmBot.Download;

namespace OsmBot
{
    internal static class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
             //   .UseUrls("http://192.168.1.23:5000")
        /*
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


            var ruilkastjes = new Dictionary<string, Coordinate>
            {
                {"Little Free Library Sanderkwartier", new Coordinate(51.1985248f, 3.2037440f)},
                {"Stubbekwartier-buurtbibliotheek", new Coordinate(51.2179199f, 3.2154662f)},
                {"Little Free Library Hoeve Hangerijn", new Coordinate(51.2003035f, 3.2671270f)},
                {"Little Free Library Sint-Anna", new Coordinate(51.2123460f, 3.2327525f)},
                {"Little Free Library Sint-Lodewijks", new Coordinate(51.2163913f, 3.2063610f)},
                {"Little Free Library Hertsvelde-West", new Coordinate(51.2057076f, 3.1962513f)},
                {"Boekenruilkast Moutstraat", new Coordinate(51.2037968f, 3.1862056f)},
                {"Minibib Sint-Michiels", new Coordinate(51.1842679f, 3.1907591f)},
                {"Boekenruilkastje Werfplein", new Coordinate(51.2185334f, 3.2188596f)},
                {"Ruilkast Zagersweg", new Coordinate(51.2302709f, 3.2431685f)},
                {"Boekenruilkast", new Coordinate(51.1908646f, 3.2439454f)},
                {"MoniqueBib", new Coordinate(51.1845721f, 3.2098215f)},
                {"Minibib", new Coordinate(51.1821803f, 3.2223278f)},
                {"Boekenruilkast Sint-Hubertus", new Coordinate(51.2046194f, 3.1845657f)},
                {"Boekenruilkast Zandstraat", new Coordinate(51.1998555f, 3.1768209f)},
                {"Minibieb", new Coordinate(51.2079878f, 3.2882667f)},
                {"Ruilkast", new Coordinate(51.2149122f, 3.2740142f)},
                {"Mini Bib", new Coordinate(51.1811363f, 3.2401819f)},
                {"Ruilbib 't Reitje", new Coordinate(51.2156570f, 3.2245804f)}
            };


            foreach (var kv in ruilkastjes)
            {
                var k = kv.Key;
                var c = kv.Value;

                var (city, street, number) = (await Geocoder.ReverseGeocode(c)).Value;

                Console.WriteLine($"{k}, {city}, {street} {number}");
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