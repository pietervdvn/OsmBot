using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OsmSharp;

namespace OsmBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class WikiDataApply : ControllerBase
    {
        /// <summary>
        /// Gives possible entries
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<string>> Get(string wikidata, string ids)
        {
            Console.WriteLine(ids + " --> " + wikidata);


            var objects = new List<OsmGeo>();
            foreach (var id in ids.Split(","))
            {
                var obj = OsmApi.Singleton.Get(id).Result;
                if (obj.Tags.TryGetValue("name:etymology:wikidata", out var oldWikidata))
                {
                    if (!oldWikidata.Equals(wikidata))
                    {
                        return
                            $"ERROR: <a href='https://www.openstreetmap.org/{id}>This way already contains a different wikidata</a>";
                    }
                }
                else
                {
                    obj.Tags.Add("name:etymology:wikidata", wikidata);
                    Console.WriteLine($"Added etymology {wikidata} to {id}");
                    objects.Add(obj);
                }
            }

            await OsmApi.CreateChange(objects);

            return "OK";
        }
    }
}