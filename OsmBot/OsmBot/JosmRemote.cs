using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace OsmBot
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class JosmRemote : ControllerBase
    {
        
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


    public class JsonTextFormatter : TextOutputFormatter
    {
        public JsonTextFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/javascript"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            if (context.Object is string s)
            {
                using (var stream = new StreamWriter(context.HttpContext.Response.Body))
                {
                    stream.Write(s);
                    return Task.CompletedTask;
                }
            }

            throw new Exception("Wut?");
        }
    }
}