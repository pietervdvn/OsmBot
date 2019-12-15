using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using OsmBot.Download;
using OsmSharp.Complete;
using OsmSharp.Streams;

namespace OsmBot
{
    /// <summary>
    /// Downloads an area from OSM
    /// </summary>
    public static class Osm
    {
        private static readonly HttpClient _client = new HttpClient();

        public static string Download(Rect r)
        {
            var url = $"https://osm.org/api/0.6/map?bbox={r.Min.X},{r.Min.Y},{r.Max.X},{r.Max.Y}";
            Console.WriteLine("Downloading " + url);
            var response = _client.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return _client.GetStringAsync(url).Result;
        }

        public static IEnumerable<ICompleteOsmGeo> DownloadStream(Rect r)
        {
            var data = Download(r);
            return new XmlOsmStreamSource(GenerateStreamFromString(data)).ToComplete();
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
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