using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using OsmBot.WikipediaTools;

namespace OsmBot.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ProducesResponseType(200)]
    public class WikiDataSearch : ControllerBase
    {
        /// <summary>
        /// Gives possible entries
        /// </summary>
        [HttpGet]
        public ActionResult<string> Get(string id)
        {
            var tags = OsmApi.Singleton.Get(id).Result.Tags;
            if (tags.TryGetValue("name:etymology:wikidata", out var wikidata))
            {
                return
                    $"Already matched with <a href='https://www.wikidata.org/wiki/{wikidata}'>{wikidata}</a>. Move the map around to change the colours (this might lag a few minutes)";
            }

            if (tags.TryGetValue("name:nl", out var streetname))
            {
            }
            else if (!tags.TryGetValue("name", out streetname))
            {
                return "This object has no name";
            }

            var name = Etymology.DropStreetOfName(streetname);
            Console.WriteLine("Searching for "+streetname);
            var wikidataEntries = Etymology.Singleton.FetchEtymologyFor(name).Result;

            var response = "The name <b>" + name + "</b> can match to the following wikidata entries:<br/>";

            response += "<table>" +
                        "<tr>" +
                        "<th>Id</th>" +
                        "<th>Label</th>" +
                        "<th>Description</th>" +
                        "<th>Is a</th>" +
                        "</tr>" +
                        string.Join("\n", wikidataEntries.Select((tpl) =>
                            $"<tr>" +
                            $"<td><a target='_blank' href='https://www.wikidata.org/wiki/{tpl.id}'>{tpl.id}</a></td>" +
                            $"<td>{tpl.label}</td><" +
                            $"td>{tpl.description}</td>" +
                            $"<td>{tpl.instanceOf}</td>" +
                            $"<td><button type='button' id=\"btn_{tpl.id}_{streetname.Replace("\"","")}\">Select this entry</button></td></tr>")) +
                        "</table>";

            return response;
        }
    }
}