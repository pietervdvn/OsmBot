using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Itinero.LocalGeo;

namespace OsmBot
{
    /// <summary>
    /// (Reverse) Geocodes stuff with nominatim
    /// </summary>
    public class Geocoder
    {
        static List<string> preferedOrder = new List<string>
        {
            "amenity",
            "address",
            "building"
        };


        public static async Task<Coordinate?> Geocode(string street, string number, string municipality)
        {
            var response = await httpClient.GetAsync(
                "https://nominatim.openstreetmap.org/search.php?" +
                "format=json&" +
                "city=Sint-Kruis&" +
                "postal-code=8310&" +
                "street=" + number + " " + street);
            var txt = (await response.Content.ReadAsStringAsync());
            Thread.Sleep(1000); // AT most 1 request per second as required by nominatim

            var resp = JToken.Parse(txt);
            if (!resp.Any())
            {
                Console.WriteLine($"No result found for {street} {number}, {municipality}");
                return null;
            }

            JToken best = null;
            var catIndex = int.MaxValue;

            foreach (var r in resp)
            {
                var cls = r["class"].ToString();
                var i = preferedOrder.IndexOf(cls.ToLower());
                if (i < 0)
                {
                    if (!cls.Equals("highway"))
                    {
                        Console.WriteLine(
                            $"Warning: unknown class {cls} for {street} {number}. Address might not be known yet, or missing category");
                    }

                    continue;
                }

                if (catIndex <= i)
                {
                    continue;
                }

                catIndex = i;
                best = r;
            }


            if (best == null)
            {
                Console.WriteLine("NO PROPER ADDRESS FOUND!");
                return null;
            }

            var lat = best["lat"].Value<float>();
            var lon = best["lon"].Value<float>();

            return new Coordinate(lat, lon);
        }

        private static async Task<(string municipality, string street, string number)?> ReverseGeocode(string url)
        {
            var response = await httpClient.GetAsync(url);
            var txt = (await response.Content.ReadAsStringAsync());
            Thread.Sleep(1000); // AT most 1 request per second as required by nominatim

            var resp = JToken.Parse(txt);
            if (!resp.Any())
            {
                Console.WriteLine($"No result found");
                return null;
            }

            var addr = resp["address"];
            return (addr.Value<string>("city_district"), addr.Value<string>("road"), addr.Value<string>("house_number"));
        }
        
        public static async Task<(string municipality, string street, string number)?> ReverseGeocode(Coordinate c)
        {
            var url = "https://nominatim.openstreetmap.org/reverse?format=json&" +
                      $"lat={c.Latitude}&lon={c.Longitude}" +
                      "&zoom=18&addressdetails=1";
            return await ReverseGeocode(url);
        }


        private static readonly HttpClient httpClient = GenClient();

        private static HttpClient GenClient()
        {
            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Add("User-Agent", "pietervdvn_at_posteo.net");
            return cl;
        }
    }
}