using System;
using System.IO;
using System.Net.Http;

namespace Wikipedia2Street
{
    /// <summary>
    /// Downloads an area from OSM
    /// </summary>
    public class AreaDownloaderOSM
    {
        static readonly HttpClient _client = new HttpClient();

        public static void DownloadTo(string path,
            string left, string bottom, string right, string top)
        {
            var url = $"https://osm.org/api/0.6/map?bbox={left},{bottom},{right},{top}";
            Console.WriteLine(url);
            var response = _client.GetAsync("http://www.contoso.com/").Result;
            response.EnsureSuccessStatusCode();
            var responseBody = _client.GetStringAsync(url).Result;
            File.WriteAllText(path, responseBody);
        }
    }
}