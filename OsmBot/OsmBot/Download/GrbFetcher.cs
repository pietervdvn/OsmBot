using System;
using System.Net.Http;
using NetTopologySuite.Geometries;
using Newtonsoft.Json.Linq;

namespace OsmBot.Download
{
    public static class GrbFetcher
    {
        private static readonly HttpClient _client = new HttpClient();

        public static string DownloadAsGeojson(Rect r)
        {
            r = r.ConvertWgs84To900913();
            var url =
                $"https://betadata.grbosm.site/grb?bbox={r.Min.X},{r.Min.Y},{r.Max.X},{r.Max.Y}";
            Console.WriteLine("Downloading "+url);
            var response = _client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            var data = _client.GetStringAsync(url).Result;
            var geojson = JObject.Parse(data);
            foreach (var feature in (JArray) geojson["features"])
            {
                if (!feature["geometry"]["type"].Value<string>().Equals("Polygon"))
                {
                    throw new ArgumentException("Not a polygon!");
                }
                
                var oldCoordinates = (JArray) feature["geometry"]["coordinates"][0];
                var newCoordinates = new JArray();
                foreach (var oldCoordinate in oldCoordinates)
                { 
                    var newCoordinate =
                        Wgs84To900913.Convert900913ToWgs84(
                            new Point(
                            oldCoordinate[0].Value<double>(),
                            oldCoordinate[1].Value<double>()));
                    newCoordinates.Add(
                        new JArray(newCoordinate.X, newCoordinate.Y));
                }

                feature["geometry"]["coordinates"][0] = newCoordinates;
            }
            return geojson.ToString();
        }


    }
}