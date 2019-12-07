using System;
using System.IO;
using System.Net.Http;

namespace OsmBot
{
    /// <summary>
    /// Downloads an area from OSM
    /// </summary>
    public static class Osm
    {
        private static readonly HttpClient _client = new HttpClient();

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

        public static string Download(double left, double bottom, double right, double top)
        {
            var url = $"https://osm.org/api/0.6/map?bbox={left},{bottom},{right},{top}";
            Console.WriteLine(url);
            var response = _client.GetAsync("http://www.contoso.com/").Result;
            response.EnsureSuccessStatusCode();
            return _client.GetStringAsync(url).Result;
        }



        public static void OpenJosmWithOSM(string left, string bottom, string right,string top)
        {
            _client.GetAsync(
                $"http://127.0.0.1:8111/load_and_zoom?new_layer=true&layer_name=OSM&" +
                $"left={left}&right={right}&top={top}&bottom={bottom}");

        }
        
        public static void OpenJosmZoomTo(string left, string bottom, string right,string top)
        {
            _client.GetAsync(
                $"http://127.0.0.1:8111/zoom?new_layer=true&layer_name=OSM&" +
                $"left={left}&right={right}&top={top}&bottom={bottom}");

        }
        
        public static void OpenJosmFile(string path)
        {
            _client.GetAsync(
                $"http://127.0.0.1:8111/open_file?filename={path}");

        }
     
        public static void Upload(string changesetPath, string comment)
        {
            // var testEndpoint = "https://master.apis.dev.openstreetmap.org";
            var endpoint = "https://openstreetmap.org";



            var urlCreate = endpoint + "/api/0.6/changeset/create";

            var content = new StringContent(
                $"<osm>" +
                $"<changeset>" +
                $"<tag k=\"created_by\" v=\"Pietervdvn OSM Bot 0.1\"/>" +
                $"<tag k=\"comment\" v=\"{comment}\"/>" +
                $"</changeset>" +
                $"</osm>"
            );

            var openingHttp = _client.PutAsync(urlCreate, content).Result;

            if (!openingHttp.IsSuccessStatusCode)
            {
                Console.WriteLine("Opening changeset failed: " + openingHttp.ReasonPhrase);
                return;
            }

            var id = long.Parse(openingHttp.Content.ReadAsStringAsync().Result);

            Console.WriteLine($"Opened changeset {id}, uploading now");

            var urlPost = endpoint + $"/api/0.6/changeset/{id}/upload";

            Console.WriteLine(urlPost);
            var cs = File.ReadAllText(changesetPath);
            cs = cs.Replace("$$$", "" + id);
            var resp = _client.PostAsync(urlPost, new StringContent(cs)).Result;
            if (!resp.IsSuccessStatusCode)
            {
                Console.WriteLine("UPLOAD FAILED");
                Console.WriteLine(resp);
                return;
            }

            Console.WriteLine("Upload successful, closing now");
            var urlClose = endpoint + $"/api/0.6/changeset/{id}/close";
            _client.PutAsync(urlClose, new StringContent(""));
        }
    }


}