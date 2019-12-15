using System.IO;
using OsmBot.Download;

namespace OsmBot
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var activeArea = Rect.FromGeoJson(File.ReadAllText("area.geojson"));

            var grb = GrbFetcher.DownloadAsGeojson(activeArea);

            var grbPolies = GeoJson2Polies.Convert(grb);
            var osmData = Osm.DownloadStream(activeArea);
            
            
            
            var cs = Conflater.Conflate(osmData, grbPolies);
            cs.WriteChangeTo("Conflation.osm");
        }
    }
}