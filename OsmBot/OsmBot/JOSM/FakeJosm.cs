using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace OsmBot.JOSM
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class FakeJosm : ControllerBase
    {
        
        public static void StartFakeJosm(){
            const int l = 5000000;
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
        
        [HttpGet("/load_data")]
        public ActionResult<string> Get(string data)
        {
            
            Console.WriteLine("Received data");
            var i = 0;
            var path = "GRB"+i+".osm";
            while (System.IO.File.Exists(path))
            {
                i++; path = "GRB"+i+".osm";
            }
            System.IO.File.WriteAllText(path, data);
            Console.WriteLine("Saved "+path);
            return "{}";
        }

        [HttpGet("/version")]
        public ActionResult<string> GetVersion()
        {
            Console.WriteLine("Got version");
            var data =
                "{\"protocolversion\" :  {\"major\" : 1,\"minor\" : 8}, " +
                "\"application\" : \"JOSM RemoteControl\"}";
            return data;
        }
    }
}