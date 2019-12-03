using System;
using System.IO;
using System.Linq;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using OsmSharp.Geo;
using OsmSharp.Streams;

namespace Wikipedia2Street
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Starting...");

          
           using (var osmStream = File.OpenRead(@"OSM.osm"))
           {
               var osmStreamComplete = new XmlOsmStreamSource(osmStream);

               using (var grbStream = File.OpenRead(@"GRB.osm"))
               {
                   var grbStreamComplete = new XmlOsmStreamSource(grbStream).ShowProgress();
                   
                   
                   var conflater = new Conflater(osmStreamComplete.ToComplete(), grbStreamComplete.ToComplete());

                   var cs = conflater.Conflate();
                   cs.WriteChangeTo("Conflation.osc");

               }
           }
        }
    }
}