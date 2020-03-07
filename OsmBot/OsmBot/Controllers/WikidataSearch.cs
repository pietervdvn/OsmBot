using System;
using Microsoft.AspNetCore.Mvc;
using OsmBot.Models;
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
        public ActionResult<WikidataSearchResponse> Get(string id)
        {
            var tags = OsmApi.Singleton.Get(id).Result.Tags;
            if (tags.TryGetValue("name:etymology:wikidata", out var wikidata))
            {
                return new WikidataSearchResponse(alreadyMatched:true);
            }

            if (!tags.TryGetValue("name", out var streetname))
            {
                return new WikidataSearchResponse(alreadyMatched:true);
            }

            var name = Etymology.DropStreetOfName(streetname);
            Console.WriteLine("Searching for " + streetname);
            var wikidataEntries = Etymology.Singleton.FetchEtymologyFor(name).Result;
            return new WikidataSearchResponse(wikidataEntries);
        }
    }
}