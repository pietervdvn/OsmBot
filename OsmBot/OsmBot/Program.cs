using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using OsmSharp.IO.PBF;
using OsmSharp.Streams;
using Way = OsmSharp.Way;

namespace OsmBot
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            bool ranConflation = false;
            foreach (var file in new DirectoryInfo(".").GetFiles("GRB*.osm"))
            {
                var path = file.FullName;
                var i = int.Parse(path.Substring(0, path.Length - 4).Substring(
                    file.DirectoryName.Length + 4));
                Console.WriteLine("Handling " + file.FullName + " " + i);
                Conflater.RunConflation(File.ReadAllText(file.FullName), i);
                ranConflation = true;
            }

            if (ranConflation)
            {
                return;
            }

            int l = 5000000;

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestLineSize = l;
                    options.Limits.MaxRequestBufferSize = l;
                    options.Limits.MaxRequestHeadersTotalSize = l;
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseUrls("http://127.0.0.1:8111")
                .UseApplicationInsights()
                .Build();

            host.Run();
        }


        private static void Manual()
        {
        }
    }
}