using System;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Wikipedia2Street
{
    public class GrbFetcher
    {
        static readonly HttpClient _client = new HttpClient();

        public static void DownloadTo(string path,
            string left, string bottom, string right, string top)
        {
            var url =
                "https://betadata.grbosm.site/grb?bbox=358898.98956496,6660151.7535377,360520.88775868,6660914.9294917";

            // var url = $"https://grb?bbox={left},{bottom},{right},{top}";
            var response = _client.GetAsync("http://www.contoso.com/").Result;
            response.EnsureSuccessStatusCode();
            var geojsonLambert72 = JObject.Parse(_client.GetStringAsync(url).Result);
            foreach (var feature in (JArray) geojsonLambert72["features"])
            {
                if (!feature["geometry"]["type"].Value<string>().Equals("Polygon"))
                {
                    throw new ArgumentException("Not a polygon!");
                }
                
                var oldCoordinates = (JArray) feature["geometry"]["coordinates"][0];
                Console.WriteLine(feature);
                var newCoordinates = new JArray();
                foreach (var oldCoordinate in oldCoordinates)
                { Console.WriteLine(oldCoordinate);

                    var newCoordinate = lambert72toWGS84(oldCoordinate[1].Value<double>(), oldCoordinate[0].Value<double>());
                    newCoordinates.Add(newCoordinate);
                }

                feature["geometry"]["coordinates"] = newCoordinates;
            }
            File.WriteAllText(path, geojsonLambert72.ToString());
        }


        static JArray lambert72toWGS84(double x, double y)
        {
            double newLongitude, newLatitude;

            const double n = 0.77164219;
            const double F = 1.81329763;
            const double thetaFudge = 0.00014204;
            const double e = 0.08199189;
            const int a = 6378388;
            const int xDiff = 149910;
            const int yDiff = 5400150;
            const double theta0 = 0.07604294;

            var xReal = xDiff - x;
            var yReal = yDiff - y;

            var rho = Math.Sqrt(xReal * xReal + yReal * yReal);
            var theta = Math.Atan(xReal / -yReal);

            newLongitude = (theta0 + (theta + thetaFudge) / n) * 180 / Math.PI;
            newLatitude = 0;

            for (var i = 0; i < 5; ++i)
            {
                newLatitude = (2 * Math.Atan(Math.Pow(F * a / rho, 1 / n) *
                                             Math.Pow((1 + e * Math.Sin(newLatitude)) / (1 - e * Math.Sin(newLatitude)),
                                                 e / 2))) - Math.PI / 2;
            }

            newLatitude *= 180 / Math.PI;
            return new JArray {newLongitude, newLatitude};
        }
    }
}