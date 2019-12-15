using System.Net.Http;
// ReSharper disable UnusedMember.Global

namespace OsmBot.JOSM
{
    public static class JosmRemoteControl
    {
        private static readonly HttpClient _client = new HttpClient();

        public static void OpenJosmWithOsm(string left, string bottom, string right,string top)
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
    }
}